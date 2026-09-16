using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultBannerView : MonoBehaviour
{
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private Image _background;

    [SerializeField] private Sprite _idleSprite;
    [SerializeField] private Sprite _successSprite;
    [SerializeField] private Sprite _errorSprite;

    public void ShowIdle()
    {
        _messageText.text = string.Empty;
        _background.sprite = _idleSprite;
    }

    public void ShowResult(string message, bool isError)
    {
        _messageText.text = message;

        Sprite sprite = isError ? _errorSprite : _successSprite;
        if (sprite != null) _background.sprite = sprite;
    }
}
