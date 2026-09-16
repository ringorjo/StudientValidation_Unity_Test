using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RosterScreenController : MonoBehaviour
{
    [SerializeField] private StudentRowView _rowPrefab;
    [SerializeField] private Transform _rowsContainer;
    [SerializeField] private Button _validateButton;
    [SerializeField] private Button _reloadButton;
    [SerializeField] private Button _continueButton;
    [SerializeField] private ResultBannerView _resultBanner;
    [SerializeField] private UIScreenManager _screenManager;
    [SerializeField] private TMP_Text _studentCountText;

    private IStudentRepository _studentRepository;
    private IGradeValidationService _gradeValidationService;
    private ObjectPool<StudentRowView> _rowPool;
    private readonly List<StudentRowPresenter> _presenters = new List<StudentRowPresenter>();

    private void Awake()
    {
        _rowPool = new ObjectPool<StudentRowView>(_rowPrefab, _rowsContainer);

        _validateButton.onClick.AddListener(HandleValidateClicked);
        _reloadButton.onClick.AddListener(HandleReloadClicked);
        _continueButton.onClick.AddListener(HandleContinueClicked);
    }

    private void Start()
    {
        _studentRepository = ServiceLocator.Instance.Get<IStudentRepository>();
        _gradeValidationService = ServiceLocator.Instance.Get<IGradeValidationService>();
        _studentRepository.OnStudentsLoaded += HandleStudentsLoaded;
        _studentRepository.Load();
    }

    private void OnDestroy()
    {
        _validateButton.onClick.RemoveListener(HandleValidateClicked);
        _reloadButton.onClick.RemoveListener(HandleReloadClicked);
        _continueButton.onClick.RemoveListener(HandleContinueClicked);
        _studentRepository.OnStudentsLoaded -= HandleStudentsLoaded;
    }

    private void HandleReloadClicked() => _studentRepository.Load();

    private void HandleContinueClicked() => _screenManager.ShowDragDropScreen();

    private void HandleStudentsLoaded(IReadOnlyList<StudentRecord> students)
    {
        ClearRows();

        foreach (StudentRecord student in students)
        {
            StudentRowView view = _rowPool.Get();
            view.SetStatusLabels(_gradeValidationService.ApprovedLabel, _gradeValidationService.FailedLabel, _gradeValidationService.ApprovedColor, _gradeValidationService.FailedColor);
            var presenter = new StudentRowPresenter(view);
            presenter.Bind(student);
            _presenters.Add(presenter);
        }

        if (_studentCountText != null) _studentCountText.text = $"{students.Count} estudiantes";
        _resultBanner.ShowIdle();
    }

    private void ClearRows()
    {
        foreach (StudentRowPresenter presenter in _presenters)
        {
            presenter.Dispose();
            _rowPool.Release(presenter.View);
        }
        _presenters.Clear();
    }

    private void HandleValidateClicked()
    {
        var mismatches = new List<StudentRowPresenter>();

        foreach (StudentRowPresenter presenter in _presenters)
        {
            bool matches = _gradeValidationService.MatchesMarkedStatus(presenter.Student, presenter.MarkedAsApproved);
            presenter.ShowMismatch(!matches);
            if (!matches) mismatches.Add(presenter);
        }

        if (mismatches.Count == 0)
        {
            _resultBanner.ShowResult($"Todo correcto. {_presenters.Count} de {_presenters.Count} estudiantes clasificados correctamente.", isError: false);
        }
        else
        {
            var names = new List<string>();
            foreach (StudentRowPresenter presenter in mismatches) names.Add(presenter.Student.FullName);
            _resultBanner.ShowResult($"Hay errores en: {string.Join(", ", names)}.", isError: true);
        }
    }
}
