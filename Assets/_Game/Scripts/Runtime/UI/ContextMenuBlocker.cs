using UnityEngine;
using UnityEngine.EventSystems;

public class ContextMenuBlocker : MonoBehaviour,IPointerClickHandler
{
    [SerializeField] private ItemContextMenu menu;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (menu == null) return;
        menu.Close();
    }
}
