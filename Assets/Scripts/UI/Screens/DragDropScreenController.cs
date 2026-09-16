using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DragDropScreenController : MonoBehaviour
{
    [SerializeField] private Canvas _rootCanvas;
    [SerializeField] private StudentDragItem _dragItemPrefab;
    [SerializeField] private Transform _unclassifiedContainer;
    [SerializeField] private DropZone _approvedZone;
    [SerializeField] private DropZone _failedZone;
    [SerializeField] private Button _verifyButton;
    [SerializeField] private ResultBannerView _resultBanner;
    [SerializeField] private StudentReferenceRowView _referenceRowPrefab;

    private IStudentRepository _studentRepository;
    private IGradeValidationService _gradeValidationService;
    private ObjectPool<StudentDragItem> _itemPool;
    private readonly Dictionary<StudentDragItem, bool?> _placement = new Dictionary<StudentDragItem, bool?>();

    private void Awake()
    {
        _studentRepository = ServiceLocator.Instance.Get<IStudentRepository>();
        _gradeValidationService = ServiceLocator.Instance.Get<IGradeValidationService>();
        _itemPool = new ObjectPool<StudentDragItem>(_dragItemPrefab, _unclassifiedContainer);

        _approvedZone.OnItemDropped += HandleItemDropped;
        _failedZone.OnItemDropped += HandleItemDropped;
        _verifyButton.onClick.AddListener(HandleVerifyClicked);
    }

    private void OnEnable()
    {
        PopulateFromCurrentStudents();
    }

    private void OnDestroy()
    {
        _approvedZone.OnItemDropped -= HandleItemDropped;
        _failedZone.OnItemDropped -= HandleItemDropped;
        _verifyButton.onClick.RemoveListener(HandleVerifyClicked);
    }

    private void PopulateFromCurrentStudents()
    {
        ClearAll();

        foreach (StudentRecord student in _studentRepository.CurrentStudents)
        {
            StudentDragItem item = _itemPool.Get();
            item.Bind(student, _rootCanvas, student.Code);
            item.SnapInto(_unclassifiedContainer);
            _placement[item] = null;

        }

        _resultBanner.ShowIdle();
    }

    private void ClearAll()
    {
        foreach (StudentDragItem item in new List<StudentDragItem>(_placement.Keys))
        {
            _itemPool.Release(item);
        }
        _placement.Clear();
    }

    private void HandleItemDropped(StudentDragItem item, bool isApprovedZone)
    {
        _placement[item] = isApprovedZone;
    }

    private void HandleVerifyClicked()
    {
        int correctCount = 0;
        var mismatches = new List<string>();

        foreach (var pair in _placement)
        {
            StudentDragItem item = pair.Key;
            bool? markedAsApproved = pair.Value;

            if (markedAsApproved == null)
            {
                mismatches.Add($"{item.Student.FullName} (sin clasificar)");
                continue;
            }

            bool matches = _gradeValidationService.MatchesMarkedStatus(item.Student, markedAsApproved.Value);
            if (matches)
            {
                correctCount++;
            }
            else
            {
                mismatches.Add(item.Student.FullName);
            }
        }

        if (mismatches.Count == 0)
        {
            _resultBanner.ShowResult($"Todo correcto. {correctCount} de {_placement.Count} estudiantes clasificados correctamente.", isError: false);
        }
        else
        {
            _resultBanner.ShowResult($"Hay errores en: {string.Join(", ", mismatches)}.", isError: true);
        }
    }
}
