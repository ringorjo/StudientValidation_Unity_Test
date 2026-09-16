public interface IGradeValidationService : IService
{
    bool Passes(StudentRecord student);
    bool MatchesMarkedStatus(StudentRecord student, bool markedAsApproved);
}
