public class GradeValidationService : IGradeValidationService
{
    private readonly GradeValidationConfig _config;

    public GradeValidationService(GradeValidationConfig config)
    {
        _config = config;
    }

    public void Register()   => ServiceLocator.Instance.Register<IGradeValidationService>(this, replaceExistingService: true);
    public void Unregister() => ServiceLocator.Instance.Unregister<IGradeValidationService>(this);

    public bool Passes(StudentRecord student) => student.FinalGrade >= _config.PassingThreshold;

    public bool MatchesMarkedStatus(StudentRecord student, bool markedAsApproved) => Passes(student) == markedAsApproved;
}
