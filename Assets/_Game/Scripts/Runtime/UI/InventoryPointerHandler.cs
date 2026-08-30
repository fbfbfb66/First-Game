using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryPointerHandler : MonoBehaviour, IPointerMoveHandler, IPointerExitHandler,IBeginDragHandler,IDragHandler,IEndDragHandler,IPointerClickHandler
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private InventoryView view;
    private bool routingToScroll;

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
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (view.DragItem != null) return;

        routingToScroll = view.BeginDrag(eventData.pressPosition) ? false : true;
        if(routingToScroll )
        {
            scrollRect.OnBeginDrag(eventData);
        }
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        view.Drag(eventData.position);
        if(routingToScroll)
        {
            scrollRect.OnDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        view.EndDrag();
        if (routingToScroll)
        {
            scrollRect.OnEndDrag(eventData);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        view.RequestItemMenu(eventData.position);
    }
}
