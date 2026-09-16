public class StudentRowPresenter
{
    public StudentRowView View { get; }
    public StudentRecord Student { get; private set; }
    public bool MarkedAsApproved { get; private set; }

    public StudentRowPresenter(StudentRowView view)
    {
        View = view;
        View.OnToggleChanged += HandleToggleChanged;
    }

    public void Bind(StudentRecord student)
    {
        Student = student;
        MarkedAsApproved = false;
        View.SetData(student.FullName, student.Code, student.Email, student.FinalGrade);
        View.SetAvatar(BuildInitials(student.FirstName, student.LastName), student.Code);
        View.SetToggleWithoutNotify(false);
        View.SetMismatchHighlight(false);
    }

    private static string BuildInitials(string firstName, string lastName)
    {
        char first = string.IsNullOrEmpty(firstName) ? '?' : char.ToUpperInvariant(firstName[0]);
        char last = string.IsNullOrEmpty(lastName) ? '?' : char.ToUpperInvariant(lastName[0]);
        return $"{first}{last}";
    }

    public void ShowMismatch(bool isMismatch)
    {
        View.SetMismatchHighlight(isMismatch);
    }

    public void Dispose()
    {
        View.OnToggleChanged -= HandleToggleChanged;
    }

    private void HandleToggleChanged(bool isOn)
    {
        MarkedAsApproved = isOn;
    }
}
