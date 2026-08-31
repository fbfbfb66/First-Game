using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailPanel : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private TMP_Text categoryLabel;
    [SerializeField] private TMP_Text descriptionLabel;
    [SerializeField] private GameObject basic;
    [SerializeField] private GameObject fallBack;
    [Space]
    [SerializeField] private Button dropButton;
    [SerializeField] private Button minusButton;
    [SerializeField] private Button plusButton;
    [SerializeField] private GameObject amountRow;
    [SerializeField] private TMP_Text amountLabel;

    public event Action<InventoryItem, int> DropRequested;
    private InventoryItem currentItem;
    private int currentAmount;

    private void Awake()
    {
        basic.SetActive(false);
        fallBack.SetActive(true);
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

    public void Show(InventoryItem item)
    {
        basic.SetActive(true);
        fallBack.SetActive(false);
        icon.sprite = item.Data.Icon;
        nameLabel.text = item.Data.DisplayName;
        categoryLabel.text = item.Data.Category.ToString();
        descriptionLabel.text = item.Data.Description;

        currentItem = item;
        currentAmount = 1;
        RefreshAmount();
        if (item.Amount <= 1) SetAmountRow(false);
        else SetAmountRow(true);
    }

    public void Hide()
    {
        currentItem = null;
        basic.SetActive(false);
        fallBack.SetActive(true);
    }

    private void SetAmountRow(bool enable)
    {
        amountRow.SetActive(enable);
    }

    private void OnDropButton()
    {
        DropRequested?.Invoke(currentItem, currentAmount);
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
        amountLabel.text = currentAmount.ToString();
        minusButton.interactable = currentAmount > 1;
        plusButton.interactable = currentAmount < currentItem.Amount;
    }
}
