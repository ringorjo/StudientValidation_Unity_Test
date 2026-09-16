using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class StudentRepositoryTests
{
    [Test]
    public void Load_TodosLosRegistrosValidos_CargaTodos()
    {
        var dto = new StudentListJsonDto
        {
            estudiantes = new[]
            {
                new StudentJsonDto { nombre = "Ana", apellido = "Ruiz", codigo = "1", correo = "a@x.com", notaFinal = 4.0f },
                new StudentJsonDto { nombre = "Bob", apellido = "Lopez", codigo = "2", correo = "b@x.com", notaFinal = 2.0f }
            }
        };
        var repo = new StudentRepository(new FakeJsonDeserializer(dto), "unused.json", _ => "{}");
        IReadOnlyList<StudentRecord> result = null;
        repo.OnStudentsLoaded += students => result = students;

        repo.Load();

        Assert.AreEqual(2, result.Count);
    }

    [Test]
    public void Load_RegistroConCampoFaltante_SeDescartaYContinua()
    {
        var dto = new StudentListJsonDto
        {
            estudiantes = new[]
            {
                new StudentJsonDto { nombre = "Ana", apellido = "Ruiz", codigo = "1", correo = "a@x.com", notaFinal = 4.0f },
                new StudentJsonDto { nombre = "", apellido = "SinNombre", codigo = "2", correo = "b@x.com", notaFinal = 2.0f }
            }
        };
        var repo = new StudentRepository(new FakeJsonDeserializer(dto), "unused.json", _ => "{}");
        IReadOnlyList<StudentRecord> result = null;
        repo.OnStudentsLoaded += students => result = students;

        repo.Load();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("1", result[0].Codigo);
    }

    [Test]
    public void Load_NotaFueraDeRango_SeDescartaYContinua()
    {
        var dto = new StudentListJsonDto
        {
            estudiantes = new[]
            {
                new StudentJsonDto { nombre = "Ana", apellido = "Ruiz", codigo = "1", correo = "a@x.com", notaFinal = 6.5f }
            }
        };
        var repo = new StudentRepository(new FakeJsonDeserializer(dto), "unused.json", _ => "{}");
        IReadOnlyList<StudentRecord> result = null;
        repo.OnStudentsLoaded += students => result = students;

        repo.Load();

        Assert.AreEqual(0, result.Count);
    }

    [Test]
    public void Load_JsonNoParseable_NoLanzaYDevuelveListaVacia()
    {
        var repo = new StudentRepository(new FakeJsonDeserializer(null, shouldSucceed: false), "unused.json", _ => "{}");
        IReadOnlyList<StudentRecord> result = null;
        repo.OnStudentsLoaded += students => result = students;

        // StudentRepository.Load() intentionally logs an error (via Debug.LogError) when
        // parsing fails; Unity Test Framework fails a test on any unexpected error log
        // unless it is expected via LogAssert, so we expect it here.
        LogAssert.Expect(LogType.Error, new Regex(".*"));
        Assert.DoesNotThrow(() => repo.Load());
        Assert.AreEqual(0, result.Count);
    }

    [Test]
    public void Load_ArchivoNoExiste_NoLanzaYDevuelveListaVacia()
    {
        var repo = new StudentRepository(new JsonUtilityDeserializer(), "ruta/que/no/existe.json");
        IReadOnlyList<StudentRecord> result = null;
        repo.OnStudentsLoaded += students => result = students;

        // StudentRepository.Load() intentionally logs an error (via Debug.LogError) when
        // the file cannot be read; Unity Test Framework fails a test on any unexpected
        // error log unless it is expected via LogAssert, so we expect it here.
        LogAssert.Expect(LogType.Error, new Regex(".*"));
        Assert.DoesNotThrow(() => repo.Load());
        Assert.AreEqual(0, result.Count);
    }
}
