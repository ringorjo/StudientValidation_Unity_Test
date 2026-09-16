using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StudentRepository : IStudentRepository
{
    private const float MinNota = 0f;
    private const float MaxNota = 5f;

    private readonly IJsonDeserializer _deserializer;
    private readonly string _jsonFilePath;
    private readonly Func<string, string> _readAllText;
    private readonly List<StudentRecord> _currentStudents = new List<StudentRecord>();

    public event Action<IReadOnlyList<StudentRecord>> OnStudentsLoaded;
    public IReadOnlyList<StudentRecord> CurrentStudents => _currentStudents;

    public StudentRepository(IJsonDeserializer deserializer, string jsonFilePath, Func<string, string> readAllText = null)
    {
        _deserializer = deserializer;
        _jsonFilePath = jsonFilePath;
        _readAllText = readAllText ?? File.ReadAllText;
    }

    public void Register()   => ServiceLocator.Instance.Register<IStudentRepository>(this, replaceExistingService: true);
    public void Unregister() => ServiceLocator.Instance.Unregister<IStudentRepository>(this);

    public void Load()
    {
        _currentStudents.Clear();

        string json;
        try
        {
            json = _readAllText(_jsonFilePath);
        }
        catch (IOException e)
        {
            Debug.LogError($"StudentRepository: could not read file at {_jsonFilePath}: {e.Message}");
            OnStudentsLoaded?.Invoke(_currentStudents);
            return;
        }

        if (!_deserializer.TryDeserialize(json, out StudentListJsonDto dto) || dto?.estudiantes == null)
        {
            Debug.LogError("StudentRepository: could not parse students JSON.");
            OnStudentsLoaded?.Invoke(_currentStudents);
            return;
        }

        for (int i = 0; i < dto.estudiantes.Length; i++)
        {
            if (TryConvert(dto.estudiantes[i], i, out StudentRecord record))
            {
                _currentStudents.Add(record);
            }
        }

        OnStudentsLoaded?.Invoke(_currentStudents);
    }

    private bool TryConvert(StudentJsonDto source, int index, out StudentRecord record)
    {
        record = null;

        if (source == null)
        {
            Debug.LogWarning($"StudentRepository: entry at index {index} is null, skipping.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(source.nombre) ||
            string.IsNullOrWhiteSpace(source.apellido) ||
            string.IsNullOrWhiteSpace(source.codigo) ||
            string.IsNullOrWhiteSpace(source.correo))
        {
            Debug.LogWarning($"StudentRepository: entry at index {index} has a missing required field, skipping.");
            return false;
        }

        if (float.IsNaN(source.notaFinal) || source.notaFinal < MinNota || source.notaFinal > MaxNota)
        {
            Debug.LogWarning($"StudentRepository: entry at index {index} (codigo={source.codigo}) has an out-of-range notaFinal ({source.notaFinal}), skipping.");
            return false;
        }

        record = new StudentRecord
        {
            Nombre = source.nombre,
            Apellido = source.apellido,
            Codigo = source.codigo,
            Correo = source.correo,
            NotaFinal = source.notaFinal
        };
        return true;
    }
}
