using System;
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
    public bool IsAreaEmpty(int x, int y, int areaWidth, int areaHeight)
    {
        for (int i = 0; i < areaWidth; i++)
        {
            for (int j = 0; j < areaHeight; j++)
            {
                if (cells[x + i, y + j] != null) return false;
            }
        }
        return true;
    }

    public bool Place(InventoryItem item, int x, int y)
    {
        if (item == null) return false;
        if (IsInside(x, y, item.CurrentWidth, item.CurrentHeight) == false) return false;
        if (IsAreaEmpty(x, y, item.CurrentWidth, item.CurrentHeight) == false) return false; 
        
        for(int i = 0;i < item.CurrentWidth; i++)
        {
            for(int j = 0;j < item.CurrentHeight; j++)
            {
                cells[x + i, y + j] = item;
            }
        }
        return true;
    }
    public InventoryItem GetItemAt(int x, int y)
    {
        if (IsInside(x, y) == false) return null;
        return cells[x, y];
    }
}
