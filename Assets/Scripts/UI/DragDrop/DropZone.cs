using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private bool _isApprovedZone;
    [SerializeField] private Transform _container;

    public event System.Action<StudentDragItem, bool> OnItemDropped;

    public void OnDrop(PointerEventData eventData)
    {
        StudentDragItem item = eventData.pointerDrag != null
            ? eventData.pointerDrag.GetComponent<StudentDragItem>()
            : null;

        if (item == null) return;

        item.SnapInto(_container != null ? _container : transform);
        OnItemDropped?.Invoke(item, _isApprovedZone);
    }
}
