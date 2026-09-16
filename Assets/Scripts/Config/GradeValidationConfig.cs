using UnityEngine;

[CreateAssetMenu(fileName = "GradeValidationConfig", menuName = "PanelDeNotas/Grade Validation Config")]
public class GradeValidationConfig : ScriptableObject
{
    [SerializeField] private float _passingThreshold = 3.0f;
    [SerializeField] private string _approvedLabel = "Aprobado";
    [SerializeField] private string _failedLabel = "Reprobado";
    [SerializeField] private Color _approvedColor = new Color(0.16f, 0.65f, 0.27f);
    [SerializeField] private Color _failedColor = new Color(0.80f, 0.15f, 0.15f);

    public float PassingThreshold => _passingThreshold;
    public string ApprovedLabel => _approvedLabel;
    public string FailedLabel => _failedLabel;
    public Color ApprovedColor => _approvedColor;
    public Color FailedColor => _failedColor;
}
