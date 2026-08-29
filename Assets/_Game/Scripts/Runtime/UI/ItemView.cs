using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Image background;
    [SerializeField] private Transform highlightTransform;
    [SerializeField] private float highlightScale = 1.1f;
    [SerializeField] private TMP_Text amountLabel;

    public void SetAmount(int amount)
    {
        amountLabel.text = amount.ToString();
    }

    public void SetBackgroundTransparent(bool value) => background.color = value ? Color.clear : Color.white;

    public void SetIcon(InventoryItem item, bool value = true)
    {
        if (item == null || item.Data == null || value == false)
        {
            icon.sprite = null;
            icon.enabled = false;
            return;
        }
        icon.sprite = item.Data.Icon;
        icon.enabled = true;
    }
    public void SetHighlighted(bool value)
    {
        highlightTransform.localScale = value ? Vector3.one * highlightScale : Vector3.one;
        if (value)
            transform.SetAsLastSibling();
    }

    public void SetFootprint(Vector2 boxSize, Vector2 iconSize, bool rotated)
    {
        RectTransform rect = (RectTransform)transform;
        rect.sizeDelta = boxSize;
        icon.rectTransform.sizeDelta = iconSize;
        icon.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotated ? -90f : 0f);
    }
}

