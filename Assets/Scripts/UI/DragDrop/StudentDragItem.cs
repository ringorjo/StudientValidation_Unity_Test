using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StudentDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _noteText;
    [SerializeField] private TMP_Text _initialsText;
    [SerializeField] private Image _initialContent;

    [SerializeField]
    private Color[] _avatarPalette =
   {
        new Color(0.20f, 0.50f, 0.80f),
        new Color(0.20f, 0.65f, 0.40f),
        new Color(0.75f, 0.30f, 0.30f),
        new Color(0.55f, 0.40f, 0.80f),
        new Color(0.85f, 0.60f, 0.15f),
        new Color(0.25f, 0.65f, 0.65f)
    };

    private Canvas _rootCanvas;
    private Transform _originalParent;
    private Vector2 _originalAnchoredPosition;

    public StudentRecord Student { get; private set; }

    public void Bind(StudentRecord student, Canvas rootCanvas, string code)
    {
        Student = student;
        _rootCanvas = rootCanvas;
        int index = Mathf.Abs(code.GetHashCode()) % _avatarPalette.Length;
        _initialContent.color = _avatarPalette[index];
        SetData(student.FullName, student.FinalGrade, BuildInitials(student.FirstName, student.LastName));
    }

    private void SetData(string fullName, float grade, string initials)
    {
        if (_nameText != null) _nameText.text = fullName;
        if (_noteText != null) _noteText.text = $"Nota: {grade:0.0}";
        if (_initialsText != null) _initialsText.text = initials;
    }

    private static string BuildInitials(string firstName, string lastName)
    {
        char first = string.IsNullOrEmpty(firstName) ? '?' : char.ToUpperInvariant(firstName[0]);
        char last = string.IsNullOrEmpty(lastName) ? '?' : char.ToUpperInvariant(lastName[0]);
        return $"{first}{last}";
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _originalParent = transform.parent;
        _originalAnchoredPosition = _rectTransform.anchoredPosition;
        transform.SetParent(_rootCanvas.transform, worldPositionStays: true);
        _canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        _rectTransform.anchoredPosition += eventData.delta / _rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = true;

        if (transform.parent == _rootCanvas.transform)
        {
            ReturnToOrigin();
        }
    }

    public void ReturnToOrigin()
    {
        transform.SetParent(_originalParent, worldPositionStays: false);
        _rectTransform.anchoredPosition = _originalAnchoredPosition;
    }

    public void SnapInto(Transform newParent)
    {
        transform.SetParent(newParent, worldPositionStays: false);
        _originalParent = newParent;
        _originalAnchoredPosition = _rectTransform.anchoredPosition;
    }
}
