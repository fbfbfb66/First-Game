using NUnit.Framework;
using UnityEngine;

/// <summary>
/// PlayerInventory.TryAdd 的 EditMode 单元测试。
///
/// PlayerInventory 是 MonoBehaviour，但 EditMode 测试照样能测它：
/// new GameObject().AddComponent<T>() 在编辑器里是合法的，
/// 只是 Awake / Start / Update 这些生命周期回调不会像 Play 模式那样跑。
/// 这里正好不受影响 —— grid 是惰性创建的，谁先访问谁负责建。
/// </summary>
public class PlayerInventoryTests
{
    private const int Width = 2;
    private const int Height = 2;

    private GameObject host;
    private PlayerInventory inventory;

    [SetUp]
    public void SetUp()
    {
        host = new GameObject("PlayerInventoryTestHost");
        inventory = host.AddComponent<PlayerInventory>();

        // width / height 同样是 [SerializeField] private，
        // 用和 ItemData 一样的手法写进去。默认 4×8 = 32 格，
        // 测「背包放满」要捡 32 次，缩成 2×2 = 4 格测起来干脆得多。
        JsonUtility.FromJsonOverwrite($"{{\"width\":{Width},\"height\":{Height}}}", inventory);
    }

    [TearDown]
    public void TearDown()
    {
        // 测试之间必须销毁，否则 GameObject 会一直堆在编辑器场景里。
        Object.DestroyImmediate(host);
    }

    private static ItemData CreateItemData(int maxStack)
    {
        var data = ScriptableObject.CreateInstance<ItemData>();
        JsonUtility.FromJsonOverwrite(
            $"{{\"itemId\":\"test\",\"displayName\":\"Test\",\"width\":1,\"height\":1,\"maxStack\":{maxStack}}}",
            data);
        return data;
    }

    // ------------------------------------------------------------
    // 基本放入
    // ------------------------------------------------------------

    [Test]
    public void TryAdd_IntoEmptyInventory_PlacesItemAtFirstCell()
    {
        ItemData herb = CreateItemData(maxStack: 3);

        Assert.IsTrue(inventory.TryAdd(herb));

        Assert.AreSame(herb, inventory.GetItemAt(0, 0).Data);
        Assert.AreEqual(1, inventory.GetItemAt(0, 0).Amount);
    }

    /// <summary>
    /// 堆叠的核心：第二株草药不该占新格子。
    /// </summary>
    [Test]
    public void TryAdd_SameItemTwice_StacksInsteadOfTakingANewCell()
    {
        ItemData herb = CreateItemData(maxStack: 3);

        inventory.TryAdd(herb);
        inventory.TryAdd(herb);

        Assert.AreEqual(2, inventory.GetItemAt(0, 0).Amount);
        Assert.IsNull(inventory.GetItemAt(1, 0), "第二株应该堆进第一格，不该占用新格子");
    }

    /// <summary>
    /// 堆满之后才另起一格。
    /// </summary>
    [Test]
    public void TryAdd_OnceStackIsFull_StartsANewStack()
    {
        ItemData herb = CreateItemData(maxStack: 3);

        for (int i = 0; i < 4; i++)
            inventory.TryAdd(herb);

        Assert.AreEqual(3, inventory.GetItemAt(0, 0).Amount);
        Assert.AreEqual(1, inventory.GetItemAt(1, 0).Amount);
    }

    /// <summary>
    /// 不同的 ItemData 不能互相堆叠。
    /// </summary>
    [Test]
    public void TryAdd_DifferentItemData_DoesNotStack()
    {
        ItemData herb = CreateItemData(maxStack: 3);
        ItemData ammo = CreateItemData(maxStack: 3);

        inventory.TryAdd(herb);
        inventory.TryAdd(ammo);

        Assert.AreEqual(1, inventory.GetItemAt(0, 0).Amount);
        Assert.AreSame(ammo, inventory.GetItemAt(1, 0).Data);
    }

    // ------------------------------------------------------------
    // 一次捡起一堆 —— 当前会挂在这里
    // ------------------------------------------------------------

    /// <summary>
    /// 玩家从地上捡起一堆 5 株草药，而 MaxStack 是 3。
    ///
    /// 现在 TryAdd 第一行就 new InventoryItem(data, 5)，
    /// 构造函数校验 amount > MaxStack 直接抛异常 ——
    /// 连堆叠分支都没走到。
    /// </summary>
    [Test]
    public void TryAdd_MoreThanMaxStackAtOnce_DoesNotThrow()
    {
        ItemData herb = CreateItemData(maxStack: 3);

        Assert.DoesNotThrow(() => inventory.TryAdd(herb, 5));
    }

    /// <summary>
    /// 5 株、MaxStack 3、背包 2×2 —— 应该拆成 3 + 2 放进两格。
    /// </summary>
    [Test]
    public void TryAdd_MoreThanMaxStackAtOnce_SplitsAcrossCells()
    {
        ItemData herb = CreateItemData(maxStack: 3);

        inventory.TryAdd(herb, 5);

        Assert.AreEqual(3, inventory.GetItemAt(0, 0).Amount);
        Assert.AreEqual(2, inventory.GetItemAt(1, 0).Amount);
    }

    /// <summary>
    /// 已有一堆 2 个，再捡 2 个：第一格补满到 3，剩下 1 个另起一格。
    /// 验证「堆叠分支的溢出」和「新开格」能接上。
    /// </summary>
    [Test]
    public void TryAdd_OverflowFromStacking_GoesIntoNextCell()
    {
        ItemData herb = CreateItemData(maxStack: 3);
        inventory.TryAdd(herb, 2);

        inventory.TryAdd(herb, 2);

        Assert.AreEqual(3, inventory.GetItemAt(0, 0).Amount);
        Assert.AreEqual(1, inventory.GetItemAt(1, 0).Amount);
    }

    // ------------------------------------------------------------
    // 放不下
    // ------------------------------------------------------------

    /// <summary>
    /// 2×2 的背包、MaxStack 1、四种不同物品塞满，第五件必须被拒绝。
    /// 关键是【不能抛异常】—— 背包满了是正常游戏状态，不是程序错误。
    /// </summary>
    [Test]
    public void TryAdd_WhenInventoryIsFull_ReturnsFalse()
    {
        for (int i = 0; i < Width * Height; i++)
            Assert.IsTrue(inventory.TryAdd(CreateItemData(maxStack: 1)));

        Assert.IsFalse(inventory.TryAdd(CreateItemData(maxStack: 1)));
    }

    // ------------------------------------------------------------
    // 事件
    // ------------------------------------------------------------

    /// <summary>
    /// 事件的分工：占用新格子喊 ItemPlaced（UI 要新建一个 ItemView），
    /// 堆进已有物品喊 ItemAmountUpdated（UI 只要改数字）。
    /// 喊错了，屏幕上就会多出一个不存在的物品，或者数字永远不变。
    /// </summary>
    [Test]
    public void TryAdd_RaisesPlacedForNewCell_AndAmountUpdatedForStacking()
    {
        ItemData herb = CreateItemData(maxStack: 3);
        int placedCount = 0;
        int amountUpdatedCount = 0;
        inventory.ItemPlaced += (item, x, y) => placedCount++;
        inventory.ItemAmountUpdated += item => amountUpdatedCount++;

        inventory.TryAdd(herb);
        Assert.AreEqual(1, placedCount, "第一株占了新格子");
        Assert.AreEqual(0, amountUpdatedCount);

        inventory.TryAdd(herb);
        Assert.AreEqual(1, placedCount, "第二株是堆叠，不该再喊 ItemPlaced");
        Assert.AreEqual(1, amountUpdatedCount);
    }
}
