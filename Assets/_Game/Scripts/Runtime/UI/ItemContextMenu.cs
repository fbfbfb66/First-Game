using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemContextMenu : MonoBehaviour
{
    [SerializeField] private ContextMenuBlocker blocker;
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private Button dropButton;
    [SerializeField] private RectTransform parent;
    [SerializeField] private Button minusButton;
    [SerializeField] private Button plusButton;
    [SerializeField] private TMP_Text amountLabel;

    public event Action<InventoryItem, int> DropRequested;
    private InventoryItem currentItem;
    private int currentAmount = 1;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (dropButton == null) return;
        dropButton.onClick.AddListener(OnDropButton);
        minusButton.onClick.AddListener(OnMinusButton);
        plusButton.onClick.AddListener(OnPlusButton);
    }

    private void OnDisable()
    {
        if (dropButton == null) return;
        dropButton.onClick.RemoveListener(OnDropButton);
        minusButton.onClick.RemoveListener(OnMinusButton);
        plusButton.onClick.RemoveListener(OnPlusButton);
    }

    public void Open(InventoryItem item, Vector2 screenPosition)
    {
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(parent, screenPosition, null, out Vector3 worldPosition) == false) return;
        gameObject.SetActive(true);
        blocker.gameObject.SetActive(true);
        currentItem = item;
        currentAmount = 1;
        itemName.text = item.Data.DisplayName;
        transform.position = worldPosition;

        RefreshAmount();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        blocker.gameObject.SetActive(false);
    }

    private void OnDropButton()
    {
        DropRequested?.Invoke(currentItem, currentAmount);
        RefreshAmount();
    }

    private void OnPlusButton()
    {
        currentAmount = Mathf.Min(currentAmount + 1, currentItem.Amount);
        RefreshAmount();
    }
    private void OnMinusButton()
    {
        currentAmount = Mathf.Max(currentAmount - 1, 1);
        RefreshAmount();
    }

    private void RefreshAmount()
    {
        currentAmount = Mathf.Min(currentAmount, currentItem.Amount);
        amountLabel.text = currentAmount.ToString();
        minusButton.interactable = currentAmount > 1;
        plusButton.interactable = currentAmount < currentItem.Amount;
    }

}
