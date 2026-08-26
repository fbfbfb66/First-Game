using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryPointerHandler : MonoBehaviour, IPointerMoveHandler, IPointerExitHandler,IBeginDragHandler,IDragHandler,IEndDragHandler
{
    [SerializeField] private InventoryView view;

    public void OnPointerMove(PointerEventData eventData)
    {
        view.UpdateHover(eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        view.ClearHover();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        view.BeginDrag(eventData.pressPosition);
    }
    public void OnDrag(PointerEventData eventData)
    {
        view.Drag(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        view.EndDrag();
    }

}
