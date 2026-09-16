[System.Serializable]
public class StudentRecord
{
    public string FirstName;
    public string LastName;
    public string Code;
    public string Email;
    public float FinalGrade;

    public string FullName => $"{FirstName} {LastName}";
}
