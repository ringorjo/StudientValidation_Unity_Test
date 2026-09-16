using TMPro;
using UnityEngine;

public class StudentReferenceRowView : MonoBehaviour
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _gradeText;

    public void SetData(string fullName, float grade)
    {
        _nameText.text = fullName;
        _gradeText.text = grade.ToString("0.0");
    }
}
