using UnityEngine;
using UnityEngine.UI;

public class ItemView : MonoBehaviour
{
    [SerializeField] private Image icon;

    public void SetIcon(InventoryItem item)
    {
        if (item == null || item.Data == null)
        {
            icon.sprite = null;
            icon.enabled = false;
            return;
        }
        icon.sprite = item.Data.Icon;
        icon.enabled = true;
    }
}
