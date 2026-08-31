using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryView : MonoBehaviour
{
    [SerializeField] private Image selectedBoard;
    [SerializeField] private Image PlacementPreview;
    [SerializeField] private Color validPlacementColor;
    [SerializeField] private Color invalidPlacementColor;
    [SerializeField] private Vector2 offsetDelta = new Vector2(-11f, -15f);
    [SerializeField] private Vector2 offsetPos = new Vector2(10.5f, 0f);
    [SerializeField] private float previewFollowSpeed = 15f;
    [Space]
    [SerializeField] private ItemDetailPanel detailPanel;
    [SerializeField] private RectTransform itemLayer;
    [SerializeField] private RectTransform dragLayer;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private ItemView itemViewPrefab;
    [SerializeField] private float cellSize = 240f;
    [SerializeField] private float spacing = 5f;

    public InventoryItem DragItem => dragItem;

    private readonly Dictionary<InventoryItem, ItemView> itemViews = new();
    private Vector2Int hoveredCell = new Vector2Int(-1, -1);
    private InventoryItem hoveredItem = null;
    private InventoryItem dragItem;
    private InventoryItem selectedItem;
    private Vector2 dragItemOriginalPosition;
    private Vector2 grabOffset;
    private bool allowToHeighlight = true;
    private bool dragRotated;
    private bool DragIsRotated => dragItem.IsRotated ^ dragRotated;
    private int DragWidth => DragIsRotated ? dragItem.Data.Height : dragItem.Data.Width;
    private int DragHeight => DragIsRotated ? dragItem.Data.Width : dragItem.Data.Height;


    private void OnEnable()
    {
        if (inventory == null)
        {
            Debug.LogError("Inventory is not assigned in InventoryView.");
            return;
        }
        Rebuild();
        inventory.ItemPlaced += ShowItem;
        inventory.ItemAmountUpdated += UpdateItemAmount;
        inventory.ItemRemoved += OnItemRemoved;
        detailPanel.DropRequested += DropItem;
    }

    private void OnDisable()
    {
        if (inventory == null)
            return;
        inventory.ItemPlaced -= ShowItem;
        inventory.ItemAmountUpdated -= UpdateItemAmount;
        inventory.ItemRemoved -= OnItemRemoved;
        detailPanel.DropRequested -= DropItem;

        // dragItem 为 null 时不能进 TryGetValue —— Dictionary 对 null 键抛
        // ArgumentNullException，而「没在拖东西时关闭背包」是最常见的路径。
        if (dragItem != null && itemViews.TryGetValue(dragItem, out var itemView))
        {
            RectTransform rect = (RectTransform)itemView.transform;
            rect.SetParent(itemLayer, true);
            itemView.SetBackgroundTransparent(false);
            SetItemView(dragItem, itemView, dragItem.IsRotated);
        }
        HidePlacementPreview();
        SetSelectedItem(null);
        dragItem = null;
        allowToHeighlight = true;
    }

    private void SetSelectedItem(InventoryItem item)
    {
        selectedItem = item;
        if(selectedItem == null)
        {
            detailPanel.Hide();
            HideSelectedBoard();
            return;
        }
        detailPanel.Show(selectedItem);
        ShowSelectedBoard();
    }

    private void ShowSelectedBoard()
    {
        if (selectedItem == null) return;
        if (dragItem != null) return;
        if (inventory.TryGetItemPosition(selectedItem, out int x, out int y) == false) return;
        selectedBoard.gameObject.SetActive(true);
        selectedBoard.rectTransform.SetAsLastSibling();
        selectedBoard.rectTransform.anchoredPosition = GetAnchorPositionForCell(x, y);
        selectedBoard.rectTransform.sizeDelta = GetRectSizeDelta(selectedItem.CurrentWidth, selectedItem.CurrentHeight);
    }

    private void HideSelectedBoard()
    {
        selectedBoard.gameObject.SetActive(false);
    }

    public void SelectItem(Vector2 screenPosition)
    {
        if (dragItem != null) return;
        if(TryGetCellAt(screenPosition,out int x,out int y))
        {
            SetSelectedItem(inventory.GetItemAt(x,y));
        }
        else
        {
            SetSelectedItem(null);
        }
    }

    private void DropItem(InventoryItem item, int amount)
    {
        inventory.TryRemove(item, amount);
    }

    private void OnItemRemoved(InventoryItem item)
    {
        if(itemViews.TryGetValue(item, out var view))
            Destroy(view.gameObject);

        itemViews.Remove(item);
        if (hoveredItem == item) hoveredItem = null;
        if(dragItem == item) dragItem = null;
        if (selectedItem == item) SetSelectedItem(null);
    }

    private void Rebuild()
    {
        foreach (var (item, x, y) in inventory.GetPlacedItems())
        {
            ShowItem(item, x, y);
        }
    }

    public void UpdateItemAmount(InventoryItem item)
    {
        if (itemViews.TryGetValue(item, out ItemView view))
        {
            view.SetAmount(item.Amount);
        }
        if (item == selectedItem) SetSelectedItem(item);
    }

    public void RotateDragItem()
    {
        if (dragItem == null) return;
        if (dragItem.Data.CanRotate == false) return;

        dragRotated = !dragRotated;

        if (itemViews.TryGetValue(dragItem, out var view))
        {
            SetItemView(dragItem, view, DragIsRotated);
            RectTransform rect = (RectTransform)view.transform;
            Vector2 pointerLocal = rect.anchoredPosition + grabOffset;
            Vector2 boxSize = GetRectSizeDelta(DragWidth, DragHeight);
            grabOffset = new Vector2(boxSize.x * .5f, -boxSize.y * .5f);
            rect.anchoredPosition = pointerLocal - grabOffset;

            GetDragTargetCell(rect, out int x, out int y);
            bool canPlace = inventory.CanPlace(x, y, DragWidth, DragHeight, dragItem);
            RotatePlacementView(new Vector2Int(x, y), canPlace);
        }
    }

    public bool BeginDrag(Vector2 screenPosition)
    {
        if (TryGetCellAt(screenPosition, out int x, out int y))
        {
            dragItem = inventory.GetItemAt(x, y);
            if (dragItem != null)
            {
                if (itemViews.TryGetValue(dragItem, out var itemView))
                {
                    RectTransform rect = (RectTransform)itemView.transform;
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(dragLayer, screenPosition, null, out Vector2 localPosition))
                    {
                        dragItemOriginalPosition = rect.anchoredPosition;
                        rect.SetParent(dragLayer, true);
                        grabOffset = localPosition - rect.anchoredPosition;
                        rect.anchoredPosition = localPosition - grabOffset;
                    }
                    itemView.SetBackgroundTransparent(true);
                    itemView.SetHighlighted(false);
                    hoveredItem = null;
                    allowToHeighlight = false;
                    dragRotated = false;

                    GetDropItemAt(dragItemOriginalPosition, out int originalX, out int originalY);
                    ShowPlacementPreview(new Vector2Int(originalX, originalY), dragItem);
                    if(selectedItem == dragItem)
                        HideSelectedBoard();
                }
                Debug.Log($"Begin dragging item: {dragItem.Data.DisplayName} from cell ({x}, {y})");
                return true;
            }
            return false;
        }
        return false;
    }
    public void Drag(Vector2 screenPosition)
    {
        if (dragItem == null || itemViews.TryGetValue(dragItem, out var itemView) == false)
            return;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(dragLayer, screenPosition, null, out Vector2 localPosition) == false)
            return;

        RectTransform rect = (RectTransform)itemView.transform;
        rect.anchoredPosition = localPosition - grabOffset;

        GetDragTargetCell(rect, out int x, out int y);
        UpdatePlacementPreview(new Vector2Int(x, y), inventory.CanPlace(x, y, DragWidth, DragHeight, dragItem));
    }
    public void EndDrag()
    {
        if (dragItem != null && itemViews.TryGetValue(dragItem, out var itemView))
        {
            RectTransform rect = (RectTransform)itemView.transform;
            rect.SetParent(itemLayer, true);

            GetDropItemAt(rect.anchoredPosition, out int x, out int y);
            if (inventory.TryMove(dragItem, x, y, DragIsRotated))
            {
                rect.anchoredPosition = GetAnchorPositionForCell(x, y);
                Debug.Log($"Dropped item: {dragItem.Data.DisplayName} to cell ({x}, {y})");

            }
            else
            {
                rect.anchoredPosition = dragItemOriginalPosition;
                SetItemView(dragItem, itemView, dragItem.IsRotated);
            }

            itemView.SetBackgroundTransparent(false);
            itemView.SetHighlighted(true);
            hoveredItem = dragItem;
        }
        HidePlacementPreview();
        allowToHeighlight = true;
        dragItem = null;
        ShowSelectedBoard();
    }

    public void UpdateHover(Vector2 screenPosition)
    {
        if (TryGetCellAt(screenPosition, out int x, out int y))
        {
            InventoryItem item = inventory.GetItemAt(x, y);
            if (hoveredCell.x != x || hoveredCell.y != y)
            {
                hoveredCell.x = x;
                hoveredCell.y = y;
                //Debug.Log($"Mouse is over cell ({x}, {y}) : {item?.Data.DisplayName}");
            }
            if (allowToHeighlight && hoveredItem != item)
            {
                if (hoveredItem != null && itemViews.TryGetValue(hoveredItem, out ItemView previousView))
                {
                    previousView.SetHighlighted(false);
                }
                hoveredItem = item;
                if (item != null && itemViews.TryGetValue(item, out ItemView view))
                {
                    view.SetHighlighted(true);
                }
            }
        }
        else
            ClearHover();
    }
    public void ClearHover()
    {
        hoveredCell = new Vector2Int(-1, -1);
        if (hoveredItem != null && itemViews.TryGetValue(hoveredItem, out ItemView view))
        {
            view.SetHighlighted(false);
            hoveredItem = null;
        }
    }

    public void ShowItem(InventoryItem item, int x, int y)
    {
        if (item == null || itemLayer == null || itemViewPrefab == null) return;

        if (!itemViews.TryGetValue(item, out ItemView view))
            view = Instantiate(itemViewPrefab, itemLayer);
        RectTransform rect = (RectTransform)view.transform;

        view.SetIcon(item);
        view.SetAmount(item.Amount);

        if (!itemViews.ContainsKey(item))
        {
            itemViews.Add(item, view);
        }

        rect.anchoredPosition = GetAnchorPositionForCell(x, y);
        SetItemView(item, view, item.IsRotated);
    }

    public bool TryGetCellAt(Vector2 screenPosition, out int x, out int y)
    {
        x = -1;
        y = -1;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(itemLayer, screenPosition, null, out Vector2 localPosition) == false)
        {
            return false;
        }
        x = Mathf.FloorToInt(localPosition.x / (cellSize + spacing));
        y = Mathf.FloorToInt(-localPosition.y / (cellSize + spacing));
        if (inventory.IsInside(x, y))
        {
            return true;
        }
        return false;
    }

    private void SetItemView(InventoryItem item, ItemView view, bool rotated)
    {
        int boxWidth = rotated ? item.Data.Height : item.Data.Width;
        int boxHeight = rotated ? item.Data.Width : item.Data.Height;

        Vector2 boxSize = GetRectSizeDelta(boxWidth, boxHeight);
        Vector2 iconSize = GetRectSizeDelta(item.Data.Width, item.Data.Height);
        view.SetFootprint(boxSize, iconSize, rotated);
    }

    private Vector2 GetAnchorPositionForCell(int x, int y)
    {
        float step = cellSize + spacing;
        return new Vector2(x * step, -y * step);
    }


    private Vector2 GetRectSizeDelta(int width, int height)
    {
        return new Vector2(cellSize * width + (width - 1) * spacing, cellSize * height + (height - 1) * spacing);
    }

    private void GetDragTargetCell(RectTransform rect, out int x, out int y)
    {
        Vector2 itemLayerPosition = itemLayer.InverseTransformPoint(rect.position);
        GetDropItemAt(itemLayerPosition, out x, out y);
    }

    private void GetDropItemAt(Vector2 itemPosition, out int x, out int y)
    {
        x = Mathf.RoundToInt(itemPosition.x / (cellSize + spacing));
        y = Mathf.RoundToInt(-itemPosition.y / (cellSize + spacing));
    }

    private void RotatePlacementView(Vector2Int cell, bool valid)
    {
        RectTransform rect = PlacementPreview.rectTransform;
        rect.anchoredPosition = GetAnchorPositionForCell(cell.x, cell.y);
        rect.sizeDelta = GetRectSizeDelta(DragWidth, DragHeight) + offsetDelta;
        PlacementPreview.color = valid ? validPlacementColor : invalidPlacementColor;
    }

    /// <summary>
    /// 拖拽开始时调用一次：把预览框摆好并【瞬间】就位。
    /// 不能走 Lerp —— 否则框会从上一次拖拽残留的位置一路飞过来。
    /// </summary>
    private void ShowPlacementPreview(Vector2Int cell, InventoryItem item)
    {
        RectTransform rect = PlacementPreview.rectTransform;
        PlacementPreview.gameObject.SetActive(true);
        rect.SetAsLastSibling();
        rect.sizeDelta = GetRectSizeDelta(item.CurrentWidth, item.CurrentHeight) + offsetDelta;
        rect.anchoredPosition = GetPreviewPosition(cell);
        PlacementPreview.color = validPlacementColor;
    }

    /// <summary>
    /// 拖拽中每帧调用：只做颜色和位置两件事。
    /// 位置用 Lerp 朝目标格逼近，目标中途变了也能随时改道。
    /// 用 unscaledDeltaTime：背包若在暂停时打开，deltaTime 会是 0，动画会整个停住。
    /// </summary>
    private void UpdatePlacementPreview(Vector2Int cell, bool valid)
    {
        RectTransform rect = PlacementPreview.rectTransform;
        PlacementPreview.color = valid ? validPlacementColor : invalidPlacementColor;
        rect.anchoredPosition = Vector2.Lerp(
            rect.anchoredPosition,
            GetPreviewPosition(cell),
            previewFollowSpeed * Time.unscaledDeltaTime);
    }

    private void HidePlacementPreview() => PlacementPreview.gameObject.SetActive(false);

    /// <summary>格子坐标 → 预览框的 anchoredPosition（含视觉微调偏移）。</summary>
    private Vector2 GetPreviewPosition(Vector2Int cell) => GetAnchorPositionForCell(cell.x, cell.y) + offsetPos;

}
