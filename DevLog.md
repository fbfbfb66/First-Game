# FirstGame 开发日志

按开发阶段倒序记录。每条包含：做了什么、为什么这么做、学到了什么、下次从哪继续。
项目当前状态请看 [FirstGameDetails.md](FirstGameDetails.md)，本文件只记录过程。

---

## 2026-09-02 — 二段跳、翻滚落地与空中状态边界修复

### 背景

本阶段补全玩家跳跃纵向切片：空中再次按跳跃触发二段跳，有水平输入播放
`DoubleForwardJump`，没有水平输入播放 `DoubleVerticalJump`；下落速度达到阈值后，
落地进入 `RollingLand`。

动画资源已经准备好，真正的难点不是播放动画，而是让一次滞空中的输入额度、物理速度、
地面检测和多个状态之间保持同一份事实。开发中连续遇到数个只在边界帧出现的状态机 Bug，
因此本条日志重点记录排查过程。

### 做了什么

**1. 二段跳行为**

- 新增 `Player_DoubleJump`，进入时读取当下的 `MoveInput.x` 作为动画与方向快照
- `PlayerMovement.HandleDoubleJump(Vector2)` 负责施加二段跳速度
- 无水平输入时明确把 X 速度归零；有输入时按输入方向叠加当前水平速度
- `JumpUp`、`Apex`、`Fall` 都能消费二段跳输入
- 新增 `DoubleForwardJump` / `DoubleVerticalJump` Animator hash 和动画状态

**2. 一次滞空只能使用一次二段跳**

`Player` 持有 `canDoubleJump`，通过 `TryConsumeDoubleJump()` 原子检查并消耗额度。
额度不属于 JumpUp、Apex 或 Fall 中的任何单一状态，因为它必须跨越整次滞空。

恢复额度也不再依赖“进入某个地面类”，而是在 `PlayerAir.TryHandleLanding()` 确认真正完成
落地转换后统一调用 `ResetDoubleJump()`。墙滑仍按当前设计主动恢复一次二段跳。

**3. Ground 与 Fall 的边缘缓冲**

- `FallEnterVelocityThreshold` 当前为 `7`：Ground/Run 离开平台后，向下速度达到 `-7`
  才进入 Fall，用一个很短的速度窗口过滤平台边缘检测抖动
- Apex 到 Fall 保持自己的 `ApexThreshold`，两种阈值不混用
- Coyote Time 普通跳跃仍优先于 Fall 中的二段跳消费

**4. 双层地面判定**

两条 GroundSensor 射线现在承担不同语义：

| 属性 | 条件 | 用途 |
| --- | --- | --- |
| `IsGrounded` | 任意一条射线命中 | 判断是否离开地面、刷新 Coyote Time |
| `CanEnterGrounded` | 两条射线都命中 | 允许空中状态真正进入地面状态 |

这样站在平台边缘时仍可保留支撑，而贴墙时单侧射线命中墙体不会把角色切到 Idle。
代价是特别窄、只能承托一条射线的平台暂时不能作为有效落地点。

**5. Rolling Land**

- `Player_Fall` 保存落地前的下落速度
- 当前阈值为 `-35`，达到后进入 `Player_RollingLand`
- Forward/Vertical 二段跳不强制翻滚，统一由真实下落速度决定
- Rolling Land 动画期间按角色朝向维持水平速度，最低速度为 `5`
- 动画的 `StartAnimation` / `EndAnimation` Event 控制状态结束

**6. 动画资源整理**

接入二段跳与 Rolling Land 动画、Animator 状态和事件；同步更新相关跳跃/翻滚 Sprite
切片元数据。`Hurt`、`PowerUp` 动画片段已导入但尚未接入 Gameplay 状态。

### 头疼 Bug 复盘

| 现象 | 根因与证据 | 最终处理 |
| --- | --- | --- |
| Fall 落地后不切状态 | 改写 `Player_Fall.LogicalUpdate()` 时漏掉 `base.LogicalUpdate()`，导致 `GroundSensor.UpdateGroundState()` 根本没有运行，`IsGrounded` 一直是旧值 | 恢复公共更新链，并让 `PlayerAir` 统一刷新传感器与处理落地 |
| 速度只有约 `-28`，阈值为 `-35` 却仍进入 RollingLand | `PlayerAir.LogicalUpdate()` 先调用虚方法 `TryHandleLanding()`；实际动态分派到 Fall 的重写版本，而当时重写版本无条件进入 RollingLand，后面的阈值判断根本来不及执行 | 让 Fall 的 `TryHandleLanding()` 自己完成“翻滚或普通落地”的完整决策 |
| 可以无限二段跳 | JumpUp/Apex/Fall 都能消费 Jump，但没有跨状态保存“本轮已经用过”的事实 | 把额度放到 `Player`，使用 `TryConsumeDoubleJump()`，成功一次后保持不可用直到真实落地 |
| 左右跳墙时空中闪一下 Idle，再进入 WallSlide | GroundSensor 的地面 Mask 包含墙层，且项目允许 Raycast 从 Collider 内部开始命中；贴墙时一侧射线会把墙误报成地面 | 分离 `IsGrounded` 与 `CanEnterGrounded`：离地用单射线，进入地面要求双射线 |
| 落地后偶尔下一跳只能一段跳 | 二段跳恢复最初写在 `PlayerGround.Enter()`，但 Run 系状态继承 `Player_RunTransition → PlayerState`，按住方向落地到 Run 时完全绕过 PlayerGround | 把恢复时机移到 `PlayerAir` 确认成功落地之后，Idle/Walk/Run/RollingLand 共用同一出口 |
| Vertical 二段跳可能突然向左窜 | 水平输入为 0 时旧公式仍进入负方向分支，得到 `-Abs(currentX)` | 无水平输入时直接把 X 速度设为 0，再施加垂直速度 |

### 关键决策与理由

| 决策 | 理由 |
| --- | --- |
| 二段跳动画类型在进入状态时决定 | 输入是触发瞬间的意图；播放途中松键不应让动画类型变化 |
| 二段跳额度由 `Player` 持有 | 数据生命周期覆盖多个状态，放进某个状态对象会让别的状态看不到同一事实 |
| 额度在“成功落地”后恢复 | 落地可能进入 Idle、Walk、Run 或 RollingLand，依赖具体继承链必然漏分支 |
| `FallEnterVelocityThreshold` 使用正数配置 | Inspector 中 `7` 表示“向下速度绝对值 7”，代码比较 `velocityY <= -threshold`，比填写负数更直观 |
| Rolling Land 只看落地速度 | Forward 二段跳不再享有特殊落地规则，所有来源使用同一套物理标准 |
| GroundSensor 保留宽松与严格两个结果 | “是否失去支撑”和“是否足以确认落地”是两个不同问题，不该共用一个 bool |

### 验证结果

- `Assembly-CSharp.csproj` 编译通过，0 Error
- Unity Play Mode 手测通过：Forward/Vertical 二段跳、第三次按键无效、落地恢复额度
- Idle、Run、RollingLand 三种落地出口后均能再次二段跳
- 平台边缘离地、Coyote Jump、左右墙跳与 WallSlide 未再观察到状态闪切
- 高/低下落速度分别验证 RollingLand 与普通落地分支

### 学到了什么

- **先找状态转换发生在哪一行，再看条件为什么错。** 条件本身可能完全正确，但状态早已在父类调用中切走。
- **虚方法会动态分派。** `base.LogicalUpdate()` 写在 PlayerAir，调用的仍可能是 Player_Fall 的重写版本。
- **共享数据要跟生命周期走。** 一次滞空的数据不属于某一个动画状态，落地恢复也不属于某一种地面类。
- **边界检测通常需要两个答案。** “至少一只脚还在平台上”和“两只脚足以确认落地”不能用同一判断表达。
- **偶发 Bug 优先怀疑时序与遗漏路径。** Idle 落地正常、Run 落地异常，关键不是随机，而是两条继承链不同。

### 遗留问题

- `PlayerAir.Enter()` 会在进入每个空中状态时清除 Jump Buffer；目前手测未观察到丢输入，但若以后出现只在 JumpUp/Apex/Fall 边界丢失按键，应优先检查这里
- 双射线严格落地暂不支持比两射线间距更窄的平台；需要时再升级为落点法线、距离或脚底 ShapeCast
- `BaseLand` 动画 hash 已存在，普通落地目前直接回到移动状态，尚未接入独立 BaseLand 状态
- `Hurt`、`PowerUp` 动画资源已准备，但没有对应输入、状态或 Gameplay 效果

### 下次从哪继续

二段跳与翻滚落地纵向切片已经闭环。下一阶段可以从一个新的可观察行为开始，优先候选是：

```
受到伤害 → HP 变化 → Hurt 状态/动画 → 受击结束恢复控制
```

或者继续此前计划的攻击纵向切片；两者都应先只打通一次输入/事件到状态与动画，不提前搭完整战斗架构。

---

## 2026-08-30 — 右键菜单改为常驻详情面板

### 背景

物品详情原本是右键弹出的 `ItemContextMenu`：跟着鼠标定位，靠一块全屏透明 `Blocker`
接住"点在菜单外面"那一下来关闭。

想要的是主流 RPG 那种布局——左边网格、右边一块**永远在**的详情区，点中哪件就显示哪件，
并且被点中的物品在网格里有选中框。

表面上像是"把菜单钉住不动"，实际上生命周期整个变了，这是这次所有坑的来源。

### 做了什么

分六个阶段推进，每阶段一个 Checkpoint 才往下走。

**1. 选中状态**

`InventoryView` 新增 `selectedItem`，左键点击格子写入。这是第三种物品状态：

| 状态 | 生命周期 |
| --- | --- |
| `hoveredItem` | 鼠标移开就没 |
| `dragItem` | 只在拖拽过程中存在 |
| `selectedItem` | 玩家主动点击产生，**一直保持**到点别处或它本身消失 |

之前不需要它，是因为"菜单开着"本身就是隐式的选中状态。

**2. `ItemDetailPanel`**

新脚本，图标 / 名字 / 类别 / 描述。`Basic` 与 `FallBackLabel` 两组子物体互斥切换表达空态，
**面板本体永不隐藏**。

**3. 选中框**

`InventoryGrid.TryGetItemPosition` —— `GetItemAt` 的反方向查询，
`PlayerInventory` 加一行转发。`SelectedBoard` 复用 `PlacementPreview` 的那套摆放代码。

**4. 单一入口 `SetSelectedItem`**

把散在三个分支里的"改字段 + 刷面板 + 刷选中框"合并，并接上另外三个来源。

**5. 迁移操作区**

丢弃按钮与数量选择器从菜单搬进面板的 `ActionRow`（放在 `Basic` 里面，空态时随之消失）。

**6. 清理**

删除 `ItemContextMenu.cs`、`ContextMenuBlocker.cs` 与场景里的两个对象，
`InventoryPointerHandler.OnPointerClick` 从右键改为左键。

### 关键决策与理由

| 决策 | 理由 |
| --- | --- |
| `selectedItem` 只在 `SetSelectedItem` 里赋值 | 选中的来源有 4 个。让每个来源各自手抄"改字段 + 刷两处 UI"，漏一处就是"UI 显示了不存在的东西"，且不报错 |
| 选中框位置用 `TryGetItemPosition` 而非点击格 | 点 2×1 物品的右半格时，两者差一整格，框会歪出去 |
| 反查方法写在 `InventoryGrid` 而不是 `InventoryItem` | 位置这本账属于网格。物品不知道、也不该知道自己在哪 |
| 面板发 `DropRequested`，不自己调 `TryRemove` | 沿用旧菜单的边界。结果是这条连线整次改版一行没改 |
| `ActionRow` 放进 `Basic` 内部 | 否则会出现"没选任何物品，却有一个可点的丢弃按钮" |
| 数量刷新走 `ItemAmountUpdated` 事件，不在丢弃后手动刷 | 数量还会因堆叠合并、使用消耗、任务扣除而变。事件是所有路径的共同出口 |
| `ItemContextMenu` 直接删除，不留着当"备用方案" | 两者共用同一批按钮对象，不可能并存；留下的会是永远不会被执行的死代码 |

### 学到的东西

**1. 弹出式改常驻式，丢掉的是"免费重置"**

这是本次最核心的一课。

`Open()` 每次都是一轮**新会话**，所以旧菜单从来不用操心 `currentAmount` 残留 ——
它每次弹出都天然重新初始化了一遍。

常驻面板从头到尾**只有一个实例、永不重新初始化**，上一件物品的选择量会原样带到下一件。
触发路径：选中 10 个的药水 → 数量调到 5 → 直接点旁边只有 2 个的物品 → 拿着 5 去丢一个只有 2 个的堆。

所以 `Show()` 里必须手写 `currentAmount = 1`。这和 `BeginDrag` 里的 `dragRotated = false`
是同一条规则：**会话开始时必须清干净上一轮的会话状态**。

**2. 状态和它的表现被拆开维护，就一定会不同步**

阶段 4 一次性暴露了三个 bug，来源不同、根因相同：

| 现象 | 漏掉的来源 |
| --- | --- |
| 丢光选中的物品后，面板和框还在 | `OnItemRemoved` |
| 关背包再开，面板残留上次内容 | `OnDisable` |
| 拖动选中的物品后，框留在原地 | `EndDrag` |

三处都不报错，因为数据全是对的，错的只有表现。

**3. NullReferenceException 分两种，看一眼就能分**

跳到出错那一行，问：这个引用是**运行时算出来的**，还是 **Inspector 拖进来的**？

- 运行时算出来的 → 逻辑 bug，改代码
- `[SerializeField]` 字段 → 99% 是忘了拖引用，改代码没用

这次是后者（`detailPanel` 没拖）。**加一个 `[SerializeField]` 字段 = 欠一次拖拽。**

**4. 一处遗漏可以伪装成两个 bug**

`OnPlusButton` 漏了 `RefreshAmount()`，现象却是"加减按钮**都**没反应"：

```
加号：currentAmount 其实加对了，只是标签没重画
减号：interactable 只在 RefreshAmount 里重算，
      从 Show 时的 false 再没被解锁过 —— 死锁
```

排查时先分清是**数据错了**还是**数据对但没画出来**，能省一半时间。

**5. 早退守卫会静默吞掉调用**

`ShowSelectedBoard` 开头有 `if (dragItem != null) return;`，
而 `EndDrag` 里 `dragItem = null;` 是最后一行。
刷新选中框那句放在它前面就会被吞掉，症状是"拖完框不动"且没有任何报错。

**顺序敏感的调用，不报错才是最难查的。**

### 遗留问题

- 图中的 `F 使用` 未做。"使用一件物品"要回答"用了会发生什么"，`ItemData` 没有任何这类信息，属于独立的物品效果系统
- `categoryLabel` 直接输出 `ItemCategory` 的英文枚举名（`Consumable`），中文显示待本地化
- 场景里 `CategoryLabel` 这个 GameObject 名字末尾有两个多余空格
- 7 个 TMP 字体资源（`m_AtlasPopulationMode: 1`，Dynamic）是运行时烘焙中文字形后回写的图集缓存，不是手动改动，未提交
- `Assets/_Game/Art/UI/Icy Fantasy RPG UI Asset Sheet.png` 尚未被任何场景引用，未提交
- 本文件缺 2026-08-28（拖拽旋转）与 2026-08-30（右键菜单丢弃）两条记录，待补

### 下次从哪继续

**下一个目标：完善角色动画系统，并接入攻击。**

现有的动画基础：

```
PlayerState 状态机  ✅
    ↓
PlayerAnimationHash  ✅  Idle / Walk / Run / RunTurn / RunEnd
    ↓                    JumpStart / JumpUp / Apex / Fall / BaseLand / RollingLand
Animator  ✅
```

移动与跳跃这条链是通的，**攻击这条链一根都没有**。

第一个断点已经找到了，就在 `PlayerInputReceiver`：

```csharp
public void RequestAttack()
{
    Debug.Log("Player received attack request.");   // 只打日志
}                                                    // 从没置位 attackPressed

public bool ConsumeAttack()
{
    if (attackPressed) { ... }    // 于是永远返回 false
    return false;
}
```

`RequestDash()` 同样。这两条从 2026-08-17 就记在遗留问题里，现在轮到它们了。

按纵向链的做法，第一个 Checkpoint 应该小到这种程度：

> 按下攻击键 → Console 打出一行 → 玩家进入一个新的 `Player_AttackState`

先不管伤害、判定框、连段、取消窗口。
链路通了再往上加，`PlayerAnimationHash` 里也还没有任何 Attack 相关的哈希。

---

## 2026-08-27 — 堆叠、数量显示、背包开关

### 背景

拾取已经能把草药送进背包，但同种物品各占一格；背包 UI 也一直常驻在屏幕上。
这次做三件事：数据层堆叠、屏幕上显示数量、按键开关背包。

### 做了什么

**堆叠（数据层）**

- `InventoryItem.CanStackWith(ItemData)` / `Add(int) -> 剩余量`
- `InventoryGrid.FindStackable(ItemData)`
- `PlayerInventory.TryAdd` 改为「先堆叠、后开新格」，两条路的剩余量都用**递归**交给下一轮

**数量显示**

- `ItemView.SetAmount`，预制体加右下角 `TMP_Text`
- 新增 `ItemAmountUpdated` 事件，`InventoryView.UpdateItemAmount` 从字典反查显示对象

**背包开关**

- 新增 `InventoryScreen`：监听 `GameLayerStack.CurrentLayerChanged`，切换 `Canvas_Inventory`
- `InventoryView.OnEnable` 先 `Rebuild()` 全量同步再订阅；`ShowItem` 改为幂等
- `PlayerInventory` 的 grid 改为惰性创建

**测试**：新增 `PlayerInventoryTests`（9 条），`InventoryGridTests` 继续全绿。

### 关键决策与理由

| 决策 | 理由 |
| --- | --- |
| `Add` 返回剩余量而不是 void | 默默夹到 MaxStack = 物品凭空消失。方法不能吞掉自己处理不了的部分 |
| `CanStackWith` 写在 `InventoryItem` 上 | 条件用到的 `Data`、`Amount` 都是它自己的数据。规则放在拥有数据的类里 |
| 溢出用递归，不写第二套逻辑 | 两条路的形状相同：尽力吃下一部分 → 算出还剩多少 → 交给下一轮 |
| `Math.Min` 只放在「开新格」分支 | 堆叠路上 `Add` 自己会算溢出，外面先夹一刀会导致溢出算两遍、丢失一部分 |
| 拆成 `ItemPlaced` / `ItemAmountUpdated` 两个事件 | 堆叠不产生新物品。沿用前者会让屏幕上多出一个数据层不存在的物品 |
| `InventoryScreen` 独立成脚本 | 「界面该不该出现」由游戏层驱动，「界面画什么」由背包数据驱动，两个不相干的系统 |
| grid 改惰性创建，不用 `Awake` | 见下 |

### 学到的东西

**1. 开关逻辑早就写好了，缺的是「有人听见」**

`InputRouter` 一直在 `PushLayer(Inventory)` / `PopLayer`，`CurrentLayerChanged` 一直在广播，
只是没人订阅去显示 Canvas。需求听起来是「做一个开关」，实际动的只有表现层。

**2. Unity 不保证跨对象的 Awake / OnEnable 顺序**

只保证：同一对象的 `Awake` 早于它自己的 `OnEnable`；所有 `Awake`/`OnEnable` 早于任何 `Start`。
`InventoryView.OnEnable` 跑在 `PlayerInventory.Awake` 之前，`grid` 还是 null，当场 NRE。

两种解法：Script Execution Order（隐形依赖，藏在项目设置里，不推荐）；
或者让被依赖方**自己保证随时就绪** —— 惰性创建，谁先访问谁负责建。

意外收获：不依赖生命周期的类，在 EditMode 测试里可以直接 `AddComponent` 使用。
**可测试性往往是「不依赖隐式顺序」的副产品，不是额外成本。**

**3. 纯增量同步的 UI 一定会错位**

背包关闭期间 `OnDisable` 退订，那段时间的 `ItemPlaced` 无人接收；重新打开时事件早已过去。
事件是广播，不是留言。UI 的标准形状：

```
OnEnable  → 订阅 + 全量同步一次
运行期间  → 事件增量更新
OnDisable → 退订 + 清理自己的临时状态
```

存档读取、切换容器、UI 比数据后创建，都是同一个形状的问题。

**4. 用测试逼出 bug，比 Play 里试快得多**

`TryAdd(herb, 5)` 而 `MaxStack = 3` —— 第一行 `new InventoryItem(data, 5)` 直接抛异常，
连堆叠分支都进不去。这个 bug 在游戏里要等到「地上掉落一堆物品」才会出现，
但一条 `Assert.DoesNotThrow` 立刻就把它按住了。

先写好会红的测试，再改到它变绿：失败信息会直接告诉你期望多少、实际多少，
比反复 Play 精确得多。

### 遗留问题

按严重程度排列。第 1 条是唯一会造成**数据错误**的，其余都是「暂时看不出来」。

**1. `TryAdd` 返回 `bool` 不够用 —— 会导致物品被复制（下一步就修）**

`bool` 只能表达「全成功 / 全失败」，而真实结果有三种，第三种正在裸奔：

| 真实结果 | 现在返回 | 后果 |
| --- | --- | --- |
| 全部放进去了 | true | 正常 |
| 一个都没放进去（背包满） | false | 正常 |
| **只放进去一部分** | **false** | 数据层已收下一部分，`WorldItem` 见 false 不销毁自己 → 玩家白得 |

现在撞不上，只是因为 `WorldItem` 每次固定给 1 个。一旦做「地上掉落一堆子弹」立刻暴露。

**2. `Rebuild` 只补不删**

`Rebuild` 遍历的是「数据里还有的」，对「数据里没了、屏幕上还在的 `ItemView`」一无所知。
等做丢弃 / 使用道具（物品会离开背包）时必须补上删除路径，
同时要 `itemViews.Remove` **并且** `Destroy(view.gameObject)` —— 只做前者会留下删不掉的显示对象。

**3. `InventoryItem.Rotate()` 在物品已入网格后调用会造成数据不一致**

尺寸（`CurrentWidth/Height`）变了，但网格里占的格子没变。做旋转交互前必须先解决。

**4. 搬家（`TryMove`）成功后不触发任何事件**

`ItemPlaced` 只在放入时喊。将来做存档 / 联机 / 撤销，需要一个统一的「数据变更」通知时要补。

**5. 小的**

- `InventoryItem.Add` 里留了一句 `Debug.Log`，纯数据类不该打日志，顺手删
- `InteractionContext.IneractorTransform` 拼写错误（少个 `t`），一直没改
- 拾取失败（背包满）只有一行 Log，没有给玩家的反馈
- `ScreenPointToLocalPointInRectangle` 在 `InventoryView` 里仍有三处重复

### 明天从哪继续

**目标：让「地上掉落一堆物品」这个场景完全正确。**

也就是把遗留问题 1 修掉。做完之后，玩家捡一堆 5 株草药、背包只塞得下 3 株时，
地上应该正确地剩下 2 株，而不是原封不动或者凭空消失。

按这个顺序做，每步都能单独验证：

**第 1 步：改 `PlayerInventory.TryAdd` 的签名**

```csharp
public int TryAdd(ItemData data, int amount = 1)   // 返回【没能放进去的剩余量】
```

- 全部放下 → 返回 0
- 一个都放不下 → 返回 `amount`
- 放下一部分 → 返回差额

内部本来就在精确计算剩余量（那两个递归分支），只是最后丢掉了。递归改成把子调用的
返回值直接传出去即可。和 `InventoryItem.Add` 的约定保持一致：**返回「你没吃下的」。**

**第 2 步：改测试**

`PlayerInventoryTests` 里所有 `Assert.IsTrue(inventory.TryAdd(...))` 要改成
`Assert.AreEqual(0, ...)`，`IsFalse` 改成断言剩余量。
**先改测试再改实现**，让它们红着，改到变绿。

顺便补一条新的、现在还不存在的用例：

```
2×2 背包放满 3 格、MaxStack 3、最后一格空
TryAdd(herb, 5) → 应返回 2（放进去 3，剩 2）
```

**第 3 步：`WorldItem` 支持「我是一堆几个」**

加一个 `[SerializeField] private int amount = 1;`，`Interact` 改成：

```
int remaining = inventory.TryAdd(itemData, amount);
remaining == 0        → Destroy(gameObject)
remaining == amount   → 提示「背包满了」，什么都不做
其余                   → amount = remaining，不销毁（地上那堆变少了）
```

第三种情况是这次改动的全部意义所在。

**第 4 步：场景里验证**

摆一株 `amount = 5` 的草药，`GreenHerb.MaxStack = 3`，背包故意留 1 格空位。
按 F 之后：背包多出 3 株，地上那株还在，它的 `amount` 应该变成 2，再按一次 F 捡不动
（背包满了）。

**如果还有时间**：清掉遗留问题 5 里那几个小的（`Debug.Log`、拼写），都是几分钟的事。

---

一句话记住今天的主线：**一个方法不能吞掉自己处理不了的部分。**
`InventoryItem.Add` 已经做对了（返回剩余量），`TryAdd` 还没有 —— 明天补上这一课的下半段。

---

## 2026-08-27 — 打通第一条纵向链：世界里的草药 → 按 F → 进背包

### 背景

背包一直是个悬空系统：靠 `PlayerInventory.Start()` 里三个手写坐标的假物品撑着，
玩家在游戏里根本碰不到它。这次把它和世界接上。

交互系统本来就有（NPC 对话在用），草药只需要挂进去。

### 做了什么

- 新增 `WorldItem`（`MonoBehaviour, IInteractable`）：持有 `ItemData`，`Interact` 时从
  `context.Interactor` 取 `PlayerInventory`，`TryAdd` 成功才 `Destroy(gameObject)`。
- 新增 `InteractionPrompt`：物品头顶的 3D TextMeshPro 提示。
- `InteractionDetector` 改为在 `Update` 里持续维护 `current`，目标变化时切换提示。
- `InventoryGrid.TryFindFreeCell`：逐行扫描找第一个放得下的位置。
- `PlayerInventory.TryAdd(ItemData, amount)`：造物品 → 找空位 → 放置 → 触发 `ItemPlaced`。
- 删掉 `Start()` 里的调试放置与 `PrintGrid`。
- 新增 `Item` 层与 Sorting Layer。

**UI 一行没改** —— `InventoryView` 早就订阅了 `ItemPlaced`，数据层喊一声，草药自动出现在屏幕上。
事件的成本花在写的时候，收益就在这种时候兑现。

### 关键决策与理由

| 决策 | 理由 |
| --- | --- |
| 新建 `WorldItem` 而不是把逻辑写进 `ItemData` | `ItemData` 是 SO，是「草药这种东西」的定义，全项目一份；地上这株是实例，有位置、会被销毁。混在一起，多个实例会互相打架 |
| 背包引用从 `context.Interactor` 取，不用 `[SerializeField]` | 写死要给每个实例手工拖一次，还锁死「只有这一个玩家能捡」。交互发起者本来就该由交互系统告知 |
| `TryFindFreeCell` 行优先（`y` 外层） | 连续拾取时物品一行行往下铺，符合玩家预期；列优先会一列列往右长 |
| 提示挂在每个物品身上，不做全场共用一个 | 子物体天然跟着物体走，省掉每帧跟随代码；不同高度的对象能各自调偏移。代价是「同时只有一个目标」要由 Detector 维护 |
| 提示用 3D TMP，不用 World Space Canvas | Canvas 的价值是整套 UI 布局系统，为一个字付这份开销不值；3D TMP 字号直接是世界单位，不需要 `Scale = 0.01` 这种阻抗匹配 |
| `GetInteractionPrompt` 终于有了消费者 | 这个接口成员写下来之后一直零调用。提示 UI 只问接口，以后加宝箱、门，UI 一行都不用改 |

### 踩的坑

**Unity 的「假 null」对接口引用失效**

草药被 `Destroy` 后，`MissingReferenceException`。

Unity 给 `UnityEngine.Object` 重载了 `==`，让已销毁的对象与 null 比较返回 true。
**但运算符重载按编译期类型分派** —— `IInteractable` 是自定义接口，编译器不会去用那个重载，
于是 `current == null` 返回 false，代码大摇大摆往下走，访问成员时炸掉。

```csharp
private static bool IsAlive(IInteractable interaction) => interaction as Object != null;
```

`as` 回 `UnityEngine.Object`，重载才重新生效。

连带的第二个坑：**`?.` 和 `??` 是 C# 语法，同样绕过这个重载**。
`currentPrompt?.Hide()` 对已销毁对象照样会调用，然后抛异常。对 Unity 对象必须写
`if (x != null)`。

**不能依赖 `OnTriggerExit2D` 清理被销毁的对象**

它跟着物理更新走，`Update` 每帧都跑，中间至少隔一帧。那一帧里列表还留着死引用。
`Update` 开头自己 `RemoveAll` 清一遍，不指望回调顺序。

**切换提示时不能回头问旧目标**

旧目标可能已经销毁。改成在选中时就缓存 `InteractionPrompt` 组件引用，
要关的时候直接用缓存，根本不碰旧目标。

### 学到的东西

**1. 「进入范围」≠「我是被选中的那个」**

物品自己在 `OnTriggerEnter2D` 里显示提示，两株挨着的草药会同时亮，
但按键只捡最近的一个 —— 屏幕在说谎。
「谁会被交互」只有 `InteractionDetector` 能回答，显示的人必须问它。

**2. 东西该归谁，看它描述的是谁的状态**

`WorldDialogueView` 每个 NPC 各带一个是对的（显示的是那个 NPC 说的话）；
交互提示显示的是「你现在瞄准了谁」，属于玩家的检测器。
最后仍然选择挂在物品身上，是因为「不需要跟随代码 + 能按对象调偏移」的收益更实在——
但那笔债（唯一性）必须由 Detector 来还。

**3. `GetComponent` 家族**

只在目标切换时调用，不在每帧。每帧调 `GetComponent` 是 Unity 最常见的性能问题之一。
另外 `GetComponentInChildren` 默认跳过被禁用的物体，且不报错——本次靠「组件挂在
始终 active 的本体上、只禁用子物体」绕开了这个坑。

### 遗留问题

- `PlayerInventory.debugItem` 字段已无调用者，是死字段，待清理
- 同种物品目前各占一格，**堆叠还没做**（下一步）
- 拾取失败（背包满）只有一行 Log，没有给玩家的反馈
- `InteractionContext.IneractorTransform` 拼写错误，一直没改
- 承接之前的遗留项（搬家无事件、旋转数据不一致、`itemViews` 只增不减）均未变

### 下次从哪继续

```
世界里的草药  ✅
    ↓ 按 F
WorldItem.Interact  ✅
    ↓
PlayerInventory.TryAdd → TryFindFreeCell → Place  ✅
    ↓ ItemPlaced
InventoryView 自动显示  ✅
    ↓
拖拽换位置  ✅
    ↓
堆叠：同种物品合并、显示数量  ← 下一步
```

堆叠会立刻暴露一个问题：**合并不产生新物品，因此不会触发 `ItemPlaced`，屏幕上的数量不会变。**
现有的事件粒度不够用了。

---

## 2026-08-26 (2) — 拖动中的绿/红落点预览

### 背景

上一步松手才知道放不放得下。这次让玩家**拖动过程中**就看见结果：
一个半透明色块吸附在目标格上，能放显示绿色，被挡住或越界显示红色。

纯表现层，数据层一行没改 —— 这正是当初坚持「`CanPlace` 是查询、绝不修改网格」的回报，
它现在每帧被调用也完全安全。

### 做了什么

- `ItemLayer` 下新增常驻 `PlacementPreview`（半透明 Image，默认隐藏，关闭 Raycast Target）。
- `InventoryView` 新增 `ShowPlacementPreview` / `UpdatePlacementPreview` / `HidePlacementPreview`
  三段式方法，外加 `GetPreviewPosition`。
- `Drag()` 每帧经世界坐标把物品左上角换算到 `itemLayer` 空间，查 `CanPlace` 并刷新预览。
- 预览框位置用 `Vector2.Lerp` 缓动追向目标格。

### 关键决策与理由

| 决策 | 理由 |
| --- | --- |
| 预览框挂在 `ItemLayer` 下，不挂 `DragLayer` | 它要吸附格子，就必须和格子同坐标系；且背包滚动时会跟着内容走 |
| 常驻 + `SetActive` 开关，不 `Instantiate` / `Destroy` | 拖拽是高频操作，反复创建销毁持续产生 GC 垃圾 |
| `SetAsLastSibling`（画在物品**上面**） | 一开始用的 `SetAsFirstSibling`，结果红色预览恰好被"挡路的那件物品"盖住——而那正是它唯一需要被看见的时刻 |
| 位置用每帧 `Lerp`，不用协程 | 目标格一直在变，Lerp 天然可打断、可改道；协程适合一次性、有始有终、目标不变的动画 |
| 用 `Time.unscaledDeltaTime` | 背包若在 `timeScale = 0` 时打开，`deltaTime` 恒为 0，动画会整个停住 |
| 颜色做成 `[SerializeField] Color` | 手感参数要反复试，Inspector 取色器可直接粘 Hex，改完不用等编译 |
| 砍掉"出现时展开"的协程动画 | Lerp 版效果已经够好。能用更简单手段满足需求时，不该为了用上某个技术而用它 |

### 踩的坑

**1. 渲染顺序 = Hierarchy 顺序**

同一个 Canvas 下没有 Z 轴参与，排在后面的后画、盖在前者之上。
`SetAsFirstSibling` 让预览框最先画，于是被每一个 ItemView 盖住。

**2. 一个值只能有一个主人**

第一版 `SetPlacementPreview` 每帧无条件写 `sizeDelta` / `anchoredPosition`，
任何跨帧动画都会被它当帧覆盖掉。想做动画，得先把「每帧要做的」和「状态变化时做一次的」分开。

**3. 「刚开始拖动时框会飘一下」**

`BeginDrag` 传给预览的是 `TryGetCellAt` 得到的**鼠标按下格**，
而预览要的是**物品左上角格**。抓多格物品的右下角时两者差一整格，
框先摆错位置、再滑向正确格子。改用 `dragItemOriginalPosition` 反算即可。

这个 bug 的定位方式值得记：**症状出现在"刚开始拖动"这个时机 → 直接去看 `BeginDrag`。**
当时的第一直觉是"加个节流字段拦住更新"，但那只会让框静止地歪在错误位置，
把错因埋掉 —— 症状消失不等于问题解决。

### 学到的东西

**两个 RectTransform 之间怎么换算坐标**

经由世界坐标中转，这是所有 Transform 通用的：

```
A 局部 --A.TransformPoint()--> 世界 --B.InverseTransformPoint()--> B 局部
```

`rect.position` 拿到的已经是世界坐标（`anchoredPosition` 才是局部的），所以只需要后半段。
`ScreenPointToLocalPointInRectangle` 干的是同一件事，只是输入为屏幕坐标、需要额外知道摄像机。

**Lerp 的缓动感是免费的**

`current = Lerp(current, target, t)` 每帧挪掉剩余距离的一小截，
距离越近挪得越少，自然形成"快速接近、缓缓贴合"，不需要任何动画曲线。

### 遗留问题

- 承接上一条的遗留项（调试代码、搬家无事件、旋转数据不一致、`itemViews` 只增不减等）均未变
- 预览框的 `offsetPos` / `offsetDelta` 是纯观感微调，若以后改 `cellSize` 需要重调
- 协程 / `yield return` 仍未接触，进知识停车场

### 下次从哪继续

背包的拖放交互已经完整：显示 → 悬停 → 拖动 → 实时预览 → 落格改数据。

两个方向二选一：

1. **阶段 4：旋转** —— 需要先解决 `InventoryItem.Rotate()` 在物品已入网格时造成数据不一致的问题
2. **接入拾取** —— 世界中的草药 → 按 E → 进背包，打通第一条真正的纵向链，
   顺带删掉 `PlayerInventory.Start()` 里的调试代码

---

## 2026-08-26 — 松手真的搬家：Remove / CanPlace / TryMove

### 背景

上一阶段拖拽已经能拖了，但完全是"视觉表演"：`EndDrag` 无论松在哪都把 `ItemView`
弹回原位，`InventoryGrid` 从头到尾不知道有人拖过东西。

这次目标：**玩家把物品拖到空位松手，物品真的搬过去（数据 + 画面一起变）；
拖到别人身上或界外，弹回原位。**

### 做了什么

**数据层 `InventoryGrid`**

- `Remove(InventoryItem item)`：扫全表按引用清格。
- `IsAreaEmpty(...)` 加 `InventoryItem ignoreItem = null` 可选参数。
- 新增 `CanPlace(item, x, y, ignoreItem = false)`，`Place` 改为先调它，判断逻辑只留一份。

**数据层 `PlayerInventory`**

- 新增 `TryMove(item, x, y)`：`CanPlace(ignoreItem: true)` → `Remove` → `Place`，返回 bool。
- 转发 `CanPlace` 给 UI 层查询用。

**表现层 `InventoryView`**

- `EndDrag` 改为：`SetParent(itemLayer, true)` → 算落点 → `TryMove` → **按返回值**决定吸附还是弹回。
- 抽出 `GetDropItemAt`（左上角 → 格子，`RoundToInt`）和 `GetAnchorPositionForCell`（格子 → 像素）。

**测试**：`InventoryGridTests` 新增 13 条（Remove 5 条 + CanPlace 8 条），全绿。

### 关键决策与理由

| 决策 | 理由 |
| --- | --- |
| `Remove(item)` 而不是 `Remove(x, y)` | 手上的 `(x,y)` 是玩家点的那格，不一定是左上角；而网格根本没存过任何物品的左上角。按错原点擦，会既留自己的残格、又抹掉邻居 |
| 先"问得下吗"再动手，而不是"先擦，失败再回滚" | 阶段 3 的绿/红预览每帧都要在**物品没动**的情况下查询一次，这个能力反正要建。回滚方案还得额外记原点 |
| `CanPlace` 加 `ignoreItem` | 查询时物品自己还在网格里。2×2 物品右移一格，新旧区域重叠，不忽略自己就永远搬不动 |
| `ignoreItem` 默认 false | 拾取新物品走的是同一个方法，那条路**不能**被放宽。两条测试正反各钉一遍 |
| 落点用物品左上角，不用鼠标 | 抓右下角拖动时，用鼠标算会整体偏移一个抓取偏移量；且贴着右边界时画面放得下、`Place` 却越界失败 |
| 落点用 `RoundToInt`，悬停仍用 `FloorToInt` | 鼠标是个点，落在哪格就是哪格；物品左上角差几像素没对齐时，应该"吸附到最近的格子" |
| 校验写进 `TryMove`，不留在 `EndDrag` | 见下 |

### 学到的东西

**1. 安全检查要放在"被绕不过去"的地方**

第一版能跑，但保证"物品不会消失"的那次 `CanPlace` 写在 `InventoryView` 里。
数据层的底线由一个 UI 类守着——拾取系统、容器转移只要不知道这个约定，
直接调 `TryMove` 就能让物品从网格里蒸发。而 `Place` 那个被丢掉的返回值，
本来是唯一会喊"我失败了"的人。

判断标准不是"现在会不会出错"，而是"下一个调用者不知道这个约定时会不会出错"。

**2. 画面要跟着数据的实际结果走，不跟 UI 的预判走**

改之前 `EndDrag` 问了两遍：自己判断一次决定画面，`TryMove` 内部又判断一次决定数据。
现在两者必然一致，但数据层将来加任何一条规则（重量、类别限制、堆叠合并），
两套判断就会分叉，表现为"画面搬过去了、数据还在原位"。

**3. 幽灵占格与物品蒸发**

同一件物品的引用同时留在两片区域 → 抓得到但看不见的幽灵；
`Remove` 之后没放下 → 数据层没有、屏幕上还在的鬼影。
两者都不会报错，都要等玩家操作很久之后才暴露。

**4. `SetParent(parent, true)` 顺手解决了坐标系问题**

`worldPositionStays: true` 会保持画面位置不变并重算 `anchoredPosition`。
所以"先换回 `itemLayer`，再读 `anchoredPosition`"就自动完成了 dragLayer → itemLayer
的坐标换算，不需要手动转。

### 遗留问题

- `PlayerInventory.Start()` 里的 `PlaceDebugItem` / `PrintGrid` 仍是临时调试代码，接通拾取后删
- 搬家成功后不触发任何事件（`ItemPlaced` 只在放入时喊）。将来存档 / 联机需要统一的变更通知时要补
- 拖动中没有任何合法性反馈，玩家松手才知道放不下 —— 下一步就是这个
- `InventoryItem.Rotate()` 在物品已入网格后调用仍会造成数据不一致，做旋转前必须先解决
- `itemViews` 字典目前只增不减。搬家不需要动它（key 是物品不是坐标），但将来做丢弃 / 移出背包时，必须同时 `Destroy(view.gameObject)`
- `ScreenPointToLocalPointInRectangle` 仍在三处重复

### 下次从哪继续

当前功能链：

```
Grid 数据层（Place / Remove / CanPlace）  ✅
    ↓
ShowItem 画到屏幕                        ✅
    ↓
悬停 → 知道在哪个格子 / 哪个物品          ✅
    ↓
拖动 → ItemView 跟着鼠标                 ✅
    ↓
松手 → TryMove 真的改数据                ✅
    ↓
拖动中实时显示绿色 / 红色预览             ← 下一步
```

下一步是纯表现层：拖动过程中每帧用 `CanPlace(dragItem, x, y, true)` 查询落点，
合法显示绿色、非法显示红色。数据层不需要任何改动——这正是上面第 2 条
"`CanPlace` 是查询不是命令"要求它绝不修改网格的原因。

再往后：旋转（阶段 4）、拾取接入（世界中的草药 → 按 E → 进背包）。

---

## 2026-08-18 — 背包数据接上屏幕：显示、悬停、拖拽

### 背景

上一次结束时，背包在 Console 里是通的（`PrintGrid()` 能打出 ASCII 网格），
在屏幕上却是空的——数据层和场景里那 32 个手摆的 Slot 各活各的，中间毫无连接。
本次的主线就是把这条链打通，并一路做到「物品能被鼠标拖起来」。

### 做了什么

**1. 数据 → 屏幕（`InventoryView`）**

新建翻译层，双向换算格子坐标与像素：

```
格子 (x, y) → anchoredPosition = (x * step, -y * step)     step = 240 + 5 = 245
n 格的边长  → n * cellSize + (n - 1) * spacing
屏幕坐标    → itemLayer 局部坐标 → 除以 step 取整 → 格子 (x, y)
```

先在编辑器里手摆一个 Image 验证公式，再写代码。

**2. 依赖方向被编译器强制纠正**

最初让 `PlayerInventory` 持有 `InventoryView` 字段，编译报
`CS0246: 找不到 InventoryView`。原因：`PlayerInventory` 在 `FirstGame.Inventory`
asmdef 内，而 `InventoryView` 在 `Assembly-CSharp`，asmdef **看不见**它。

没有改 asmdef 配置绕过去，而是把方向反过来：`PlayerInventory` 暴露
`event Action<InventoryItem,int,int> ItemPlaced`，UI 层订阅。同一堵墙后来又挡了
第二次——想给 `InventoryItem` 加 `ItemView` 字段时。

事件选局部 `event Action` 而非 `GameEventBus`：「背包 UI 该重画了」只有那个 View
关心，不是跨系统事件。（`GameEventBus` 也在 `Assembly-CSharp`，同样够不着。）

**3. `ItemView` 两层结构**

物品图标是透明 PNG，铺不满格子，中缝的格线会透出来。解法不是改图，而是分层：
底板表达「占了哪」，图标表达「是什么」。两个信息用两个东西承载，才能各自变化。

**4. 悬停检测（`InventoryPointerHandler` + EventSystem）**

先用 `Update()` 轮询，改为 `IPointerMoveHandler` / `IPointerExitHandler`。
接口必须挂在**被射线击中的物体**上，所以新建脚本挂 `ItemLayer`
（`InventoryView` 在没有 Graphic 的 `Inventory` 上，收不到射线）。

`ItemLayer` 改为 stretch 铺满 + `Alpha = 0` 的 `Image` 作隐形热区。
放弃「32 个 Slot 各挂一个 handler」的方案：物品底板会挡住 Slot 的射线，
且每个 Slot 都得知道自己是第几格。

**5. 高亮**

`Dictionary<InventoryItem, ItemView>` 建立数据 → 显示的反查。
判断依据用 `hoveredItem` 而不是 `hoveredCell`：多格物品在自己的两格之间移动时，
按格子判断会白白重设一次高亮，按物品判断则完全不产生状态变化。

表现从改色改成微缩放，缩放放在一个专用子物体上——根物体的 pivot 必须留在 (0,1)
服务定位公式，缩放需要的却是居中 pivot。

**6. 拖拽（阶段 4.1，纯视觉）**

`IBeginDragHandler` / `IDragHandler` / `IEndDragHandler`，松手一律退回原位，
**完全不碰数据层**，这样出问题一定在表现层。

- 抓取偏移：按下时记 `grabOffset = 鼠标 - 物品`，移动时 `物品 = 鼠标 - grabOffset`
- 用 `eventData.pressPosition` 而非 `position`：`OnBeginDrag` 要等越过拖拽阈值才触发
- 拖出面板被 `Viewport` 的 `Mask` 裁掉 → 新建 `DragLayer`，拖拽期间 `SetParent` 过去

### 踩的坑

| 现象 | 原因 |
| --- | --- |
| 退出 Play 时报「Inventory is not assigned」 | 销毁顺序不定，`OnDisable` 时对方已销毁，`== null` 为 true。配置错误只该在 `OnEnable` 报一次 |
| 坐标偏半个网格，且只有右下角区域才有输出 | 用 Anchor Presets 时多按了 Shift，`ItemLayer` 的 pivot 被改成 (0.5, 0.5)。`ScreenPointToLocalPointInRectangle` 返回的是**相对 pivot** 的坐标 |
| 拖拽时物品飞走 | 把 `itemLayer` 局部坐标赋给了 `transform.position`（世界坐标）。UI 里一律用 `anchoredPosition` |
| 第一次悬停物品就抛 `ArgumentNullException` | `Dictionary.TryGetValue(null)` 会抛异常，"Try" 只保护「查不到」，不保护「key 非法」 |
| 取消高亮后底板变白 | 硬编码 `Color.white` 当「正常色」，而真正的正常色在 prefab 里。两个真相来源必然对不上 |
| 高亮完全看不出来 | `Color.white * 2` 渲染时被钳制回 1，和白色没区别 |

### 学到了什么

- **asmdef 把「谁能认识谁」变成编译期错误。** 两次挡住方向错误的设计，比靠自觉可靠。
- **`class` 传引用，`struct` 传副本。** `(RectTransform)transform` 不是转换也不是复制，是换个视角看同一个对象。
- **`anchoredPosition` 认锚点，`localPosition` 不认。** UI 里只用前者。
- **状态清理不能只走成功路径。** 开始时清一次、结束时清一次，任何分支都留下干净状态。
- **测试用例选错，跑通也不算通过。** `(0,0)` 测不出 x/y 交换，必须用 `(2,5)` 这种 x≠y 的坐标。
- **UI 点不到时，先看 EventSystem 的 Inspector 里 `Pointer Enter` 是谁**，而不是回去读代码。
- **`Place()` 成功才通知 UI。** 否则屏幕上会出现数据层查无此物的东西。越界测试专门验这条。

### 下次从哪继续

阶段 4 剩下两步：

```
4.2  拖到哪一格能放 / 不能放，实时预览绿或红
4.3  松手真的移动物品；非法则退回
```

4.2 开始前要先定两件事：

1. **拖拽过程中，物品在 `InventoryGrid` 里还占着原格子吗？**
   一把 2×1 的刀往右挪一格，若仍占原位，`IsAreaEmpty` 会撞见自己而判失败。
   立刻移除则数据与画面暂时不一致，取消时要放回去。
2. **松手时以哪个格子为准？** 鼠标所在格，还是物品左上角所在格？
   抓住物品右下角时两者能差好几格。

其他待办：

- 抽 `TryGetLocalPoint` 消除三处 `ScreenPointToLocalPointInRectangle` 重复
- 物品移除时要从 `itemViews` 删除并 `Destroy`，生成与销毁必须成对
- `allowToHeighlight` 拼写应为 `allowToHighlight`
- `InputRouter.OnDisable()` 退出 Play 时抛 `NullReferenceException`（与背包无关，单独处理）
- `KuroganeKatana` 的 `displayName` 为空会让 `PrintGrid()` 抛 `IndexOutOfRangeException`
- 美术：多格物品需要匹配宽高比的图（2×1 物品配 2:1 的图），当前图集全是 16×16

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
