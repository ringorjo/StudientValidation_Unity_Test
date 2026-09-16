using System.IO;
using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    [SerializeField] private GradeValidationConfig _gradeValidationConfig;

    private StudentRepository _studentRepository;
    private GradeValidationService _gradeValidationService;

    private void Awake()
    {
        string jsonPath = Path.Combine(Application.streamingAssetsPath, "estudiantes.json");

        _studentRepository = new StudentRepository(new JsonUtilityDeserializer(), jsonPath);
        _studentRepository.Register();

        _gradeValidationService = new GradeValidationService(_gradeValidationConfig);
        _gradeValidationService.Register();

        _studentRepository.Load();
    }

    private void OnDestroy()
    {
        _studentRepository.Unregister();
        _gradeValidationService.Unregister();
    }
}
