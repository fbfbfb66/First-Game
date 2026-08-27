# FirstGame 开发日志

按开发阶段倒序记录。每条包含：做了什么、为什么这么做、学到了什么、下次从哪继续。
项目当前状态请看 [FirstGameDetails.md](FirstGameDetails.md)，本文件只记录过程。

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
