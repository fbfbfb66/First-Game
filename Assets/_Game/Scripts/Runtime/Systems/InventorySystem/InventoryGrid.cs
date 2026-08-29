using System;
using System.Collections.Generic;
public class InventoryGrid
{
    private readonly InventoryItem[,] cells;
    public int Width { get; private set; }
    public int Height { get; private set; }

    public InventoryGrid(int width, int height)
    {
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be greater than 0");
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be greater than 0");

        Width = width;
        Height = height;
        cells = new InventoryItem[width, height];
    }

    public InventoryItem FindStackable(ItemData data)
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                InventoryItem item = cells[x, y];
                if (item != null && item.CanStackWith(data))
                {
                    return item;
                }
            }
        }
        return null;
    }

    public IEnumerable<(InventoryItem item, int x, int y)> GetPlacedItems()
    {
        HashSet<InventoryItem> seenItems = new HashSet<InventoryItem>();
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                InventoryItem item = cells[x, y];
                if (item != null && !seenItems.Contains(item))
                {
                    seenItems.Add(item);
                    yield return (item, x, y);
                }
            }
        }
    }

    public bool TryFindFreeCell(InventoryItem item, out int x, out int y)
    {
        x = -1;
        y = -1;
        for (int j = 0; j < Height; j++)
        {
            for (int i = 0; i < Width; i++)
            {
                if (CanPlace(item, i, j))
                {
                    x = i;
                    y = j;
                    return true;
                }
            }
        }
        return false;
    }

    public bool IsInside(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return false;
        return true;
    }
    public bool IsInside(int x, int y, int areaWidth, int areaHeight)
    {
        if (IsInside(x, y) == false) return false;
        int maxX = x + areaWidth - 1;
        int maxY = y + areaHeight - 1;
        if (IsInside(maxX, maxY)) return true;
        return false;
    }
    public bool IsAreaEmpty(int x, int y, int areaWidth, int areaHeight, InventoryItem ignoreItem = null)
    {
        for (int i = 0; i < areaWidth; i++)
        {
            for (int j = 0; j < areaHeight; j++)
            {
                InventoryItem item = cells[x + i, y + j];
                if (item == ignoreItem) continue;
                if (item != null) return false;
            }
        }
        return true;
    }

    public bool Place(InventoryItem item, int x, int y, bool ignoreItem = false)
    {
        if (CanPlace(item, x, y, ignoreItem) == false) return false;

        for (int i = 0; i < item.CurrentWidth; i++)
        {
            for (int j = 0; j < item.CurrentHeight; j++)
            {
                cells[x + i, y + j] = item;
            }
        }
        return true;
    }
    //CanPlace 检查从（x,y）开始放置物品是否可行，考虑物品的宽度和高度，以及是否忽略当前物品
    public bool CanPlace(InventoryItem item, int x, int y, bool ignoreItem = false)
    {
        if (item == null) return false;
        if (IsInside(x, y, item.CurrentWidth, item.CurrentHeight) == false) return false;
        if (IsAreaEmpty(x, y, item.CurrentWidth, item.CurrentHeight, ignoreItem ? item : null) == false) return false;
        return true;
    }

    public bool CanPlace(int x, int y, int areaWidth, int areaHeight, InventoryItem ignoreItem = null)
    {
        if (IsInside(x, y, areaWidth, areaHeight) == false) return false;
        if (IsAreaEmpty(x, y, areaWidth, areaHeight, ignoreItem) == false) return false;
        return true;
    }

    public bool Remove(InventoryItem item)
    {
        if (item == null) return false;
        bool found = false;
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (cells[x, y] == item)
                {
                    found = true;
                    cells[x, y] = null;
                }
            }
        }
        return found;
    }

    public InventoryItem GetItemAt(int x, int y)
    {
        if (IsInside(x, y) == false) return null;
        return cells[x, y];
    }
}
