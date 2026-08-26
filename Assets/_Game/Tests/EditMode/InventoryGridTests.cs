using System;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// InventoryGrid 的 EditMode 单元测试。
///
/// 这个类不需要挂在任何 GameObject 上，也不需要进 Play Mode。
/// 打开 Window → General → Test Runner → EditMode → Run All 即可运行。
/// </summary>
public class InventoryGridTests
{
    // 大部分测试都要用到一个 10×6 的网格，
    // 与其每个测试都写一遍，不如约定成常量。
    private const int Width = 10;
    private const int Height = 6;

    private InventoryGrid grid;

    /// <summary>
    /// [SetUp] 标记的方法会在【每一个】测试跑之前重新执行一次。
    ///
    /// 关键点：是"每一个之前"，不是"全部之前"。
    /// 所以每个测试拿到的都是一个全新的、干净的 grid，
    /// 测试之间不会互相污染。这叫测试的"隔离性"。
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        grid = new InventoryGrid(Width, Height);
    }

    // ------------------------------------------------------------
    // 构造函数
    // ------------------------------------------------------------

    /// <summary>
    /// [Test] 标记的方法就是一个测试。
    /// 命名约定：被测方法_测试场景_期望结果
    /// 名字要长要啰嗦 —— 因为测试失败时，Test Runner 只显示这个名字，
    /// 名字本身就该说清楚"什么坏了"。
    /// </summary>
    [Test]
    public void Constructor_WithValidSize_SetsWidthAndHeight()
    {
        // Assert.AreEqual(期望值, 实际值)
        // 断言：我认为这两个必须相等。不相等就让这个测试失败。
        Assert.AreEqual(Width, grid.Width);
        Assert.AreEqual(Height, grid.Height);
    }

    /// <summary>
    /// 你在构造函数里写了 ArgumentOutOfRangeException 的校验。
    /// 这个测试确认那段校验真的会触发。
    ///
    /// [TestCase(...)] 可以让同一个测试方法用不同参数跑多遍。
    /// 下面 4 行 = 4 个独立的测试，Test Runner 里会显示成 4 条。
    /// </summary>
    [TestCase(0, 6)]
    [TestCase(-1, 6)]
    [TestCase(10, 0)]
    [TestCase(10, -1)]
    public void Constructor_WithInvalidSize_Throws(int width, int height)
    {
        // Assert.Throws<T>(() => ...) 的意思是：
        // "我断言执行这段代码时，必须抛出 T 类型的异常。"
        // 如果没抛异常，或者抛了别的类型的异常，测试就失败。
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            new InventoryGrid(width, height);
        });
    }

    // ------------------------------------------------------------
    // IsInside —— 这是你已经写好的方法
    // ------------------------------------------------------------

    /// <summary>
    /// 合法坐标。特别注意 (9, 5)：
    /// Width = 10 意味着合法的 x 是 0..9，最大合法索引是 Width - 1。
    /// 这个"减一"是网格代码里最容易写错的地方，所以必须单独测。
    /// </summary>
    [TestCase(0, 0)]              // 左上角
    [TestCase(9, 0)]              // 右上角 (Width - 1)
    [TestCase(0, 5)]              // 左下角 (Height - 1)
    [TestCase(9, 5)]              // 右下角
    [TestCase(4, 3)]              // 中间随便一格
    public void IsInside_WithCoordinateInsideGrid_ReturnsTrue(int x, int y)
    {
        Assert.IsTrue(grid.IsInside(x, y));
    }

    /// <summary>
    /// 越界坐标。
    /// 注意 (10, 0) 和 (0, 6)：它们刚好差一格出界，
    /// 是最典型的 off-by-one 陷阱，必须覆盖。
    /// </summary>
    [TestCase(-1, 0)]             // x 负数
    [TestCase(0, -1)]             // y 负数
    [TestCase(10, 0)]             // x == Width，刚好出界
    [TestCase(0, 6)]              // y == Height，刚好出界
    [TestCase(-1, -1)]            // 两个都负
    [TestCase(10, 6)]             // 两个都出界
    [TestCase(100, 100)]          // 离谱的值
    public void IsInside_WithCoordinateOutsideGrid_ReturnsFalse(int x, int y)
    {
        Assert.IsFalse(grid.IsInside(x, y));
    }

    // ------------------------------------------------------------
    // Task 1.2：IsInside 的矩形版重载
    // 网格是 10 × 6，所以合法坐标是 x: 0..9, y: 0..5
    // ------------------------------------------------------------

    [TestCase(0, 0, 1, 1)]        // 左上角一格
    [TestCase(9, 5, 1, 1)]        // 右下角一格
    [TestCase(0, 0, 4, 2)]        // 4×2 步枪放左上
    [TestCase(6, 4, 4, 2)]        // 4×2 步枪，右下角刚好是 (9,5)
    [TestCase(0, 0, 10, 6)]       // 占满整个网格
    [TestCase(8, 0, 2, 6)]        // 贴着右边的竖条
    public void IsInside_WithRectFullyInsideGrid_ReturnsTrue(int x, int y, int w, int h)
    {
        Assert.IsTrue(grid.IsInside(x, y, w, h));
    }

    [TestCase(7, 4, 4, 2)]        // 右边超出 1 格 (最右到 x=10)
    [TestCase(6, 5, 4, 2)]        // 下边超出 1 格 (最下到 y=6)
    [TestCase(-1, 0, 4, 2)]       // 左边超出
    [TestCase(0, -1, 4, 2)]       // 上边超出
    [TestCase(0, 0, 11, 6)]       // 比网格还宽
    [TestCase(0, 0, 10, 7)]       // 比网格还高
    [TestCase(10, 5, 1, 1)]       // 起点本身就在外面
    [TestCase(100, 100, 1, 1)]    // 离谱的值
    public void IsInside_WithRectOutsideGrid_ReturnsFalse(int x, int y, int w, int h)
    {
        Assert.IsFalse(grid.IsInside(x, y, w, h));
    }

    // ------------------------------------------------------------
    // Task 1.3：IsAreaEmpty
    //
    // 注意：这里只测得了"空网格上一切都是空的"。
    // "区域里有东西所以返回 false" 的用例，要等 Place 写完
    // （Task 1.5）才能构造出来，到时候回来补。
    // ------------------------------------------------------------

    [TestCase(0, 0, 1, 1)]
    [TestCase(9, 5, 1, 1)]
    [TestCase(0, 0, 4, 2)]
    [TestCase(6, 4, 4, 2)]
    [TestCase(0, 0, 10, 6)]       // 整个网格
    public void IsAreaEmpty_OnEmptyGrid_ReturnsTrue(int x, int y, int w, int h)
    {
        Assert.IsTrue(grid.IsAreaEmpty(x, y, w, h));
    }

    // ------------------------------------------------------------
    // 造测试用物品
    //
    // ItemData 的 width / height 是 [SerializeField] private，
    // 测试代码没法直接赋值。
    // JsonUtility.FromJsonOverwrite 可以写入被序列化的私有字段，
    // 这样就不用为了测试去改动 ItemData 的封装。
    // ------------------------------------------------------------
    private static ItemData CreateItemData(int width, int height, int maxStack = 1)
    {
        var data = ScriptableObject.CreateInstance<ItemData>();
        JsonUtility.FromJsonOverwrite(
            $"{{\"itemId\":\"test\",\"displayName\":\"Test\",\"width\":{width},\"height\":{height},\"maxStack\":{maxStack}}}",
            data);
        return data;
    }

    private static InventoryItem CreateItem(int width, int height)
    {
        return new InventoryItem(CreateItemData(width, height), 1);
    }

    // ------------------------------------------------------------
    // IsAreaEmpty —— 补上"区域被占"的用例（现在有 Place 了）
    // ------------------------------------------------------------

    [Test]
    public void IsAreaEmpty_WhenAreaIsOccupied_ReturnsFalse()
    {
        grid.Place(CreateItem(2, 2), 2, 2);        // 占住 x:2~3, y:2~3

        Assert.IsFalse(grid.IsAreaEmpty(2, 2, 1, 1));   // 正好压在物品上
        Assert.IsFalse(grid.IsAreaEmpty(3, 3, 1, 1));   // 物品的另一角
        Assert.IsFalse(grid.IsAreaEmpty(1, 1, 3, 3));   // 大区域包住了物品
    }

    [Test]
    public void IsAreaEmpty_WhenAreaIsNextToItem_ReturnsTrue()
    {
        grid.Place(CreateItem(2, 2), 2, 2);        // 占住 x:2~3, y:2~3

        Assert.IsTrue(grid.IsAreaEmpty(0, 0, 2, 2));   // 完全不挨着
        Assert.IsTrue(grid.IsAreaEmpty(4, 2, 1, 1));   // 紧贴右边但不重叠
        Assert.IsTrue(grid.IsAreaEmpty(2, 4, 1, 1));   // 紧贴下边但不重叠
    }

    // ------------------------------------------------------------
    // Place
    // ------------------------------------------------------------

    [Test]
    public void Place_WithNullItem_ReturnsFalse()
    {
        Assert.IsFalse(grid.Place(null, 0, 0));
    }

    [Test]
    public void Place_OnEmptyGrid_FillsEveryCoveredCell()
    {
        InventoryItem rifle = CreateItem(4, 2);

        Assert.IsTrue(grid.Place(rifle, 0, 0));

        // 覆盖到的 8 格，全都要指向【同一个】对象
        for (int x = 0; x < 4; x++)
            for (int y = 0; y < 2; y++)
                Assert.AreSame(rifle, grid.GetItemAt(x, y), $"({x},{y}) 应该是这把步枪");

        // 边界外的格子必须还是空的
        Assert.IsNull(grid.GetItemAt(4, 0));
        Assert.IsNull(grid.GetItemAt(0, 2));
    }

    [TestCase(7, 0)]              // 右边放不下
    [TestCase(0, 5)]              // 下边放不下
    [TestCase(-1, 0)]             // 起点越界
    public void Place_OutsideGrid_ReturnsFalse(int x, int y)
    {
        Assert.IsFalse(grid.Place(CreateItem(4, 2), x, y));
    }

    [Test]
    public void Place_OnOccupiedArea_ReturnsFalse()
    {
        grid.Place(CreateItem(4, 2), 0, 0);

        Assert.IsFalse(grid.Place(CreateItem(1, 1), 3, 1));   // 压在步枪身上
    }

    /// <summary>
    /// 最重要的一条：放置失败时，网格必须保持原样，
    /// 不能留下"填了一半"的格子。
    /// </summary>
    [Test]
    public void Place_WhenItFails_LeavesGridUnchanged()
    {
        InventoryItem herb = CreateItem(1, 1);
        grid.Place(herb, 3, 0);                    // 先在 (3,0) 放一株草药

        InventoryItem rifle = CreateItem(4, 2);
        Assert.IsFalse(grid.Place(rifle, 0, 0));   // 会撞上草药，必须失败

        // 步枪一格都不能留下
        Assert.IsNull(grid.GetItemAt(0, 0));
        Assert.IsNull(grid.GetItemAt(1, 0));
        Assert.IsNull(grid.GetItemAt(2, 0));
        // 草药必须完好无损
        Assert.AreSame(herb, grid.GetItemAt(3, 0));
    }

    // ------------------------------------------------------------
    // GetItemAt
    // ------------------------------------------------------------

    [TestCase(-1, 0)]
    [TestCase(0, -1)]
    [TestCase(10, 0)]
    [TestCase(0, 6)]
    public void GetItemAt_OutsideGrid_ReturnsNullInsteadOfThrowing(int x, int y)
    {
        Assert.IsNull(grid.GetItemAt(x, y));
    }

    // ------------------------------------------------------------
    // Remove
    // ------------------------------------------------------------

    /// <summary>
    /// 物品占的每一格都要被擦干净，不能只擦左上角。
    /// 这是 Remove 存在的全部意义：拖动搬家时，
    /// 旧位置一格残留都会变成抓得到、看不见的"幽灵"。
    /// </summary>
    [Test]
    public void Remove_ExistingItem_ClearsEveryCellItOccupied()
    {
        InventoryItem rifle = CreateItem(2, 1);
        grid.Place(rifle, 3, 2);

        Assert.IsTrue(grid.Remove(rifle));

        Assert.IsNull(grid.GetItemAt(3, 2));
        Assert.IsNull(grid.GetItemAt(4, 2));
    }

    /// <summary>
    /// 擦干净的直接证据：同一片区域必须能重新放东西。
    /// 只要还剩一格残留，这个 Place 就会失败。
    /// </summary>
    [Test]
    public void Remove_ThenPlaceAtSameSpot_Succeeds()
    {
        InventoryItem rifle = CreateItem(2, 1);
        grid.Place(rifle, 3, 2);
        grid.Remove(rifle);

        InventoryItem herb = CreateItem(2, 1);
        Assert.IsTrue(grid.Place(herb, 3, 2));
        Assert.AreSame(herb, grid.GetItemAt(3, 2));
    }

    /// <summary>
    /// 只能擦自己的格子，不能误伤邻居。
    /// </summary>
    [Test]
    public void Remove_DoesNotTouchOtherItems()
    {
        InventoryItem rifle = CreateItem(2, 1);
        InventoryItem herb = CreateItem(1, 1);
        grid.Place(rifle, 3, 2);
        grid.Place(herb, 5, 2);

        grid.Remove(rifle);

        Assert.AreSame(herb, grid.GetItemAt(5, 2));
    }

    /// <summary>
    /// 返回值必须诚实：网格里没有这件物品，就不能说"移除成功"。
    /// 拖放流程是"先 Remove 再 Place"，
    /// 这里说谎会让我们在一个不该发生的状态上继续往下走。
    /// </summary>
    [Test]
    public void Remove_ItemNotInGrid_ReturnsFalse()
    {
        grid.Place(CreateItem(2, 1), 3, 2);

        Assert.IsFalse(grid.Remove(CreateItem(2, 1)));   // 长得一样，但不是同一个引用
    }

    [Test]
    public void Remove_Null_ReturnsFalseInsteadOfThrowing()
    {
        Assert.IsFalse(grid.Remove(null));
    }

    // ------------------------------------------------------------
    // CanPlace
    // ------------------------------------------------------------

    [Test]
    public void CanPlace_OnEmptyArea_ReturnsTrue()
    {
        Assert.IsTrue(grid.CanPlace(CreateItem(2, 2), 3, 2));
    }

    [Test]
    public void CanPlace_OverlappingAnotherItem_ReturnsFalse()
    {
        grid.Place(CreateItem(4, 2), 0, 0);

        Assert.IsFalse(grid.CanPlace(CreateItem(1, 1), 3, 1));
    }

    [TestCase(7, 0)]
    [TestCase(0, 5)]
    [TestCase(-1, 0)]
    public void CanPlace_OutsideGrid_ReturnsFalse(int x, int y)
    {
        Assert.IsFalse(grid.CanPlace(CreateItem(4, 2), x, y));
    }

    [Test]
    public void CanPlace_Null_ReturnsFalseInsteadOfThrowing()
    {
        Assert.IsFalse(grid.CanPlace(null, 0, 0));
    }

    /// <summary>
    /// CanPlace 存在的核心理由：拖动时物品还躺在原位，
    /// 它必须能看穿"挡路的其实是我自己"。
    /// 2×2 往右挪一格，新旧区域重叠 —— 不忽略自己就永远搬不动。
    /// </summary>
    [Test]
    public void CanPlace_OverlappingItself_WithIgnore_ReturnsTrue()
    {
        InventoryItem handgun = CreateItem(2, 2);
        grid.Place(handgun, 0, 0);

        Assert.IsTrue(grid.CanPlace(handgun, 1, 0, ignoreItem: true));
    }

    /// <summary>
    /// 反过来钉住默认行为：不传 ignoreItem 时，
    /// 自己占的格子照样算"被占用"。
    /// 新物品入包（拾取）走的就是这条路，不能被放宽。
    /// </summary>
    [Test]
    public void CanPlace_OverlappingItself_WithoutIgnore_ReturnsFalse()
    {
        InventoryItem handgun = CreateItem(2, 2);
        grid.Place(handgun, 0, 0);

        Assert.IsFalse(grid.CanPlace(handgun, 1, 0));
    }

    /// <summary>
    /// CanPlace 是一个"查询"，不是"命令"。
    /// 拖动时每帧都会调它来刷新绿/红预览，
    /// 它要是偷偷改了网格，那就是每帧都在破坏数据。
    /// </summary>
    [Test]
    public void CanPlace_DoesNotModifyGrid()
    {
        InventoryItem herb = CreateItem(1, 1);
        grid.Place(herb, 3, 0);

        grid.CanPlace(CreateItem(2, 2), 3, 0);
        grid.CanPlace(CreateItem(2, 2), 6, 4);

        Assert.AreSame(herb, grid.GetItemAt(3, 0));
        Assert.IsNull(grid.GetItemAt(6, 4));
    }

    /// <summary>
    /// 把"搬家"的完整三步走一遍：问 → 擦 → 放。
    /// 这是 InventoryView.EndDrag 里真实发生的流程，
    /// 只是这里没有鼠标和 UI。
    /// </summary>
    [Test]
    public void MoveItem_ByOneCell_KeepsExactlyOneCopyInGrid()
    {
        InventoryItem handgun = CreateItem(2, 2);
        grid.Place(handgun, 0, 0);

        Assert.IsTrue(grid.CanPlace(handgun, 1, 0, ignoreItem: true));
        grid.Remove(handgun);
        Assert.IsTrue(grid.Place(handgun, 1, 0));

        // 新位置四格都是它
        Assert.AreSame(handgun, grid.GetItemAt(1, 0));
        Assert.AreSame(handgun, grid.GetItemAt(2, 1));
        // 旧位置不能留幽灵
        Assert.IsNull(grid.GetItemAt(0, 0));
        Assert.IsNull(grid.GetItemAt(0, 1));
    }
}
