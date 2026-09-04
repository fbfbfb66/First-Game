using NUnit.Framework;

/// <summary>
/// SandGrid 的 EditMode 单元测试。
/// 每条测试只检查一个可观察的数据行为，不依赖场景或 GameObject。
/// </summary>
public class SandGridTests
{
    private const int Width = 6;
    private const int Height = 4;

    private SandGrid grid;

    [SetUp]
    public void SetUp()
    {
        grid = new SandGrid(Width, Height);
    }

    [Test]
    public void Constructor_WithValidSize_ExposesWidthAndHeight()
    {
        Assert.AreEqual(Width, grid.Width);
        Assert.AreEqual(Height, grid.Height);
    }

    [TestCase(0, 0)]
    [TestCase(5, 3)]
    [TestCase(2, 1)]
    public void IsInside_WithCoordinateInsideGrid_ReturnsTrue(int x, int y)
    {
        Assert.IsTrue(grid.IsInside(x, y));
    }

    [TestCase(-1, 0)]
    [TestCase(0, -1)]
    [TestCase(6, 3)]
    [TestCase(5, 4)]
    public void IsInside_WithCoordinateOutsideGrid_ReturnsFalse(int x, int y)
    {
        Assert.IsFalse(grid.IsInside(x, y));
    }

    [Test]
    public void NewlyCreatedGrid_AllCellsContainEmptyMaterialId()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Assert.IsTrue(grid.TryGetMaterial(x, y, out byte materialId));
                Assert.AreEqual(0, materialId, $"新 Grid 的 ({x}, {y}) 应该为空");
            }
        }
    }

    [Test]
    public void TrySetMaterial_InsideGrid_CanReadBackSameByte()
    {
        const byte RedSand = 2;

        Assert.IsTrue(grid.TrySetMaterial(2, 1, RedSand));
        Assert.IsTrue(grid.TryGetMaterial(2, 1, out byte materialId));
        Assert.AreEqual(RedSand, materialId);
    }

    [Test]
    public void TrySetMaterial_AtOneCell_DoesNotModifyNeighbourCell()
    {
        const byte YellowSand = 1;

        grid.TrySetMaterial(2, 1, YellowSand);

        grid.TryGetMaterial(2, 1, out byte writtenMaterialId);
        grid.TryGetMaterial(3, 1, out byte neighbourMaterialId);
        Assert.AreEqual(YellowSand, writtenMaterialId);
        Assert.AreEqual(0, neighbourMaterialId);
    }

    [Test]
    public void TrySetMaterial_WithZero_ClearsOccupiedCell()
    {
        grid.TrySetMaterial(2, 1, 1);

        Assert.IsTrue(grid.TrySetMaterial(2, 1, 0));
        Assert.IsTrue(grid.TryGetMaterial(2, 1, out byte materialId));
        Assert.AreEqual(0, materialId);
    }

    [Test]
    public void TrySetMaterial_WithByteMaxValue_CanReadBackSameValue()
    {
        const byte HighestMaterialId = byte.MaxValue;

        Assert.IsTrue(grid.TrySetMaterial(2, 1, HighestMaterialId));
        Assert.IsTrue(grid.TryGetMaterial(2, 1, out byte materialId));
        Assert.AreEqual(HighestMaterialId, materialId);
    }

    [TestCase(-1, 0)]
    [TestCase(0, -1)]
    [TestCase(6, 3)]
    [TestCase(5, 4)]
    public void TrySetMaterial_OutsideGrid_ReturnsFalse(int x, int y)
    {
        Assert.IsFalse(grid.TrySetMaterial(x, y, 1));
    }

    [TestCase(-1, 0)]
    [TestCase(0, -1)]
    [TestCase(6, 3)]
    [TestCase(5, 4)]
    public void TryGetMaterial_OutsideGrid_ReturnsFalseAndOutputsZero(int x, int y)
    {
        Assert.IsFalse(grid.TryGetMaterial(x, y, out byte materialId));
        Assert.AreEqual(0, materialId);
    }
}
