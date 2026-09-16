# Panel de Notas

Aplicación Unity (Unity 6000.4.8f1) que permite revisar el listado de estudiantes de un curso, validar si cada uno está aprobado o reprobado según su nota final, y practicar esa clasificación mediante una interacción de drag & drop. Los datos de los estudiantes se cargan desde un archivo JSON externo (`StreamingAssets/estudiantes.json`), y las reglas de aprobación se configuran mediante un `ScriptableObject` sin tocar código.

Este documento describe la **arquitectura del proyecto**: cómo están organizadas las capas, qué responsabilidad tiene cada módulo y cómo se comunican entre sí a través de interfaces.

## Assembly

El código de gameplay vive en el assembly `PanelDeNotas` (`Assets/Scripts/PanelDeNotas.asmdef`), separado del assembly de tests `PanelDeNotas.EditModeTests` (`Assets/Tests/EditMode`). Esto mantiene el código de producción libre de dependencias de testing y acelera la recompilación en el editor.

## Visión general de capas

El proyecto sigue una arquitectura en capas con **inversión de dependencias**: las capas superiores dependen de interfaces, no de implementaciones concretas, y esas implementaciones se resuelven en tiempo de ejecución mediante un Service Locator.

```
Assets/Scripts/
├── Core/      → infraestructura transversal (Service Locator, pooling, contrato de servicio)
├── Config/    → datos de configuración como ScriptableObjects
├── Data/      → acceso y transformación de datos (repositorio + serialización)
├── Domain/    → reglas de negocio (validación de notas)
└── UI/        → presentación (vistas, presenters, controllers de pantalla)
```

El flujo de dependencias va siempre "hacia adentro": `UI → Domain/Data (vía interfaces) → Core`. Ninguna capa de dominio o datos conoce a la UI, y ninguna implementación concreta es referenciada directamente fuera de donde se construye (`Bootstrapper`).

```
UI (Controllers, Views, Presenters)
        │  depende de
        ▼
IStudentRepository / IGradeValidationService   (Domain/Data — interfaces)
        │  implementadas por
        ▼
StudentRepository / GradeValidationService     (Domain/Data — implementaciones)
        │  usa
        ▼
IJsonDeserializer, GradeValidationConfig       (Data/Serialization, Config)
        │  registradas/resueltas vía
        ▼
ServiceLocator                                  (Core)
```

## Core: infraestructura transversal

### `IService`
Contrato mínimo que deben cumplir los servicios de la aplicación: `Register()` y `Unregister()`. Estandariza el ciclo de vida de alta/baja en el `ServiceLocator`, independientemente de qué haga el servicio.

### `ServiceLocator`
Singleton simple (`ServiceLocator.Instance`) que actúa como contenedor de servicios, indexados por tipo de interfaz (`typeof(T).Name`). Expone `Register`, `Unregister`, `Get` y `TryGet`. Es el único punto de acoplamiento entre las capas: la UI no instancia `StudentRepository` ni `GradeValidationService` directamente, solo pide `ServiceLocator.Instance.Get<IStudentRepository>()`. Esto permite:

- Sustituir implementaciones (por ejemplo, en tests) sin tocar el código consumidor.
- Mantener la UI ajena a los detalles de construcción (rutas de archivo, deserializadores, configuración).

### `ObjectPool<T>`
Pool genérico de componentes (`where T : Component`) con `Get`, `Release` y `ReleaseAll`. Se usa para evitar instanciar/destruir GameObjects repetidamente cada vez que se recargan filas de estudiantes o tarjetas de drag & drop (`RosterScreenController`, `DragDropScreenController`).

### `Bootstrapper`
`MonoBehaviour` de arranque, colocado en la escena, responsable de construir las implementaciones concretas y registrarlas en el `ServiceLocator`:

```
StudentRepository(new JsonUtilityDeserializer(), jsonPath).Register();
GradeValidationService(_gradeValidationConfig).Register();
```

Es la **composition root** del proyecto: el único lugar donde se conocen las clases concretas de Data/Domain. En `OnDestroy` desregistra ambos servicios.

## Config: datos configurables sin código

`GradeValidationConfig` es un `ScriptableObject` (`[CreateAssetMenu]`) que expone umbral de aprobación, etiquetas ("Aprobado"/"Reprobado") y colores asociados. Vive como asset en `Assets/ScriptableObjects/GradeValidationConfig.asset` y se inyecta al `Bootstrapper` vía `[SerializeField]`. Cambiar las reglas de negocio (nota mínima, textos, colores) no requiere recompilar código.

## Data: acceso y transformación de datos

| Elemento | Rol |
|---|---|
| `IStudentRepository` (extiende `IService`) | Contrato de acceso a estudiantes: `Load()`, `CurrentStudents`, evento `OnStudentsLoaded`. |
| `StudentRepository` | Implementación: lee el JSON desde disco (`Func<string,string>` inyectable para testear sin IO real), lo deserializa vía `IJsonDeserializer` y valida/convierte cada entrada (`StudentJsonDto` → `StudentRecord`), descartando registros inválidos con logs de advertencia. |
| `IJsonDeserializer` / `JsonUtilityDeserializer` | Abstrae la librería de deserialización JSON (`bool TryDeserialize<T>(...)`), permitiendo reemplazar `JsonUtility` por otra implementación (o un fake en tests) sin tocar el repositorio. |
| `StudentJsonDto` / `StudentListJsonDto` | DTOs que reflejan literalmente el esquema del JSON externo (`nombre`, `apellido`, `codigo`, `correo`, `notaFinal`). |
| `StudentRecord` | Modelo de dominio interno, desacoplado del formato JSON (nombres de propiedades en inglés/PascalCase, con `FullName` calculado). |

La separación **DTO vs. modelo de dominio** evita que un cambio en el formato del JSON externo se propague a toda la aplicación: solo `StudentRepository.TryConvert` conoce el mapeo entre ambos.

## Domain: reglas de negocio

| Elemento | Rol |
|---|---|
| `IGradeValidationService` (extiende `IService`) | Contrato de las reglas de aprobación: `Passes`, `MatchesMarkedStatus`, además de etiquetas/colores expuestos para la UI. |
| `GradeValidationService` | Implementación que delega los valores configurables en `GradeValidationConfig` y aplica la regla `FinalGrade >= PassingThreshold`. |

Esta capa no tiene ninguna referencia a Unity UI ni a MonoBehaviours de presentación: es lógica pura, fácil de testear de forma aislada (ver `GradeValidationServiceTests`).

## UI: presentación

La UI está organizada en tres subcapas con responsabilidades distintas, siguiendo un patrón **View / Presenter / Controller**:

### Views (`UI/Roster`, `UI/DragDrop`, `UI/Common`)
`MonoBehaviour`s "tontas" que solo exponen métodos para pintar datos (`SetData`, `SetAvatar`, `ShowResult`, etc.) y eventos de UI crudos (`OnToggleChanged`, `OnItemDropped`). No conocen `StudentRecord` en términos de reglas de negocio, solo reciben strings/colores/floats ya resueltos.

- `StudentRowView`: fila del roster con nombre, nota, toggle de aprobado/reprobado y resaltado de discrepancia.
- `StudentDragItem`: tarjeta arrastrable (`IBeginDragHandler`/`IDragHandler`/`IEndDragHandler`) que se vincula a un `StudentRecord`.
- `DropZone`: zona de destino (`IDropHandler`) que emite `OnItemDropped(item, isApprovedZone)`.
- `ResultBannerView`, `StudentReferenceRowView`: componentes de apoyo reutilizables.

### Presenters (`UI/Roster`)
`StudentRowPresenter` es una clase **no-MonoBehaviour** que actúa de intermediario entre un `StudentRowView` y un `StudentRecord`: bindea datos a la vista, escucha sus eventos y mantiene el estado de interacción (`MarkedAsApproved`). Mantener el presenter fuera de Unity (sin heredar `MonoBehaviour`) permite testearlo o razonar sobre él sin depender del ciclo de vida de GameObjects.

### Screen Controllers (`UI/Screens`)
Orquestan una pantalla completa: resuelven los servicios desde el `ServiceLocator`, gestionan el pool de vistas, reaccionan a eventos y aplican las reglas de negocio (a través de las interfaces de Domain, nunca de las implementaciones).

- `RosterScreenController`: carga estudiantes (`IStudentRepository.Load()`), genera una fila por estudiante vía pool + presenter, y valida las clasificaciones marcadas contra `IGradeValidationService`.
- `DragDropScreenController`: genera tarjetas arrastrables, escucha los `DropZone` de "aprobado"/"reprobado" y verifica el resultado al pulsar "Verificar".
- `UIScreenManager`: alterna la visibilidad entre la pantalla de roster y la de drag & drop (no contiene lógica de negocio).

## Flujo de ejecución típico

1. `Bootstrapper.Awake()` construye `StudentRepository` y `GradeValidationService` y los registra en `ServiceLocator`.
2. `UIScreenManager` muestra la pantalla de roster.
3. `RosterScreenController.Start()` obtiene `IStudentRepository`/`IGradeValidationService` del `ServiceLocator`, se suscribe a `OnStudentsLoaded` y llama a `Load()`.
4. `StudentRepository` lee `estudiantes.json`, lo deserializa con `IJsonDeserializer`, valida cada entrada y dispara `OnStudentsLoaded` con la lista de `StudentRecord` válidos.
5. El controller crea una `StudentRowView` (desde el pool) y un `StudentRowPresenter` por estudiante, usando las etiquetas/colores de `IGradeValidationService`.
6. El usuario marca cada fila como aprobado/reprobado (o los arrastra a una `DropZone` en la pantalla de drag & drop) y pulsa "Validar"/"Verificar".
7. El controller compara cada marca contra `IGradeValidationService.MatchesMarkedStatus` y muestra el resultado en `ResultBannerView`.

## Formato de datos externos

`Assets/StreamingAssets/estudiantes.json` contiene un objeto con la clave `estudiantes`, un arreglo de objetos con: `nombre`, `apellido`, `codigo`, `correo`, `notaFinal` (rango válido `0`–`5`). Los registros con campos vacíos o nota fuera de rango se descartan durante la carga (`StudentRepository.TryConvert`) y quedan registrados como warnings en consola.

## Tests

`Assets/Tests/EditMode` (assembly `PanelDeNotas.EditModeTests`) cubre las capas independientes de Unity/MonoBehaviour:

- `StudentRepositoryTests`: carga y validación de JSON, incluyendo entradas inválidas.
- `JsonUtilityDeserializerTests`: comportamiento del deserializador ante JSON válido/ inválido.
- `FakeJsonDeserializer`: doble de test para `IJsonDeserializer`, usado para aislar `StudentRepository` de `JsonUtility`.
- `GradeValidationServiceTests`: reglas de aprobación/reprobación.
- `ObjectPoolTests`: comportamiento de alta/baja de instancias del pool genérico.

La combinación de interfaces (`IJsonDeserializer`, `IStudentRepository`, `IGradeValidationService`) más inyección de dependencias por constructor es lo que permite testear `StudentRepository` y `GradeValidationService` en EditMode, sin necesidad de una escena cargada.

## Principios de diseño aplicados

- **Inversión de dependencias**: la UI depende de `IStudentRepository`/`IGradeValidationService`, nunca de las clases concretas.
- **Composition root único**: solo `Bootstrapper` conoce las implementaciones concretas y las conecta.
- **Separación DTO/modelo de dominio**: el formato del JSON externo no contamina el resto del código.
- **Vistas sin lógica de negocio**: las `View` solo pintan datos primitivos; las reglas viven en `Domain` y se orquestan desde los `Screen Controllers`.
- **Configuración fuera del código**: umbrales, etiquetas y colores se editan como asset (`ScriptableObject`), no como constantes en código.
- **Reutilización de GameObjects**: `ObjectPool<T>` evita instanciar/destruir filas y tarjetas en cada recarga.
