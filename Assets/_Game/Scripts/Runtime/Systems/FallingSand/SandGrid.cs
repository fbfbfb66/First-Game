
public class SandGrid
{
    private int width;
    private int height;
    private byte[] cells;

    public int Width => width;
    public int Height => height;
    public SandGrid(int width, int height)
    {
        this.width = width;
        this.height = height;
        cells = new byte[width * height];
    }

    public bool IsInside(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return true;
        }
        return false;
    }

    public bool TryGetMaterial(int x, int y, out byte materialId)
    {
        materialId = 0;
        if (IsInside(x, y) == false) return false;
        int index = ToIndex(x, y);
        materialId = cells[index];
        return true;
    }

    public bool TrySetMaterial(int x, int y, byte materialId)
    {
        if (IsInside(x, y) == false) return false;
        int index = ToIndex(x, y);
        cells[index] = materialId;
        return true;
    }

    private int ToIndex(int x, int y)
    {
        return x + y * width;
    }

}
