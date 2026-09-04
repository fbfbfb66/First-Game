using UnityEngine;

public class SandGridRenderer : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    private SandGrid grid;
    private Texture2D texture;
    private Sprite runtimeSprite;
    private Color32[] pixels;

    public void Initialize(SandGrid grid, float cellSize)
    {
        this.grid = grid;
        texture = new Texture2D(grid.Width, grid.Height, TextureFormat.RGBA32, false);
        pixels = new Color32[grid.Width * grid.Height];
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        Rect textureRect = new Rect(0, 0, texture.width, texture.height);
        float pixelsPerUnit = 1f / cellSize;
        runtimeSprite = Sprite.Create(texture, textureRect, Vector2.zero, pixelsPerUnit);
        spriteRenderer.sprite = runtimeSprite;
    }

    public void Render()
    {
        for(int y=0; y<grid.Height; y++)
        {
            for(int x=0; x<grid.Width; x++)
            {
                if (grid.TryGetMaterial(x, y, out var materialId) == false) continue;
                int index = x + y* grid.Width;
                pixels[index] = GetColor(materialId);
            }
        }
        texture.SetPixels32(pixels);
        texture.Apply(false);
    }

    private Color32 GetColor(byte materialId)
    {
        switch (materialId)
        {
            case 0:
                return new Color32(0, 0, 0, 0); // Empty
            case 1:
                return new Color32(235, 195, 70, 255);
            case 2:
                return new Color32(247, 215, 105, 255);
            case 3:
                return new Color32(195, 140, 38, 255);
            case 4:
                return new Color32(224, 175, 55, 255);
        }
        return new Color32(255, 0, 255, 255);
    }
}
