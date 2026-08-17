# FirstGame 开发日志

按开发阶段倒序记录。每条包含：做了什么、为什么这么做、学到了什么、下次从哪继续。
项目当前状态请看 [FirstGameDetails.md](FirstGameDetails.md)，本文件只记录过程。

---

## 2026-08-17 — 恢复开发 + 背包网格数据模型起步

### 背景

项目中断数月后恢复开发。本次先重新梳理整个项目，然后开始 Resident Evil 风格
二维网格背包系统的第一阶段：**纯数据模型**。

### 做了什么

**1. 项目恢复检查**

重新通读全项目（105 个脚本）与 `FirstGameDetails.md`，确认三条核心数据流：

```
输入流：InputSystem_Actions → GameInputReader → InputRouter
        → PlayerControlArbitration（问能不能做）
        → PlayerInputReceiver（缓存意图）→ PlayerState → PlayerMovement

对话流：交互键 → InteractionDetector → NPCDialogueInteractable
        → NPCDialogueProfile 按条件选对话 → DialogueManager
        → DialogueEventAction 改 Flag / Quest → 下次交互走不同分支

剧情流：StoryTrigger → StorySequenceRunner → StoryStepAction (SO) 顺序执行
```

背包当时的状态：`ItemData` / `ItemCategory` / `InventoryItem` / `InventoryGrid`
四个类已存在，但**全项目没有任何代码引用它们**，也没有任何物品资源，
场景里的背包 UI 是 32 个手摆的静态 Image，没有脚本。

**2. 引入 EditMode 单元测试（新工程实践）**

- 新建 `FirstGame.Inventory.asmdef`，把背包系统从 `Assembly-CSharp` 分离
- 新建 `Assets/_Game/Tests/EditMode/`，含测试程序集与 `InventoryGridTests.cs`

**为什么必须先做这一步**：Unity 的测试代码必须放在 asmdef 里，
而 asmdef **无法引用 `Assembly-CSharp`**。背包代码原本在 `Assembly-CSharp`，
所以不拆出来就没法写单元测试。

**3. 实现 `InventoryGrid` 的基础能力**

| 函数 | 作用 |
| --- | --- |
| `IsInside(x, y)` | 单点边界判断（原有，本次补了测试） |
| `IsInside(x, y, w, h)` | 矩形是否完整在网格内 |
| `IsAreaEmpty(x, y, w, h)` | 区域是否全空 |
| `Place(item, x, y)` | 放置物品，失败时不留半填状态 |
| `GetItemAt(x, y)` | 取某格物品，越界返回 null |

**4. 让背包在游戏里真的存在**

- 创建第一个物品资源 `Assets/_Game/Data/Items/GreenHerb.asset`（1×1，Consumable）
- 新建 `PlayerInventory` 组件挂到 `Player` 上，`Awake()` 里创建 4×8 网格
- 临时在 `Awake()` 放入草药并 `PrintGrid()` 打到 Console 验证

### 关键决策与理由

| 决策 | 理由 |
| --- | --- |
| `InventoryGrid` 用普通 C# 类，不是 MonoBehaviour | 不需要生命周期、不需要 Inspector；能被 `new` 出来，所以能脱离 Unity 测试 |
| `cells[x, y]` 存**引用**而非 bool | 存 bool 会丢失"这格是谁占的"，点击拾取就做不了 |
| 跨格物品的每一格存**同一个**对象引用 | 改一处即全部同步；且"移除物品"退化成扫描相等引用 |
| `IsAreaEmpty` 故意**不做**边界检查 | `IsInside` 已经在回答这个问题，职责不重叠，也避免重复检查 |
| `GetItemAt` 越界返回 null 而**不抛异常** | 调用方将来是鼠标位置，划出背包是正常情况不是错误 |
| `Place` 先全部校验再开始写 | 否则放置失败会留下"填了一半"的格子，数据不一致 |
| 尺寸取 `CurrentWidth/Height` 而非 `Data.Width/Height` | 前者已经包含旋转，Grid 不需要关心物品转没转 |
| 测试用 `JsonUtility.FromJsonOverwrite` 构造 `ItemData` | 不用为了测试放宽生产代码的封装 |
| 背包尺寸定为 4×8 | 数了 `GameScene` 里已有的 32 个 Slot，不是拍脑袋 |

### 学到的东西

**1. Play 模式跑通 ≠ 代码正确**

本次最有价值的一课。`IsAreaEmpty` 的循环写成了 `i <= areaWidth`（应为 `<`），
导致 1×1 的物品实际检查了 4 格。

- **点 Play：完全正常。** 多检查的格子恰好在数组范围内、恰好是 null，草药正常放入、正常打印。
- **跑单元测试：红的。** `IsAreaEmpty(9, 5, 1, 1)` 直接 `IndexOutOfRangeException`。

这个 bug 如果没有测试，会一直潜伏到玩家把物品放到背包最右一列才崩。

**2. 测试要测"行为"，不测"实现"**

所有测试只调用公开方法、检查返回值，没有任何一个去看 `cells` 内部长什么样。
这样将来把二维数组换成一维数组时，测试一条都不用改。

**3. 循环从 0 开始，偏移量直接就是 `i`**

从 1 开始会导致每次访问都要 `-1`，而这个 `-1` 正是网格代码最容易写错的地方。

**4. 什么该写单元测试**

> 答案唯一确定 → 单元测试（`InventoryGrid`、`GameFlagCenter`、`QuestManager`）
> 答案靠感觉 → 手测（跳跃手感、动画切换、UI 位置、拖拽高亮）

### 遗留问题

- `PlayerInventory.Awake()` 里的放置和打印是临时调试代码，接通拾取后要删
- `InventoryItem.Rotate()` 在物品已入网格后调用会造成数据不一致，做旋转交互前必须先解决
- `IsInside(x, y, w, h)` 对 w 或 h ≤ 0 会返回 true；当前由 `ItemData` 的 `[Range]` 在上游保证，暂不处理
- `InputRouter.OnDisable()` 在 `inputReader` 为 null 时抛 NRE（直接从 GameScene 开始 Play 会复现），与本次改动无关，未修
- `PlayerInputReceiver.RequestAttack()` / `RequestDash()` 只打日志没有置位，`ConsumeAttack/Dash` 永远返回 false，与本次改动无关，未修

### 下次从哪继续

当前功能链：

```
GreenHerb.asset  ✅
    ↓
PlayerInventory 持有 InventoryGrid  ✅
    ↓
Place() 写入格子  ✅
    ↓
Console 打印验证  ✅
    ↓
世界中的草药 + 按 E 拾取   ← 下一步在这里
    ↓
背包 UI 显示
```

下一个目标是完成**第一条纵向可运行链**：
在场景里放一株真实的草药，玩家走过去按 E，草药消失并进入背包。

这会用到项目里已有的 `IInteractable` / `InteractionDetector`（对话系统已经在用同一套）。
