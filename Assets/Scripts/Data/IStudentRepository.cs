using System;
using System.Collections.Generic;

public interface IStudentRepository : IService
{
    event Action<IReadOnlyList<StudentRecord>> OnStudentsLoaded;
    IReadOnlyList<StudentRecord> CurrentStudents { get; }
    void Load();
}
