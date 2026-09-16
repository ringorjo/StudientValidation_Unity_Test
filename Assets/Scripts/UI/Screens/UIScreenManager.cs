using UnityEngine;

public class UIScreenManager : MonoBehaviour
{
    [SerializeField] private GameObject _rosterScreenRoot;
    [SerializeField] private GameObject _dragDropScreenRoot;

    private void Awake()
    {
        ShowRosterScreen();
    }

    public void ShowRosterScreen()
    {
        _rosterScreenRoot.SetActive(true);
        _dragDropScreenRoot.SetActive(false);
    }

    public void ShowDragDropScreen()
    {
        _rosterScreenRoot.SetActive(false);
        _dragDropScreenRoot.SetActive(true);
    }
}
