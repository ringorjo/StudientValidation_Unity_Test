using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultBannerView : MonoBehaviour
{
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private Image _background;
    [SerializeField] private Color _successColor = new Color(0.85f, 0.95f, 0.85f);
    [SerializeField] private Color _errorColor = new Color(0.98f, 0.85f, 0.85f);
    [SerializeField] private Color _idleColor = Color.white;

    public void ShowIdle()
    {
        _messageText.text = string.Empty;
        _background.color = _idleColor;
    }

    public void ShowResult(string message, bool isError)
    {
        _messageText.text = message;
        _background.color = isError ? _errorColor : _successColor;
    }
}
