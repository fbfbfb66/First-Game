using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryPointerHandler : MonoBehaviour, IPointerMoveHandler, IPointerExitHandler
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
}
