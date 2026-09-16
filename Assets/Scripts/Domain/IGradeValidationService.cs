using UnityEngine;

public interface IGradeValidationService : IService
{
    bool Passes(StudentRecord student);
    bool MatchesMarkedStatus(StudentRecord student, bool markedAsApproved);
    string ApprovedLabel { get; }
    string FailedLabel { get; }
    Color ApprovedColor { get; }
    Color FailedColor { get; }
}
