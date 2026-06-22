using System;
public class InventoryGrid 
{
    private readonly InventoryItem[,] cells;
    public int Width {get; private set;}
    public int Height {get; private set;}

    public InventoryGrid(int width, int height)
    {   
        if(width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be greater than 0");
        if(height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be greater than 0");

        Width = width;
        Height = height;
        cells = new InventoryItem[width, height];
    }

    public bool IsInside(int x,int y)
    {
        if(x < 0 || x >= Width || y < 0 || y >= Height)
            return false;
        return true;
    }
}
