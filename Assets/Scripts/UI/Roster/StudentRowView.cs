using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StudentRowView : MonoBehaviour
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _codeText;
    [SerializeField] private TMP_Text _emailText;
    [SerializeField] private TMP_Text _gradeText;
    [SerializeField] private Toggle _approvedToggle;
    [SerializeField] private Image _statusIcon;
    [SerializeField] private TMP_Text _statusLabelText;
    [SerializeField] private Image _noteBackground;
    [SerializeField] private Sprite _approvedSprite;
    [SerializeField] private Sprite _failedSprite;
    [SerializeField] private Image _rowBackground;
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _mismatchColor = new Color(1f, 0.85f, 0.85f);
    [SerializeField] private Image _avatarBackground;
    [SerializeField] private TMP_Text _avatarInitialsText;
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

    private string _approvedLabel = "Aprobado";
    private string _failedLabel = "Reprobado";
    private Color _approvedLabelColor = Color.black;
    private Color _failedLabelColor = Color.black;

    public event System.Action<bool> OnToggleChanged;

    public void SetStatusLabels(string approvedLabel, string failedLabel, Color approvedColor, Color failedColor)
    {
        _approvedLabel = approvedLabel;
        _failedLabel = failedLabel;
        _approvedLabelColor = approvedColor;
        _failedLabelColor = failedColor;
    }

    public void SetData(string fullName, string code, string email, float grade)
    {
        _nameText.text = fullName;
        _codeText.text = code;
        _emailText.text = email;
        _gradeText.text = grade.ToString("0.0");
    }

    public void SetAvatar(string initials, string colorSeed)
    {
        _avatarInitialsText.text = initials;
        int index = Mathf.Abs(colorSeed.GetHashCode()) % _avatarPalette.Length;
        _avatarBackground.color = _avatarPalette[index];
    }

    public void SetToggleWithoutNotify(bool approved)
    {
        _approvedToggle.SetIsOnWithoutNotify(approved);
        UpdateStatusIcon(approved);
    }

    public void SetMismatchHighlight(bool isMismatch)
    {
        _rowBackground.color = isMismatch ? _mismatchColor : _normalColor;
    }

    private void Awake()
    {
        _approvedToggle.onValueChanged.AddListener(HandleToggleChanged);
    }

    private void OnDestroy()
    {
        _approvedToggle.onValueChanged.RemoveListener(HandleToggleChanged);
    }

    private void HandleToggleChanged(bool isOn)
    {
        UpdateStatusIcon(isOn);
        OnToggleChanged?.Invoke(isOn);
    }

    private void UpdateStatusIcon(bool approved)
    {
        _statusIcon.sprite = approved ? _approvedSprite : _failedSprite;
        _statusLabelText.text = approved ? _approvedLabel : _failedLabel;
        _statusLabelText.color = approved ? _approvedLabelColor : _failedLabelColor;
        _noteBackground.color = approved ? _approvedLabelColor : _failedLabelColor;
    }
}
