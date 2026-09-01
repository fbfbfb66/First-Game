# First Game Project Details

本文档用于记录当前 Unity 项目的结构、核心系统、资源变化、脚本职责、函数职责、脚本之间的关系和 Git 提交规则。项目结构、脚本职责、函数、输入绑定、场景、资源或 ScriptableObject 发生变化后，应同步更新本文档。

最后更新时间：2026-08-27。

## 1. 项目概览

- 项目类型：Unity 2D 游戏项目。
- 主要游戏内容目录：`Assets/_Game/`。
- 当前分支：`main`。
- 远程仓库：`https://github.com/fbfbfb66/First-Game.git`。
- 当前核心系统：
  - 场景加载：`SceneLoader` + `SceneNames` + `GameBootstrap`。
  - 游戏层管理：`GameLayerStack` + `GameLayerRuleDatabase` + `GameLayerRule` + `GameLayerType`。
  - 输入读取与路由：`GameInputReader` + `InputRouter`。
  - 玩家输入、移动与状态机：`PlayerInputReceiver` + `PlayerMovement` + `Player` + `StateMachine` + 玩家状态类。
  - 世界交互：`IInteractable` + `InteractionContext` + `InteractionDetector` + `InteractionPrompt`。
  - 世界物品拾取：`WorldItem` → `PlayerInventory.TryAdd()` → `ItemPlaced` → `InventoryView`。
  - 对话系统：`DialogueManager` + `DialogueData` + `NPCDialogueProfile` + `ConditionalDialogueEntry` + `WorldDialogueView` + `WorldDialogueChoiceView`。
  - 事件总线：`GameEventBus` + `IGameEvent` + `GameSignalEvent` + `GameFlagChangedEvent` + `QuestStateChangedEvent`。
  - Flag 与条件：`GameFlagCenter` + `GameFlagDatabase` + `GameFlagData` + `GameCondition` + `FlagBoolCondition`。
  - Quest 系统：`QuestManager` + `QuestDatabase` + `QuestData` + `QuestState` + `QuestStateCondition`。
  - 剧情序列：`StoryTrigger` + `StorySequenceRunner` + `StorySequence` + `StoryStepAction` + `StorySceneBindings` + `StoryCameraDirector` + `StoryContext`。
  - 背包系统：数据层 `ItemData` + `ItemCategory` + `InventoryItem` + `InventoryGrid` + `PlayerInventory`；表现层 `InventoryView` + `InventoryPointerHandler` + `ItemView` + `ItemDetailPanel`。已打通「显示 → 悬停高亮 → 拖拽 → 旋转 → 落格改数据」与「左键选中 → 选中框 + 常驻详情面板 → 按数量丢弃 → 移除数据并销毁显示对象」。
  - 其他 UI：`UI_HPBarView`。

## 2. Unity Git 提交规则

需要提交：

- `Assets/`：手写脚本、场景、预制体、动画、图片、音频、ScriptableObject 和对应 `.meta`。
- `Packages/manifest.json` 与 `Packages/packages-lock.json`。
- `ProjectSettings/`：项目设置、Build Settings、Tags/Layers、URP、Physics、Input 等配置。
- `.gitignore`、`.gitattributes`、项目文档。

不要提交：

- `Library/`、`Temp/`、`Obj/`、`Logs/`、`UserSettings/`。
- IDE 自动生成文件：`*.csproj`、`*.sln`、`*.slnx`、`*.csproj.lscache`、`.vs/`、`.idea/`。
- Build 输出、录屏、崩溃日志和本机缓存。

当前 `.gitattributes` 使用 Git LFS 管理常见二进制资源，例如图片、音频、Aseprite、FBX、视频和 Blender 文件。

## 3. 根目录结构

```text
First Game/
|-- .git/               Git 仓库数据
|-- .vscode/            本机 IDE 配置，通常不提交
|-- Assets/             Unity 资源与游戏代码，提交
|-- Library/            Unity 自动缓存，不提交
|-- Logs/               Unity 日志，不提交
|-- Packages/           Unity 包依赖清单，提交
|-- ProjectSettings/    Unity 项目设置，提交
|-- Temp/               Unity 临时文件，不提交
|-- UserSettings/       本机用户设置，不提交
|-- .gitattributes      Git LFS 规则
|-- .gitignore          Git 忽略规则
`-- FirstGameDetails.md 当前项目追踪文档
```

## 4. `Assets/_Game` 结构

```text
Assets/_Game/
|-- Animation/
|-- Art/
|-- Audio/
|-- Data/
|   |-- Core/
|   |-- Dialogue/
|   |-- Flags/
|   |-- Items/
|   |-- Player/
|   |-- Story/
|   `-- Quests/
|-- Fonts/
|-- Materials/
|-- Prefabs/
|-- Scenes/
|-- Scripts/
|   |-- Editor/
|   |-- Runtime/
|   |   |-- Core/
|   |   |-- GameFlow/
|   |   |-- GamePlay/
|   |   |-- Input/
|   |   |-- Systems/
|   |   `-- UI/
|   `-- Tool/
|-- Settings/
|-- Tests/
|   `-- EditMode/
|-- Shaders/
`-- TileMaps/
```

Unity 的 `.meta` 文件保存资源 GUID 和导入设置，必须和对应资源一起提交。

## 5. 场景列表

`ProjectSettings/EditorBuildSettings.asset` 当前启用 3 个场景：

| 顺序 | 场景路径 | 用途 |
| --- | --- | --- |
| 0 | `Assets/_Game/Scenes/BootScene.unity` | 启动场景，承载启动流程对象 |
| 1 | `Assets/_Game/Scenes/MainMenuScene.unity` | 主菜单场景 |
| 2 | `Assets/_Game/Scenes/GameScene.unity` | 游戏主场景 |

### `GameScene` 背包 UI 结构

```text
Canvas_Inventory (Screen Space - Overlay)
|-- BackGround
|-- InventoryPart
|   |-- InventoryBG
|   |-- Inventory                        <- InventoryView
|   |   `-- InventoryArea (ScrollRect)
|   |       `-- Viewport (Mask)          <- 裁剪来源
|   |           `-- Content              <- GridLayoutGroup (Cell 240, Spacing 5) + ContentSizeFitter
|   |               |-- Slot .. Slot (31) <- 32 个静态格子底图
|   |               `-- ItemLayer        <- InventoryPointerHandler
|   |                   |-- PlacementPreview <- 拖放预览框（默认隐藏）
|   |                   |-- SelectedBoard     <- 选中框（默认隐藏）
|   |                   `-- ItemView(Clone) ...
|   `-- ItemDetailPanel                  <- ItemDetailPanel（常驻，不隐藏自身）
|       |-- Basic                        <- 有选中项时显示
|       |   |-- Icon
|       |   |-- NameLabel
|       |   |-- CategoryLabel
|       |   |-- DescriptionLabel
|       |   `-- ActionRow
|       |       |-- DropButton
|       |       `-- AmountRow (＋ / − / AmountLabel)
|       `-- FallBackLabel                <- 无选中项时显示
`-- DragLayer                            <- 拖拽期间物品的临时父节点
```

要点：

- `ItemLayer` 是 `Content` 的**最后一个子物体**，因此在渲染与射线顺序上位于所有 Slot 之上。
- `ItemLayer` 挂 `LayoutElement`（勾选 `Ignore Layout`），否则会被 `GridLayoutGroup` 当成第 33 个格子摆走。
- `ItemLayer` 锚点为 stretch/stretch、pivot 为 (0, 1)，并挂一个 `Alpha = 0` 且开启 `Raycast Target` 的 `Image` 作为隐形鼠标热区。**pivot 一旦被改动，`TryGetCellAt` 的换算结果会整体偏移。**
- 物品显示对象不是 Slot 的子物体——多格物品无法塞进单个 Slot，只能由 `ItemLayer` 统一按坐标摆放。
- `Canvas_Inventory` 默认**未启用**，由 `InventoryScreen` 根据 `GameLayerType.Inventory` 开关。
- `PlacementPreview` 是一张半透明 `Image`，anchor/pivot 同为 (0, 1)，**关闭 `Raycast Target`**（否则会挡住 `ItemLayer` 的鼠标射线，拖动直接失灵），默认 `SetActive(false)`。它必须是 `ItemLayer` 的子物体：预览框要吸附到格子，就得和格子共用同一套坐标系，且背包滚动时会跟着内容一起走。拖拽开始时 `SetAsLastSibling()` 把它排到所有 `ItemView` **之后**——同一 Canvas 下渲染顺序即 Hierarchy 顺序，排在后面才画在上面。若排在前面（`SetAsFirstSibling`），红色预览恰好会被"挡路的那件物品"的底板完全盖住，而那正是它唯一需要被看见的时刻。
- `SelectedBoard` 是选中框，与 `PlacementPreview` 同为 `ItemLayer` 的子物体、同样 anchor/pivot 为 (0, 1) 且**关闭 `Raycast Target`**，默认 `SetActive(false)`。放在 `ItemLayer` 下的理由与预览框一致：要吸附到格子就得共用同一套坐标系，且背包滚动时跟着内容一起走。`ShowSelectedBoard` 里 `SetAsLastSibling()` 让它画在物品之上——框是空心描边，压在图标上不会遮挡内容，反倒能盖过 `ItemView.SetHighlighted` 里同样调 `SetAsLastSibling()` 抢到最前的悬停项。
- `ItemDetailPanel` 是 `InventoryPart` 的子物体，与 `Inventory` 左右并列，**它自身永不隐藏**——「常驻」正是这次改版的核心，空态由内部的 `Basic` / `FallBackLabel` 两组子物体互斥切换来表达，而不是关掉整块面板。
- `ActionRow`（丢弃按钮与数量行）**必须放在 `Basic` 里面**：没有选中项时整个 `Basic` 关闭，按钮随之消失。放到 `Basic` 外面会出现「没选任何物品却有一个可点的丢弃按钮」。
- 旧的 `Blocker` 与 `ItemContextMenu` 已随右键菜单一并删除。全屏透明遮罩是**弹出式**菜单才需要的东西（用来接住「点在了菜单以外」那一下），常驻面板没有「关闭」这个动作，也就不需要遮罩；顺带消除了遮罩期间格子悬停与拖拽被吞掉的副作用。
- `DragLayer` 是 `Canvas_Inventory` 的**直接子物体且排在最后**，因此位于 `Viewport` 上 `Mask` 的作用范围之外，拖出背包的物品不会被裁掉；排最后保证它画在其余 UI 之上。锚点 stretch/stretch、offset 全 0、**pivot 同样为 (0, 1)**（必须与 `ItemLayer` 一致，否则拖拽定位会整体偏移）。**刻意不挂 `Image`**——挂了会变成覆盖全屏的射线目标，吞掉所有 UI 鼠标事件；也不挂 `Mask`。

## 6. 输入与游戏层

输入配置来自 `Assets/Settings/InputSystem_Actions.inputactions`，生成代码为 `Assets/Settings/InputSystem_Actions.cs`。

- `Player` Action Map：移动、跳跃、攻击、冲刺、交互、使用物品。
- `Game` Action Map：暂停、打开背包、打开地图。
- `UI` Action Map：导航、确认、取消、旋转物品（`RotateItem`，绑定 <Keyboard>/r）。`RotateItem` 放在 `UI` 而非 `Player` Map，因为背包打开时 `SetInputMode` 只启用 `UI`，放在别处这个键在背包里就是死的。

`GameInputReader.SetInputMode(GameLayerType layerType)` 会按当前游戏层启用对应 Action Map：

- `Gameplay`：启用 `Player` 和 `Game`。
- 菜单和 UI 层：启用 `UI`。
- `Dialogue`：启用 `Player`。
- `Cutscene`：关闭输入。

`InputRouter` 负责把输入派发到当前层：

- `Gameplay` 的移动输入进入 `PlayerInputReceiver.SetMoveInput()`。
- `Gameplay` 的跳跃、攻击、冲刺、交互输入分别进入 `PlayerInputReceiver` 的请求函数。
- 玩家地面状态中消费交互请求，并调用 `InteractionDetector.TryInteract()`。
- `Dialogue` 的交互输入调用 `DialogueManager.RequestAdvance()`。
- `DialogueChoice` 的导航与确认输入调用 `DialogueManager.HandleChoiceSelectedNavigate()` 和 `DialogueManager.HandleChoiceConfirmed()`。
- `Inventory` 的旋转输入调用 `InventoryView.RotateDragItem()`。Router 只判断「当前是不是背包层」，**不判断玩家有没有在拖东西**——`dragItem` 是 `InventoryView` 的私有状态，Router 不该知道；它是分发器，不是决策者。`inventoryView` 字段在 `Awake` 用 `FindAnyObjectByType<InventoryView>(FindObjectsInactive.Include)` 兜底，`Include` 不可省——背包 Canvas 默认是关闭的。

## 7. 全脚本与函数详解

本节按脚本路径记录所有当前游戏脚本。每个条目包含脚本职责、关键字段或属性、函数职责，以及与其他脚本的关联。

### Tool

#### `Assets/_Game/Scripts/Tool/TimeTool.cs`

- 脚本职责：提供按字符串 ID 管理的简单计时协程，用于状态逻辑判断某个时间窗口是否仍然存在。
- 关键字段：
  - `timeCounterCoroutines`：保存计时 ID 到协程对象的映射；外部状态通过 `ContainsKey` 判断窗口是否还在运行。
- 函数：
  - `StartTimeCounter(float time, string coroutineID)`：如果同名计时已经存在则先停止旧协程，再启动新的计时协程，并用 `coroutineID` 注册。
  - `timeCounter(float time, string coroutineID)`：等待指定秒数，然后从字典中移除计时 ID。
- 关联：`Player_RunState` 使用 `RunBuffer` 和 `RunEndEarly` 两个 ID 控制跑步松手缓冲与过早结束窗口。

#### `Assets/_Game/Scripts/Tool/GroundSensor.cs`

- 脚本职责：用 2D 射线检测角色是否接触地面。
- 关键字段：
  - `point`：射线发射点。
  - `whatIsGround`：地面 LayerMask。
  - `distance`：检测距离。
  - `IsGrounded`：对外只读的落地状态。
- 函数：
  - `UpdateGroundState()`：由玩家状态逻辑主动刷新 `IsGrounded`，从 `point` 向下发射射线，命中地面层则为 true。
  - `OnDrawGizmos()`：在 Scene 视图中绘制检测射线，方便调试检测距离。
- 关联：`PlayerState.LogicalUpdate()` 统一刷新检测结果；`PlayerGround`、`PlayerAir`、`Player_Fall` 等玩家状态通过 `groundSensor.IsGrounded` 决定落地、起跳、下落转换。

#### `Assets/_Game/Scripts/Tool/WallSensor.cs`

- 脚本职责：用水平 2D 射线检测玩家朝向一侧是否接触墙面。
- 关键字段：
  - `point`：射线发射点。
  - `whatIsWall`：墙面 LayerMask。
  - `distance`：检测距离。
  - `IsTouchingWall`、`WallDirection`：对外只读的贴墙状态与检测方向。
- 函数：
  - `UpdateWallState(bool facingRight)`：根据玩家朝向决定射线方向，命中墙层时更新 `IsTouchingWall`。
  - `OnDrawGizmos()`：在 Scene 视图中绘制红色检测射线，方便校准发射点和距离。
- 关联：`PlayerState.LogicalUpdate()` 统一刷新检测结果；`Player_Fall` 命中墙面时进入 `Player_WallSlide`。

### Runtime/UI

#### `Assets/_Game/Scripts/Runtime/UI/MainMenuController.cs`

- 脚本职责：管理主菜单按钮和主菜单层状态。
- 关键字段：
  - `newGameButton`、`continueGameButton`、`quitGameButton`：主菜单三个按钮。
  - `sceneLoader`：执行场景切换。
  - `layerStack`：进入主菜单时重置当前游戏层。
- 函数：
  - `Awake()`：查找或缓存 `SceneLoader`、`GameLayerStack`。
  - `OnEnable()`：注册按钮点击事件，并把层重置为 `MainMenu`。
  - `OnDisable()`：注销按钮点击事件，避免重复订阅。
  - `OnNewGameClicked()`：加载游戏场景。
  - `OnContinueGameClicked()`：当前逻辑同样加载游戏场景，后续可接入存档。
  - `OnQuitGameClicked()`：调用退出游戏逻辑。
- 关联：依赖 `SceneLoader` 切换到 `GameScene`，依赖 `GameLayerStack` 让输入系统进入菜单/UI 模式。

#### `Assets/_Game/Scripts/Runtime/UI/UI_HPBarView.cs`

- 脚本职责：控制 UI 血条填充区域的显示宽度。
- 关键字段：
  - `fillClip`：血条填充裁剪区域的 `RectTransform`，由场景 UI 绑定。
  - `fullWidth`：血条满值宽度，当前固定为 `600f`。
- 函数：
  - `UpdateHpBar(float value)`：接收 0 到 1 的血量比例；比例非法时输出错误日志，合法时把 `fillClip` 横向宽度设置为 `fullWidth * value`。
- 关联：`GameScene.unity` 中的 `HPBar` 对象挂载该脚本，并绑定 `FillClip` 节点作为裁剪填充区域。

#### `Assets/_Game/Scripts/Runtime/UI/InventoryView.cs`

- 脚本职责：背包数据层与屏幕像素之间的唯一翻译官。负责「格子坐标 ↔ 屏幕坐标」双向换算，并把 `InventoryItem` 生成为屏幕上的 `ItemView`。
- 关键字段：
  - `itemLayer`：所有物品显示对象的父节点，同时是坐标换算的参照系与鼠标射线接收区。**其 pivot 必须为 (0, 1)（左上角）**，`TryGetCellAt` 依赖这一前提。
  - `dragLayer`：拖拽期间物品的临时父节点，pivot 同为 (0, 1)。
  - `inventory`：数据来源 `PlayerInventory`。依赖方向为 UI → 数据，反向被 asmdef 禁止。
  - `itemViewPrefab`：物品显示预制体。
  - `cellSize` / `spacing`：需与 `Content` 上 `GridLayoutGroup` 的 Cell Size、Spacing 保持一致，当前为 240 / 5，故一格步长为 245。
  - `PlacementPreview`：拖放预览框的 `Image`。常驻场景、靠 `SetActive` 开关，**不做 `Instantiate` / `Destroy`**——拖拽是高频操作，反复创建销毁会持续产生 GC 垃圾。
  - `validPlacementColor` / `invalidPlacementColor`：合法 / 非法落点的颜色，含透明度。做成字段而非常量，因为这属于要反复试的手感参数，Inspector 取色器底部可直接粘 Hex。
  - `offsetPos` / `offsetDelta`：预览框相对格子的位置与尺寸微调，用于做出"内缩一圈"的边框观感。
  - `previewFollowSpeed`：预览框追向目标格的速度。
  - `dragRotated`：**本次拖拽中相对拿起那一刻是否又转过**，属于「拖拽会话状态」，和 `dragItem` / `grabOffset` / `dragItemOriginalPosition` 同类，松手即作废。旋转期间**刻意不改数据层**：`InventoryItem.IsRotated` 描述的是「物品在背包里躺着的朝向」，是既成事实，格子占用由它算出；而拖拽中的旋转是随时可撤销的**意图**（再按一次 R、松手时位置非法、拖着关背包），把意图直接写成事实必然需要回滚，且会立刻造成「`IsRotated` 变了但 `Grid` 里的格子仍按旧尺寸占着」。
  - `DragIsRotated` / `DragWidth` / `DragHeight`：由 `dragRotated` 推出的只读属性。`DragIsRotated = dragItem.IsRotated ^ dragRotated`（异或：转两次等于没转）是**绝对朝向**；`DragWidth` / `DragHeight` 必须配 `Data.Width` / `Data.Height` 这套**原始尺寸**，**不能用 `CurrentWidth` / `CurrentHeight`**——后者自身已含 `IsRotated`，两边各算一次会重复计算，症状是「本来就竖着躺在包里的物品，第二次拖拽时尺寸判断全错」。同一个概念只能有一个来源：要么全程 `Current*`（自带朝向），要么全程 `Data.*` + 显式朝向。
  - `itemViews`：`Dictionary<InventoryItem, ItemView>`，从数据对象反查其显示对象。刻意放在 UI 层——`InventoryItem` 不该知道自己有没有被显示，且它在 asmdef 内也引用不到 `ItemView`。
  - `hoveredCell` / `hoveredItem`：鼠标当前所在格子 / 当前悬停的物品。高亮以 `hoveredItem` 为判断依据，因此在同一多格物品的不同格之间移动不会触发重复的高亮切换。
  - `dragItem` / `dragItemOriginalPosition` / `grabOffset`：拖拽中的物品、它在 `itemLayer` 下的原始 `anchoredPosition`、按下瞬间「鼠标 → 物品」的偏移。
  - `allowToHeighlight`：拖拽期间抑制悬停高亮。
  - `selectedItem`：**当前选中的物品**，与 `hoveredItem`、`dragItem` 并列的第三种物品状态，三者语义互不重叠：`hoveredItem` 鼠标一移开就没了，`dragItem` 只在拖拽过程中存在，`selectedItem` 由玩家主动点击产生并**一直保持**，直到玩家点了别处或它本身消失。详情面板与选中框都以它为唯一依据。
  - `selectedBoard` / `detailPanel`：选中框的 `Image`、右侧常驻详情面板。
- 函数：
  - `OnEnable()`：**先 `Rebuild()` 全量同步，再订阅** `ItemPlaced` / `ItemAmountUpdated`。背包关闭期间本组件随 Canvas 一起被禁用，`OnDisable` 已退订，那段时间捡到的物品事件**没有任何人听见**——事件是广播不是留言，错过一次，UI 与数据就永久错位。所以 UI 的标准形状是「打开时全量同步一次 + 打开期间靠事件增量更新」。
  - `OnDisable()`：退订，并清理拖拽状态——把拖拽中的 `ItemView` `SetParent` 回 `itemLayer`（否则它会一直留在 `DragLayer` 下，坐标系不对且不随背包滚动）、恢复其底板、隐藏预览框（否则下次打开会看到僵在原地的绿框），最后 `SetSelectedItem(null)` 清空选中（否则下次打开背包，面板会残留上一次的内容，而那件物品可能早已不在包里）并清空 `dragItem` 与 `allowToHeighlight`，并调 `SetItemView(dragItem, itemView, dragItem.IsRotated)` **把朝向恢复成数据层的真相**——「拖着转了 90° 后直接关背包」是丢弃意图的三个出口之一（另两个是松手成功、松手非法），三处都必须收尾。取 `itemViews` 前必须先判 `dragItem != null`：**`Dictionary.TryGetValue` 对 null 键会抛 `ArgumentNullException`**，而「没在拖东西时关闭背包」恰恰是最常见的路径。
  - `SelectItem(Vector2 screenPosition)`：**左键选中入口**。拖拽中早退，否则把命中格里的物品（可能为 `null`）交给 `SetSelectedItem`。它只负责回答「玩家点中了谁」，不碰任何表现。
  - `SetSelectedItem(InventoryItem item)`：**`selectedItem` 的唯一赋值入口**，赋值后同步刷新详情面板与选中框，`item` 为 `null` 走空态。选中的来源有四个——点击格子、选中项被移除、背包关闭、拖拽结束——若让每个来源各自手抄「改字段 + 刷面板 + 刷选中框」，漏掉一处的表现就是「UI 显示了一个已经不存在的东西」，且不会报任何错。**状态与它的表现必须在同一处更新**，这条规则比省下的几行代码重要得多。
  - `ShowSelectedBoard()` / `HideSelectedBoard()`：按 `inventory.TryGetItemPosition` 求出选中物品的**原点格**摆放选中框。**不能用玩家点击的那一格**——点 2×1 物品的右半格时，点击格与原点格差一整格，框会歪出去一格。`dragItem != null` 时早退：拖拽中物品不在网格原位，此时算出的位置没有意义。
  - `DropItem(InventoryItem item, int amount)`：`ItemDetailPanel.DropRequested` 的回调，转成 `inventory.TryRemove`。**订阅方是 `InventoryView` 而不是让面板直接持有 `PlayerInventory`**：面板是纯 UI，只知道「用户点了丢弃」，不该知道背包数据长什么样；以后同一个面板要对不同容器的物品操作时，硬连 `PlayerInventory` 那天就得推倒重来。这条连线在「右键菜单 → 常驻面板」的改版中**一个字都没改**——请求方换了个前端，执行方完全不受影响，正是当初把「发出请求」与「执行操作」分开的收益。
  - `OnItemRemoved(InventoryItem item)`：`ItemRemoved` 的回调。`Destroy` 掉 `ItemView` 的 `GameObject`、**从 `itemViews` 字典移除该条映射**，并把 `hoveredItem` / `dragItem` 中指向它的引用置空；`selectedItem` 指向它时走 `SetSelectedItem(null)`。**三处都必须先判等于被移除的那件**——无脑清空会让「丢掉 A 时 B 的选中状态也跟着消失」。字典必须删——`Destroy` 只销毁场景对象，留着映射会让后续 `TryGetValue` 成功返回一个已销毁的 `ItemView`，访问其 `transform` 即撞上 Unity 的假 null（`MissingReferenceException`）。`Destroy` 是帧末延迟的，而 `itemViews.Remove` 立即生效，两者互不依赖。
  - `Rebuild()`：遍历 `inventory.GetPlacedItems()` 逐个 `ShowItem`。`ShowItem` 已做成幂等（同一 `InventoryItem` 复用已有的 `ItemView`），因此重复调用不会产生重复对象。**目前只补不删**。背包**打开期间**不成问题：`ItemRemoved` 会实时销毁对应的 `ItemView`，字典与数据层保持同步。真正的洞在于 `OnDisable` 会退订 `ItemRemoved`——若将来出现「背包关闭时也会改数据」的路径（世界里丢弃、仓库转移、任务脚本扣道具），这期间的移除通知收不到，再打开时 `Rebuild` 只补不删，已经没了的物品会留在屏幕上。当前唯一的移除入口是详情面板的丢弃按钮，而面板必须背包开着才点得到，故暂不处理；第一个这类功能出现时，`Rebuild` 要从「补齐」改成「对齐」：先扫一遍 `itemViews` 销毁掉数据层已不存在的，再补新的。
  - `UpdateItemAmount(InventoryItem)`：`ItemAmountUpdated` 的回调，从 `itemViews` 反查显示对象并刷新格子上的数字；若变化的正是 `selectedItem`，再走一次 `SetSelectedItem(item)` 让详情面板重算数量选择器的上限。**走事件而不是让面板在丢弃后自己刷新**：数量还会因堆叠合并、使用消耗、任务扣除等别的原因变化，手动刷新每多一条路径就要多补一次，而事件是所有路径的共同出口。
  - `ShowItem(InventoryItem, int x, int y)`：`ItemPlaced` 的回调，同时被 `Rebuild` 复用。**幂等**：`itemViews` 中已有该物品则复用其 `ItemView`，否则实例化并登记，按 `(x * step, -y * step)` 设置 `anchoredPosition`，按 `n * cellSize + (n - 1) * spacing` 设置 `sizeDelta`，使多格物品在视觉上跨越对应格数。
  - `TryGetCellAt(Vector2 screenPosition, out int x, out int y)`：`ShowItem` 的反函数。经 `RectTransformUtility.ScreenPointToLocalPointInRectangle` 把屏幕坐标转为 `itemLayer` 局部坐标（Canvas 为 Screen Space - Overlay，摄像机参数传 `null`），再除以步长并 `FloorToInt` 得到格子坐标，最后用 `inventory.IsInside` 校验。用 `FloorToInt` 而非 `CeilToInt`：格子 n 覆盖 `[n, n+1)` 区间，且出界时结果为负数便于识别。
  - `UpdateHover(Vector2 screenPosition)` / `ClearHover()`：由 `InventoryPointerHandler` 调用。仅在悬停物品发生变化时切换 `ItemView` 的高亮。
  - `BeginDrag(Vector2 screenPosition)`：**返回是否真的抓起了物品**，供 `InventoryPointerHandler` 决定这次拖拽该归自己还是转发给 `ScrollRect`。以 `eventData.pressPosition` 为输入。查出按下格子里的物品，记下原位置，**先** `SetParent(dragLayer, true)` **再**计算 `grabOffset`（两者必须在同一坐标系内），并抑制高亮（先 `SetHighlighted(false)` 并清空 `hoveredItem` 再置 `allowToHeighlight = false`，否则被抓起的那件正好是当前 hover 项，会带着 1.1 倍缩放拖一路），同时 `dragRotated = false` 开启新一轮意图。最后调 `ShowPlacementPreview`，其格子**必须由 `dragItemOriginalPosition` 反算**，不能用 `TryGetCellAt` 得到的按下格——玩家抓多格物品的右下角时两者相差一整格，预览框会先摆错位置、再滑向正确格子，表现为"刚开始拖动时框会飘一下"。
  - `Drag(Vector2 screenPosition)`：`anchoredPosition = 鼠标在 dragLayer 局部坐标 - grabOffset`，保持抓取时的相对位置不变。随后用 `itemLayer.InverseTransformPoint(rect.position)` 把物品左上角换算到 `itemLayer` 空间，求出落点格并调 `UpdatePlacementPreview`，合法性判断走**尺寸版** `CanPlace(x, y, DragWidth, DragHeight, dragItem)`——`Drag` 是鼠标一动就触发的高频入口，若这里仍读物品自身尺寸，旋转后只要鼠标一动，判断就会被旧尺寸覆盖回去。**拖拽期间物品必须留在 `dragLayer`**（否则会被 Mask 裁掉），所以不能用 `EndDrag` 那种 `SetParent` 换算法；此处经由世界坐标中转：`rect.position` 已是世界坐标，`InverseTransformPoint` 再转入 `itemLayer` 局部空间，`ScrollRect` 滚动、缩放都自动成立。函数体用早退（guard clause）写法，主线逻辑保持零缩进。
  - **`grabOffset` 的语义**：定义为「鼠标位置 − 物品左上角位置」，即**鼠标该待在物品身上的哪个点**。它成立的前提是三个原点对齐——`DragLayer` 的 pivot 在 (0, 1)、`ItemView` 的 anchor 在 (0, 1)、`ItemView` 的 pivot 也在 (0, 1)，于是 `ScreenPointToLocalPointInRectangle` 的返回值与 `anchoredPosition` 共用同一个原点，可以直接加减。`Drag` 每帧维持等式 `anchoredPosition = 鼠标 − grabOffset`，所以**鼠标位置隐含在这两个字段里**，`RotateDragItem` 无需另存字段即可用 `anchoredPosition + grabOffset` 反推。
    更重要的是：`grabOffset` 不是一次性的测量值，而是**一条可改写的策略**。`BeginDrag` 时的策略是「待在你点的那个点」；旋转时改写成 `(boxSize.x / 2, -boxSize.y / 2)`（从左上角向右半宽、向下半高，UI 中 y 向上为正故取负），即宣布「从现在起鼠标待在物品正中心」，下一帧 `Drag` 会自动照办。之所以选中心而不是保留原抓取点，是因为转 90° 后「原来抓的那个点」在新形状里没有唯一对应位置，而中心永远存在、永远在物品内部。若不同步改写 `grabOffset`，左上角会原地不动、外框朝右下重新展开，表现为「按 R 后物品从鼠标底下跳开」。
  - `ShowPlacementPreview(Vector2Int cell, InventoryItem item)` / `UpdatePlacementPreview(Vector2Int cell, bool valid)` / `HidePlacementPreview()`：拖放预览框的三段生命周期，刻意拆成三个方法而不是一个带 `enable` / `valid` 参数的方法——「出现」「每帧刷新」「消失」需要做的事并不相同，捏在一起会让每个调用点都要反推参数含义。`Show` 负责显示、置顶、设尺寸并**瞬间**就位（走缓动会让框从上一次拖拽的残留位置飞过来）；`Update` 每帧只刷颜色并用 `Vector2.Lerp` 朝目标格逼近，`t` 取 `previewFollowSpeed * Time.unscaledDeltaTime`——**用 unscaled 是因为背包若在 `timeScale = 0` 时打开，`Time.deltaTime` 恒为 0，动画会整个停住**；`Hide` 只负责隐藏。
  - `GetPreviewPosition(Vector2Int cell)`：格子坐标 → 预览框 `anchoredPosition`（叠加 `offsetPos`）。
  - `GetRectSizeDelta(int width, int height)`：格数 → 像素尺寸，`n * cellSize + (n - 1) * spacing`。刻意只留这一个「格数 → 像素」的换算，`ShowItem`、预览框、拖拽中的临时尺寸全部转发到它。
  - `SetItemView(InventoryItem item, ItemView view, bool rotated)`：按**显式传入的朝向**算出外框尺寸与图标尺寸，转发给 `ItemView.SetFootprint`。朝向作为参数而非从 `item` 读，是因为两条调用路各有各的真相：`ShowItem` 传 `item.IsRotated`（数据事实），`RotateDragItem` 传 `DragIsRotated`（拖拽意图）。图标尺寸**永远是 `Data.Width` × `Data.Height` 的原始尺寸**，靠旋转 90° 去填满外框，而不是把图塞进新框里拉伸。
  - `RotateDragItem()`：`InputRouter` 在背包层按下 R 时调用。翻转 `dragRotated` → `SetItemView` 换新外框 → 重算 `grabOffset` 与 `anchoredPosition` → 求落点格 → 尺寸版 `CanPlace` → `RotatePlacementView`。**顺序不可调换**：重摆位置会改变落点格，若先求格子再挪物品，红绿判断和预览框位置都会错一格。
  - `RotatePlacementView(Vector2Int cell, bool valid)`：旋转时刷新预览框的尺寸、颜色与位置。位置**直接赋值不走 Lerp**——玩家可以鼠标不动只按 R，此时 `Drag` 根本不会被调用，靠 `UpdatePlacementPreview` 的缓动去追会让预览框僵在原格。
  - `EndDrag()`：**第一步必须是** `SetParent(itemLayer, true)`。`worldPositionStays: true` 会让 Unity 在换父节点时保持画面位置不变并重算 `anchoredPosition`，因此这一行执行完，`rect.anchoredPosition` 就已从 `dragLayer` 空间变成 `itemLayer` 空间——即与 `ShowItem` 同一套坐标系，可以直接换算格子。随后 `GetDropItemAt` 求出落点，交给 `inventory.TryMove(dragItem, x, y, DragIsRotated)`，**按其返回值决定画面**：成功则吸附到目标格（画面无需再动——按 R 那一刻就已画成新朝向，此刻是数据追上了画面）；失败则退回 `dragItemOriginalPosition` 并 `SetItemView(dragItem, itemView, dragItem.IsRotated)` 把朝向复原，**此处传数据层的 `IsRotated` 而非 `DragIsRotated`：失败时以数据为准，画面服从数据**。最后清理 `dragItem`、`dragRotated` 与高亮抑制，并调 `ShowSelectedBoard()` 把选中框挪到新位置。**这一句必须排在 `dragItem = null` 之后**——`ShowSelectedBoard` 开头有 `dragItem != null` 早退，放前面会被静默吞掉，症状是「拖完选中框不动」且没有任何报错。**`HidePlacementPreview()` 与这些清理一样放在 `if` 之外**——「无论如何都该做」的收尾不能依赖 `dragItem` 是否有效，恰恰是它出错时最需要收尾。
  - `GetDropItemAt(Vector2 itemPosition, out int x, out int y)`：把物品**左上角**的 `anchoredPosition` 换算成格子坐标。与 `TryGetCellAt` 的两点区别：其一，落点必须由物品左上角决定而非鼠标位置，否则玩家抓着物品右下角拖动时，放置结果会整体偏移一整个抓取偏移量；其二，用 `RoundToInt` 而非 `FloorToInt`——`Floor` 只认「左上角落在哪格」，差几像素没对齐就会判到左边一格甚至负数，`Round` 才是「吸附到最近的格子」的手感。不做合法性判断，合法与否由 `TryMove` 回答。
  - `GetAnchorPositionForCell(int x, int y)`：格子坐标 → `anchoredPosition`，即 `(x * step, -y * step)`。`ShowItem` 与 `EndDrag` 共用，避免 `cellSize` / `spacing` 在 Inspector 改动后两处结果不一致。
- 依赖方向：View 只能**请求**数据层改动（`TryMove`），不能直接操作网格；改动成功与否一律以数据层的返回值为准，画面不按 UI 自己的预判摆放。当前两者结果必然一致，但数据层将来一旦加入重量上限、容器类别限制、堆叠合并等规则，UI 预判就会与真实结果分叉，表现为「画面搬过去了、数据还在原位」这类极难定位的问题。
- 关联：挂在 `GameScene` 的 `Inventory` 对象上。
- 已知技术债：`ScreenPointToLocalPointInRectangle` 的调用在三处重复（`TryGetCellAt`、`BeginDrag`、`Drag`），尚未抽成辅助方法。若 Canvas 改为 Screen Space - Camera，三处的摄像机参数都要改。

#### `Assets/_Game/Scripts/Runtime/UI/ItemView.cs`

- 脚本职责：单个物品在屏幕上的表现。预制体为多层结构——底板 `Image` 表达「这块区域被占用」，`Icon` 表达「这是什么」，另有一个专用于缩放的 `highlightTransform` 层。
- 关键字段：
  - `icon`：物品图标的 `Image`。勾选 `Preserve Aspect`。其 `RectTransform` 的 anchor 与 pivot **均为 (0.5, 0.5)**，尺寸由代码显式指定，**不能用四角拉伸**——拉伸的前提是「我的形状 = 父级的形状」，而旋转要求图标形状与外框**反着来**（外框 1×2 时图标必须是 2×1 的图，转 90° 后才正好填满）；且 anchor 处于拉伸状态时 `sizeDelta` 的含义会从「尺寸」变成「相对父级的增量」，直接赋值会把图标撑成父级尺寸加自身尺寸那么大。pivot 居中则是因为 `localRotation` 绕 pivot 旋转，留在左上角图标会绕角甩出去。
  - `amountLabel`：堆叠数量文字（`TMP_Text`，UI 版 `TextMeshProUGUI`），锚定在格子右下角，同样关闭 `Raycast Target`。
  - `background`：底板 `Image`。拖拽期间置为 `Color.clear`，让原位置视觉上「空出来」。
  - `highlightTransform`：**专门用于缩放的中间层**。缩放绕自身 pivot 进行，而根物体的 pivot 必须留在 (0, 1) 以服务 `anchoredPosition` 的定位公式；把缩放交给一个居中 pivot 的子物体，可让两个需求互不干扰（根物体负责「在哪一格」，子物体负责「什么表现」）。
  - `highlightScale`：高亮时的缩放倍数，默认 1.1。
- 函数：
  - `SetAmount(int amount)`：刷新右下角的数量文字。
  - `SetIcon(InventoryItem item, bool value = true)`：设置图标 sprite；`item`、其 `Data` 为空或 `value` 为 false 时禁用 `icon`，避免留下白色方块。
  - `SetBackgroundTransparent(bool)`：切换底板透明。
  - `SetHighlighted(bool)`：缩放 `highlightTransform`；高亮时调用 `SetAsLastSibling()` 让放大后的物品画在邻居之上。
  - `SetFootprint(Vector2 boxSize, Vector2 iconSize, bool rotated)`：一次性设定根物体外框尺寸、图标尺寸与图标旋转（`rotated` 时 Z 轴 -90°）。**写在 `ItemView` 而非 `InventoryView`**：`icon` 是它的私有序列化字段，`InventoryView` 既拿不到也不该知道预制体内部有哪些子物体。`iconSize` 由调用方传入原始尺寸，`boxSize` 传旋转后尺寸，两者故意分开——外框决定「占几格」，图标决定「画多大」，旋转后二者必然不同。
- 关联：预制体位于 `Assets/_Game/Prefabs/`，由 `InventoryView.ShowItem` 实例化。底板与 `Icon` 的 `Raycast Target` 均关闭，以免遮挡 `ItemLayer` 的鼠标射线——物品「是谁」一律由 `grid.GetItemAt()` 回答，不靠显示对象自报。

#### `Assets/_Game/Scripts/Runtime/UI/InventoryScreen.cs`

- 脚本职责：根据当前游戏层决定背包界面是否出现在屏幕上。
- 存在原因：开关逻辑本身早已存在——`InputRouter.OnOpenInventoryPressed` 会 `PushLayer` / `PopLayer`，`OnUICancelPressed` 也能关闭，`GameLayerStack.CurrentLayerChanged` 一直在广播。缺的只是「有人听见这一声去显示 Canvas」。此事由游戏层状态驱动，而 `InventoryView` 由背包数据驱动，两者不该由同一个类负责。
- 关键字段：`layerStack`（为空时 `Awake` 用 `FindAnyObjectByType` 兜底）、`root`（背包 Canvas）。
- 函数：
  - `Awake()`：兜底获取 `layerStack`，并 `root.SetActive(false)`。背包 UI 在场景里保持**激活**状态以便编辑（否则在 Hierarchy 里选不中、也看不到布局），由本脚本在运行时关掉。这件事**只能由 `InventoryScreen` 做**——它本来就是这个 Canvas 的唯一负责人，让 `InventoryView` 之类的脚本再去 `transform.root.gameObject.SetActive(false)` 会造成两个所有者，而且 `transform.root` 会随层级调整悄悄指向别的对象。
  - `CurrentLayerChanged(previous, current)` → `root.SetActive(current == GameLayerType.Inventory)`。
- 关联：**不能挂在 `Canvas_Inventory` 自己身上**——它被关闭后 `OnEnable` 不再执行，也就永远收不到「该打开了」的通知。负责开关某个对象的脚本必须待在那个对象之外的常驻物体上。

#### `Assets/_Game/Scripts/Runtime/UI/InventoryPointerHandler.cs`

- 脚本职责：接收 `EventSystem` 指针事件并转达给 `InventoryView`。不做任何判断。
- 存在原因：`EventSystem` 只调用**被射线击中的那个 GameObject** 上的接口，而 `InventoryView` 挂在没有 `Graphic` 的 `Inventory` 上，收不到射线。此脚本挂在真正被鼠标压住的 `ItemLayer` 上，从而把「接收输入」与「负责显示」拆开。
- 实现接口：`IPointerMoveHandler`、`IPointerExitHandler`、`IBeginDragHandler`、`IDragHandler`、`IEndDragHandler`、`IPointerClickHandler`。
- **所有指针回调都必须先按 `eventData.button` 过滤**。拖拽三件套**不区分鼠标按键**：右键按下并移动同样会走一遍完整的 `OnBeginDrag → OnDrag → OnEndDrag`。曾因此产生过一个 bug——左键拖着物品时右键在空格上一拖，第二次 `BeginDrag` 把 `dragItem` 覆盖成 null，随后左键的 `EndDrag` 因 `dragItem == null` 整段跳过清理，物品卡死在 `DragLayer` 上、预览框留在网格里。
- 函数：
  - `OnPointerMove(PointerEventData)`：把 `eventData.position` 交给 `InventoryView.UpdateHover`。
  - `OnPointerExit(PointerEventData)`：调用 `InventoryView.ClearHover`。
  - `OnBeginDrag(PointerEventData)`：传 **`eventData.pressPosition`**（按下瞬间的屏幕坐标）而非 `position`。`OnBeginDrag` 要等鼠标越过拖拽阈值才触发，此时 `position` 已偏离按下点几个像素，会同时影响抓取偏移与「按在哪一格」的判断。
  - `OnDrag(PointerEventData)` / `OnEndDrag(PointerEventData)`：转达当前位置 / 结束拖拽。`OnEndDrag` 不传坐标——落点由 `InventoryView` 用物品自身的位置算，与松手瞬间鼠标在哪无关。
  - `OnPointerClick(PointerEventData)`：仅左键进入，调用 `InventoryView.SelectItem`。用点击选中与拖拽不冲突——`EventSystem` 在一次按下里只要越过拖拽阈值就会清掉 `eligibleForClick`，松手时不再派发 `OnPointerClick`，因此「拖完松手」不会被误判成一次选中。
- **`ScrollRect` 转发**：`EventSystem` 沿层级向上只把事件交给**第一个**实现了该接口的物体，不广播。`ItemLayer` 实现了拖拽三件套，于是祖先 `InventoryArea` 上的 `ScrollRect` 永远收不到拖拽（滚轮走的是 `IScrollHandler`，本脚本没实现，所以能正常上浮）。解决办法是按下瞬间定归属：`BeginDrag` 抓到物品则本次拖拽归自己，抓空则置 `routingToScroll` 并把三个回调手动转发给 `scrollRect`。**归属必须在按下那一刻定死并用字段记住**——玩家滑动列表时鼠标会扫过一堆物品，在 `OnDrag` 里现判「当前位置有没有物品」是错的。
- 关联：挂在 `GameScene` 的 `ItemLayer` 上。

#### `Assets/_Game/Scripts/Runtime/UI/ItemDetailPanel.cs`

- 脚本职责：把「当前选中的物品」显示成右侧那块**常驻**详情面板（图标 / 名字 / 类别 / 描述 / 数量选择 / 丢弃），并把玩家的点击翻译成一次「丢弃请求」。**纯 UI，不认识网格也不认识 `PlayerInventory`**。
- 存在原因：`InventoryView` 的职责是「格子与物品在网格里的表现」——坐标换算、拖拽、放置预览、高亮，已背着三套状态；详情文字排版与它毫无关系，塞进去只会让它继续膨胀。面板的职责边界是**被动的**：它不决定谁被选中（那是 `InventoryView` 的事），也不负责自己的显隐（面板随 `Canvas_Inventory` 一起开关）；它可以**发出请求**（`DropRequested`），但不执行。
- 前身是 `ItemContextMenu`（右键唤起、跟随鼠标定位、`Close()` + 全屏 `Blocker` 关闭）。改成常驻后，**定位与开关两块逻辑整个作废**，只有「显示物品信息 + 数量选择 + 丢弃」被保留下来，`DropRequested` 与 `InventoryView.DropItem` 的连线原样沿用。
- 关键字段：`icon` / `nameLabel` / `categoryLabel` / `descriptionLabel`、`basic` / `fallBack`（有无选中项的两组互斥子物体）、`dropButton` / `minusButton` / `plusButton` / `amountRow` / `amountLabel`、`currentItem`、`currentAmount`。
- 函数：
  - `Awake()`：`basic` 关、`fallBack` 开。**刻意不像 `ItemContextMenu` 那样 `gameObject.SetActive(false)`**——面板本体是常驻的，隐藏的只是内容。
  - `Show(InventoryItem item)`：切到 `basic`、填四项信息、记下 `currentItem`、**把 `currentAmount` 重置为 1** 并 `RefreshAmount()`；`item.Amount <= 1` 时整行隐藏 `amountRow`（只有 1 个时没有「选多少」可言）。
    **重置是这次改版最容易翻车的地方**：弹出式菜单每次 `Open` 天然就是一次重新初始化，而常驻面板从头到尾只有一个实例、永远不会重新初始化，上一件物品的选择量会原样带到下一件。在「选中 10 个的药水 → 数量调到 5 → 直接点旁边只有 2 个的物品」这条路径上，不重置就会拿着 5 去丢一个只有 2 个的堆。这与 `InventoryView.BeginDrag` 里的 `dragRotated = false` 是同一条规则：**会话开始时必须清干净上一轮的会话状态**。
  - `Hide()`：切回 `fallBack`，并把 `currentItem` 置 `null`，不留悬空引用（面板空态时那件物品可能已被丢光并从网格删除）。
  - `OnPlusButton()` / `OnMinusButton()`：改 `currentAmount` 后**都必须调 `RefreshAmount()`**。上限每次直接读 `currentItem.Amount`，不缓存副本——上限的唯一真相在物品身上，抄一份就是制造第二个来源。
    曾漏写加号里的 `RefreshAmount()`，症状是**两个按钮看起来都失灵**：加号那边 `currentAmount` 其实加对了、只是标签没重画；而减号的 `interactable` 也只在 `RefreshAmount` 里重算，于是它从 `Show` 时的 `false` 再没被解锁过，形成死锁。排查这类问题先分清「数据错了」还是「数据对但没画出来」，能省一半时间。
  - `RefreshAmount()`：刷新数字文本与加减按钮的 `interactable`。**读了 `currentItem.Amount`，因此调用前必须保证 `currentItem` 非空。**
  - 按钮一律**在 `OnEnable` / `OnDisable` 里 `AddListener` / `RemoveListener`**，不在 Inspector 连 `OnClick`：Inspector 连线是隐形依赖，代码里搜不到调用关系；且面板随背包反复开关，写在 `Awake` 里会重复订阅或丢失订阅。
- 对外事件：`DropRequested`（`Action<InventoryItem, int>`），由 `InventoryView` 订阅并转成 `PlayerInventory.TryRemove`。
- 关联：挂在 `Canvas_Inventory/InventoryPart/ItemDetailPanel` 上，由 `InventoryView.SetSelectedItem` 驱动。
- 待办：图中的 `F 使用` 尚未实现——「使用一件物品」要回答「用了会发生什么」（回血多少、装备到哪、触发什么），`ItemData` 当前没有任何这类信息，属于独立的物品效果系统，不在本次改版范围内。`categoryLabel` 目前直接输出 `ItemCategory` 的英文枚举名，中文显示留待本地化。

### Runtime/Systems/InventorySystem

#### `Assets/_Game/Scripts/Runtime/Systems/InventorySystem/ItemData.cs`

- 脚本职责：背包物品数据 ScriptableObject，用于描述物品静态配置。
- 关键字段/属性：
  - `itemId`、`ItemId`：物品唯一 ID。
  - `displayName`、`DisplayName`：物品显示名称。
  - `description`、`Description`：物品描述文本。
  - `icon`、`Icon`：物品 UI 图标。
  - `category`、`Category`：物品分类。
  - `width`、`height`、`Width`、`Height`：物品占用背包格尺寸，宽度限制 1 到 4，高度限制 1 到 8。
  - `maxStack`、`MaxStack`：最大堆叠数量。
  - `canRotate`、`CanRotate`：是否允许在背包格中旋转。
- 关联：通过 `CreateAssetMenu` 暴露 `Game/Inventory/Item Data` 创建入口；被 `InventoryItem` 作为运行时物品实例的数据来源。

#### `Assets/_Game/Scripts/Runtime/Systems/InventorySystem/ItemCategory.cs`

- 脚本职责：定义背包物品分类枚举。
- 枚举值：`Material`、`Consumable`、`Weapon`、`Armor`、`Quest`、`Tool`、`Misc`。
- 关联：由 `ItemData.category` 使用，用于后续筛选、排序、页签或物品规则判断。

#### `Assets/_Game/Scripts/Runtime/Systems/InventorySystem/InventoryItem.cs`

- 脚本职责：运行时背包物品实例，保存物品数据、数量和旋转状态。
- 关键属性：
  - `Data`：引用的 `ItemData`。
  - `Amount`：当前堆叠数量。
  - `IsRotated`：当前是否旋转。
  - `CurrentWidth`、`CurrentHeight`：根据旋转状态计算后的实际占格尺寸。
- 函数：
  - `InventoryItem(ItemData data, int amount)`：校验 `data` 非空，并确保数量在 1 到 `data.MaxStack` 之间。**超过 `MaxStack` 会抛异常**——调用方有责任先把数量拆开，因为「一格装不下这么多」是调用方能预见的情况，不是构造函数该默默夹断的。
  - `CanStackWith(ItemData data)`：同一个 `ItemData` 引用且 `Amount < MaxStack` 才能堆叠。判断写在这里而不是调用方，因为条件用到的 `Data` 和 `Amount` 都是它自己的数据；以后加规则（如损坏度不同不能堆）只需改这一处。
  - `Reduce(int amount)`：减少堆叠数量。`amount <= 0` 或 **`amount >= Amount`** 都抛 `ArgumentOutOfRangeException`——后者是刻意的：「数量为 0 的 `InventoryItem`」是非法状态（还占着格子却什么都不装，玩家会看到一个抓得起来、数量为 0 的幽灵）。与其让它出现后再到处 `if (Amount == 0)`，不如让类型本身无法进入该状态；想减到 0 的那不是「减少」而是「移除」，走 `PlayerInventory.TryRemove` 的另一条分支。
  - `Add(int amount)`：增加数量，**返回没能吃下的剩余量**（未溢出则为 0）。刻意不是 `void`——若把超出 `MaxStack` 的部分默默夹掉，物品就凭空消失了；返回剩余量，调用方才有机会另开一格安置。
  - `Rotate()`：如果物品允许旋转，则切换 `IsRotated`。
- 关联：依赖 `ItemData` 的尺寸、堆叠和旋转配置；后续会由 `InventoryGrid` 或背包容器持有。

#### `Assets/_Game/Scripts/Runtime/Systems/InventorySystem/InventoryGrid.cs`

- 脚本职责：背包二维格子模型。纯 C# 类，不继承 `MonoBehaviour`，不引用任何 Unity UI 或屏幕坐标，坐标单位是"格"。
- 关键字段/属性：
  - `cells`：二维 `InventoryItem` 数组，索引顺序固定为 `cells[x, y]`。一个跨多格的物品，会让它覆盖的每一格都保存**同一个** `InventoryItem` 引用。
  - `Width`、`Height`：网格尺寸。
- 函数：
  - `InventoryGrid(int width, int height)`：创建指定尺寸的格子数组，并校验宽高必须大于 0。
  - `IsInside(int x, int y)`：判断单个坐标是否在网格范围内。
  - `IsInside(int x, int y, int areaWidth, int areaHeight)`：判断一个左上角在 `(x, y)` 的矩形区域是否完整落在网格内。只检查左上角和右下角两点，为 O(1)。
  - `IsAreaEmpty(int x, int y, int areaWidth, int areaHeight, InventoryItem ignoreItem = null)`：判断区域内每一格是否都为 `null`。命中 `ignoreItem` 的格子按「空」处理。**约定不做边界检查**，调用者需先用 `IsInside` 保证区域合法。
  - `CanPlace(InventoryItem item, int x, int y, bool ignoreItem = false)`：**纯查询，绝不修改网格**。校验 `item` 非空、区域在界内、区域为空。`ignoreItem` 为 true 时把 `item` 自己传给 `IsAreaEmpty` 当作「可忽略」——**移动已在网格中的物品时必须开启**，否则新旧区域一旦重叠，物品会被自己判定为障碍而永远挪不动一格（如 2×2 物品右移一格）。新物品入包则保持 false。
  - `CanPlace(int x, int y, int areaWidth, int areaHeight, InventoryItem ignoreItem = null)`：**按给定尺寸**判断，与物品自身的 `CurrentWidth` / `CurrentHeight` 无关。存在原因是拖拽期间的旋转只是「意图」，尚未写进 `InventoryItem.IsRotated`，此时需要回答的是**假设性问题**：「假如这件物品是 1×2，放这里行吗？」上面那个 item 版读的是物品当前的真实朝向，回答不了。`IsAreaEmpty` 本就是按 `(x, y, w, h, ignore)` 写的，把尺寸写死的只是 `CanPlace` 这一层，因此只需加重载，item 版沦为薄封装，既有调用方与单元测试零改动。
  - `Place(InventoryItem item, int x, int y, bool ignoreItem = false)`：先调 `CanPlace`，通过后把 `item` 写入覆盖到的每一格并返回 `true`；否则返回 `false` 且**不修改任何格子**（不留半填状态）。判断逻辑只存在于 `CanPlace` 一处，避免「预判」与「实际」两套规则分叉。物品尺寸取 `CurrentWidth` / `CurrentHeight`，因此自动支持旋转。
  - `Remove(InventoryItem item)`：**扫描整张表**，把所有等于该引用的格子置 `null`，返回是否至少清掉一格。刻意不用 `Remove(int x, int y)`——调用方手里通常只有「玩家点了哪一格」，那不一定是物品左上角，而网格并未记录任何物品的左上角坐标；按错误的原点往右下擦，会同时留下自己的残格并抹掉邻居的格子。用引用比较还顺带钉住一条规则：两株外观相同的草药是两件独立物品，移除一件不会波及另一件。
  - `TryFindFreeCell(InventoryItem item, out int x, out int y)`：从左上角起**逐行**（`y` 外层、`x` 内层）扫描，返回第一个放得下该物品的位置。行优先是为了匹配玩家预期——连续拾取时物品一行行往下铺，而不是一列列往右铺。内部直接调用 `CanPlace`，不自己重写判断，规则只保留一份。
  - `FindStackable(ItemData data)`：行优先扫描，返回第一个 `CanStackWith(data)` 为 true 的物品，没有则返回 `null`。
  - `GetPlacedItems()`：返回 `IEnumerable<(InventoryItem item, int x, int y)>`，供 UI 全量重建使用。返回 `IEnumerable` 而非 `List`，是只给调用方「遍历」这一项能力。**用 `HashSet` 去重**——多格物品在 `cells` 里出现多次；按行优先扫描，**第一次遇到它的那一格必然是它的左上角**（占用区域是矩形），因此无需额外记录原点坐标。
  - `GetItemAt(int x, int y)`：返回该格的物品；**越界返回 `null` 而不抛异常**，因为将来会由鼠标位置驱动调用，划出背包范围属于正常情况。
  - `TryGetItemPosition(InventoryItem item, out int x, out int y)`：`GetItemAt` 的**反方向**查询——由物品反查它的原点格。行优先扫描，**第一次撞到该引用的那一格必然是它的左上角**（占用区域是矩形），与 `GetPlacedItems` 依赖的是同一个前提，因此无需额外取最小值。存在原因：位置这本账属于 `InventoryGrid`，`InventoryItem` 自身**不知道也不该知道**它在网格的哪里；而 UI 要把选中框画在物品所占的整个矩形上，只有点击格是不够的（点 2×1 物品的右半格时，点击格与原点格差一整格）。
- 关联：被 `PlayerInventory` 持有。尚未实现堆叠合并、旋转与网格的联动。

#### `Assets/_Game/Scripts/Runtime/Systems/InventorySystem/PlayerInventory.cs`

- 脚本职责：把 `InventoryGrid` 接入游戏运行时。玩家身上的背包组件。
- 关键字段：
  - `width`、`height`：网格尺寸，`[SerializeField]` 暴露给 Inspector，当前为 4 × 8（与 `GameScene` 中已有的 32 个 Slot 对应）。
  - `grid` / `Grid`：运行时的 `InventoryGrid` 实例，不对外暴露。**由 `Grid` 属性惰性创建（`grid ??= new InventoryGrid(...)`），不在 `Awake()` 里建**——Unity 只保证「同一对象的 `Awake` 早于它自己的 `OnEnable`」以及「所有 `Awake`/`OnEnable` 早于任何 `Start`」，跨对象的先后由场景加载次序决定，`InventoryView.OnEnable` 完全可能先跑并立刻索要数据。惰性创建让「谁先访问谁负责建」，不依赖任何执行顺序；副作用是这个 MonoBehaviour 在 EditMode 测试里也能直接用（生命周期回调不跑也没关系）。`??=` 在此安全，是因为 `InventoryGrid` 是纯 C# 类，不存在 Unity 的假 null。
- 事件：
  - `ItemPlaced`（`Action<InventoryItem, int, int>`）：物品**占用了新格子**后触发，UI 据此新建一个 `ItemView`。
  - `ItemRemoved`（`Action<InventoryItem>`）：物品**整个离开网格**后触发，是 `ItemPlaced` 的对偶。数据层不能也不该去 `Destroy` 一个 `ItemView`——`PlayerInventory` 在 asmdef 内根本引用不到 UI 层的类型；它唯一能做的就是喊一声「这件东西没了」，谁持有它的显示对象谁自己负责收尾。
  - `ItemAmountUpdated`（`Action<InventoryItem>`）：已有物品的**数量**发生变化后触发，UI 据此只更新数字。两个事件必须分开：堆叠不产生新物品、不占新格子，若沿用 `ItemPlaced`，屏幕上会多出一个数据层不存在的物品。均属 View ↔ Model 的局部同步，因此使用 C# `event Action` 而非 `GameEventBus`；后者用于「玩家获得物品」这类真正跨系统的事件。
- 函数：
  - `TryAdd(ItemData data, int amount = 1)`：外部把物品放进背包的唯一入口。流程为**先堆叠、后开新格**：`FindStackable` 找到未满的同类堆 → `Add` → 触发 `ItemAmountUpdated`；否则本格装 `Math.Min(amount, MaxStack)` 个，`TryFindFreeCell` → `Place` → 触发 `ItemPlaced`。两条路都可能剩下一部分装不下，**统一用递归把剩余量交给下一轮**，因此「一次捡起一堆超过 MaxStack 的物品」会自动拆成多格。`Math.Min` 的夹断只属于开新格这条路：堆叠路上 `Add` 自己会算溢出，在外面先夹一刀会导致溢出被算两遍、丢失一部分。找不到空位时返回 false 且**一个都没放进去**（此时不存在「剩余」的概念）。**UI 无需任何改动即可显示新物品**——`InventoryView` 早已订阅这两个事件。
  - 已知不足：返回 `bool` 无法表达「只放进去了一部分」。玩家捡起 2 株而背包只塞得下 1 株时，数据层已收下 1 株却返回 false，调用方（`WorldItem`）会因此不销毁世界物体，导致物品被复制。诚实的签名应与 `InventoryItem.Add` 一致，返回未能放入的剩余量。
  - `TryRemove(InventoryItem item, int amount)`：从背包中扣除 `amount` 个。**两条分支**，由「扣完还剩不剩」决定：`amount >= item.Amount` 时物品整个离开网格，走 `Grid.Remove()` 并触发 `ItemRemoved`；否则只是数字变小，走 `item.Reduce()` 并触发 `ItemAmountUpdated`。分支判断**必须在动手之前**——`Reduce` 一执行 `Amount` 就变了。移除分支用 `Grid.Remove()` 的返回值把关（物品不在本网格时返回 false），失败就不发事件，否则 UI 会销毁一个数据层根本没删掉的对象。
    「减少」分支目前**未校验物品是否真的属于本背包**（移除分支靠 `Grid.Remove` 的返回值免费拿到了这个校验）。当前没有调用方能构造出该情况，等物品需要在多个容器间流动时再补。
  - `TryMove(InventoryItem item, int x, int y, bool rotated)`：把已在网格中的物品移到 `(x, y)` 并落实为 `rotated` 指定的朝向，返回是否成功。内部顺序为**尺寸版** `CanPlace(x, y, w, h, ignoreItem: item)` → `grid.Remove()` → 朝向不一致才 `item.Rotate()` → `grid.Place()`。`Rotate()` **必须在 `Place()` 之前**：`Place` 是按 `item.CurrentWidth` / `CurrentHeight` 铺格子的，转晚了就按旧尺寸占格，导致「物品尺寸与格子占用不一致」。`Remove` 按引用扫全表清除，与尺寸无关，放前放后皆可。校验用尺寸版而非 item 版，因为此刻 `IsRotated` 还是旧值。
    **这是一个事务**：校验不通过就 `return false` 且一个字节都不改，不存在「转了一半失败要回滚」的中间态。另一种写法是让 UI 先 `item.Rotate()` 再 `TryMove`、失败了再 `Rotate()` 回去——回滚代码是 bug 温床（漏分支、中途异常、后来者新增失败条件忘了补），而且坏掉的是玩家存档不是画面。**校验必须写在这里而不是调用方**：`Remove` 一旦执行，物品就必须有地方落，否则它会从数据层彻底消失，而屏幕上的 `ItemView` 和 `itemViews` 字典仍在，玩家会看到一个抓得到、却已不存在于网格中的「鬼影」。把这条底线交给某个 UI 类去守，等于让其他调用方（拾取、容器转移）随时能绕过它。
  - `IsInside(int x, int y)` / `GetItemAt(int x, int y)` / `TryGetItemPosition(InventoryItem, out int, out int)` / `CanPlace(InventoryItem, int, int, bool)` / `CanPlace(int, int, int, int, InventoryItem)` / `GetPlacedItems()`：向 `InventoryGrid` 的转发方法，供 UI 层查询。刻意不暴露 `grid` 本身，以免外部绕过 `TryAdd` / `TryMove` 直接改数据而跳过校验与事件通知。
- 关联：挂在 `GameScene` 的 `Player` 对象上。原先 `Start()` 里的调试放置与 `PrintGrid()` 已随拾取流程接通而移除。

#### `Assets/_Game/Scripts/Runtime/Systems/InventorySystem/FirstGame.Inventory.asmdef`

- 职责：把背包系统编译成独立程序集 `FirstGame.Inventory`，从默认的 `Assembly-CSharp` 中分离出来。
- 配置要点：
  - `references` 为空：背包系统在**编译层面**无法引用项目中任何其他模块，依赖方向由编译器强制。
  - `autoReferenced: true`：`Assembly-CSharp` 会自动引用它，因此其余脚本使用 `ItemData` 等类型无需额外配置。
- 目的：让 `InventoryGrid` 等纯逻辑类可以被 EditMode 单元测试引用（asmdef 无法引用 `Assembly-CSharp`）。

#### `Assets/_Game/Tests/EditMode/`

- `FirstGame.Inventory.Tests.asmdef`：EditMode 测试程序集。引用 `FirstGame.Inventory`、`UnityEngine.TestRunner`、`UnityEditor.TestRunner`，`includePlatforms` 限定为 `Editor`，因此不会打进游戏包。
- `InventoryGridTests.cs`：`InventoryGrid` 的单元测试。覆盖构造校验、`IsInside` 单点与矩形版、`IsAreaEmpty` 空与被占两种情况、`Place` 的成功/越界/重叠/失败不留痕、`GetItemAt` 越界返回 null。
  - 测试内通过 `JsonUtility.FromJsonOverwrite` 写入 `ItemData` 的私有序列化字段来构造测试物品，避免为了测试放宽生产代码的封装。
  - 运行方式：`Window → General → Test Runner → EditMode → Run All`。

### Runtime/GameFlow

#### `Assets/_Game/Scripts/Runtime/GameFlow/GameBootstrap.cs`

- 脚本职责：启动流程入口，确保启动场景中的核心服务存在，并跳转到主菜单。
- 关键字段：
  - `sceneLoader`：启动后加载主菜单。
  - `layerStack`：启动时建立初始层。
- 函数：
  - `Awake()`：通过 `RequireComponent` 和组件查找确保 `SceneLoader`、`GameLayerStack` 可用。
  - `Start()`：重置当前层并加载主菜单场景。
- 关联：BootScene 中的启动对象使用它把项目从启动场景导入 `MainMenuScene`。

### Runtime/Input

#### `Assets/_Game/Scripts/Runtime/Input/GameInputReaders.cs`

- 脚本职责：包装 Unity Input System 生成类，把输入回调转换为项目内部事件和输入缓存。
- 关键字段/事件：
  - `inputActions`：`InputSystem_Actions` 生成对象。
  - `MoveInput`：当前玩家移动输入。
  - `UINavigateInput`：当前 UI 导航输入。
  - `MoveChanged`、`JumpPressed`、`AttackPressed`、`DashPressed`、`InteractPressed`、`UseItemPressed`：玩家输入事件。
  - `PausePressed`、`OpenInventoryPressed`、`OpenMapPressed`：游戏级输入事件。
  - `UINavigateChanged`、`UISubmitPressed`、`UICancelPressed`：UI 输入事件。
- 函数：
  - `Awake()`：创建 `InputSystem_Actions` 并绑定所有回调。
  - `OnEnable()`：启用输入系统。
  - `OnDisable()`：禁用输入系统。
  - `SetInputMode(GameLayerType layerType)`：根据游戏层启用/禁用 Player、Game、UI Action Map。
  - `OnDestroy()`：释放输入对象。
  - `BindPlayerInput()`：绑定移动、跳跃、攻击、冲刺、交互、使用物品回调。
  - `BindGameInput()`：绑定暂停、背包、地图回调。
  - `BindUIInput()`：绑定 UI 导航、确认、取消回调。
- 关联：`InputRouter` 订阅这些事件，并根据 `GameLayerStack.CurrentLayer` 决定分发目标。

#### `Assets/_Game/Scripts/Runtime/Input/InputRouter.cs`

- 脚本职责：输入路由层，把 `GameInputReader` 的原始事件分发到玩家、对话、UI 或系统。
- 关键字段：
  - `inputReader`：输入事件来源。
  - `gameLayerStack`：判断当前输入层。
  - `playerControlArbitration`：判断当前层是否允许玩家动作。
  - `playerInputReceiver`：接收玩家行动请求。
  - `dialogueManager`：接收对话推进和选项输入。
- 函数：
  - `Awake()`：补齐必要引用。
  - `OnEnable()`：订阅输入事件和层变化事件。
  - `OnDisable()`：取消订阅。
  - `OnCurrentLayerChanged(GameLayerType previousLayer, GameLayerType currentLayer)`：层变化时调用 `inputReader.SetInputMode()`。
  - `OnMoveChanged(Vector2 moveInput)`：Gameplay 且允许移动时设置玩家移动输入，否则清空。
  - `OnJumpPressed()`：Gameplay 且允许跳跃时登记跳跃请求。
  - `OnAttackPressed()`：Gameplay 且允许攻击时登记攻击请求。
  - `OnDashPressed()`：Gameplay 且允许冲刺时登记冲刺请求。
  - `OnInteractPressed()`：Gameplay 时登记世界交互请求；Dialogue 时请求推进台词。
  - `OnUseItemPressed()`：Gameplay 且允许使用物品时处理使用物品入口，目前预留。
  - `OnPausePressed()`：Gameplay 或其他允许层中处理暂停入口，目前预留。
  - `OnOpenInventoryPressed()`：处理背包输入入口，目前预留。
  - `OnOpenMapPressed()`：处理地图输入入口，目前预留。
  - `OnUINavigateChanged(Vector2 navigateInput)`：DialogueChoice 层把导航交给对话选项。
  - `OnUISubmitPressed()`：DialogueChoice 层确认当前选项。
  - `OnUICancelPressed()`：处理 UI 取消输入入口，目前预留。
  - `IsCurrentLayer(GameLayerType layerType)`：封装当前层判断。
- 关联：位于 `GameInputReader` 和游戏系统之间；既依赖 `GameLayerStack`，也依赖 `PlayerControlArbitration` 的动作锁定结果。

### Runtime/Core

#### `Assets/_Game/Scripts/Runtime/Core/SceneNames.cs`

- 脚本职责：集中保存场景名常量，避免脚本中散落字符串。
- 常量：
  - `MainMenuScene`：主菜单场景名。
  - `GameScene`：游戏主场景名。
- 关联：`SceneLoader.LoadMainMenu()`、`SceneLoader.LoadGameScene()` 使用这些常量。

#### `Assets/_Game/Scripts/Runtime/Core/SceneLoader.cs`

- 脚本职责：封装场景加载与退出游戏。
- 函数：
  - `LoadScene(string sceneName)`：按名称加载场景；空字符串会输出警告并中止。
  - `LoadMainMenu()`：加载 `SceneNames.MainMenuScene`。
  - `LoadGameScene()`：加载 `SceneNames.GameScene`。
  - `QuitGame()`：在编辑器中停止播放，在构建版本中退出应用。
- 关联：`GameBootstrap` 和 `MainMenuController` 使用它进行场景流转。

#### `Assets/_Game/Scripts/Runtime/Core/GameLayerType.cs`

- 脚本职责：定义游戏当前逻辑层。
- 枚举值：
  - `None`：无层。
  - `MainMenu`：主菜单。
  - `Gameplay`：正常游玩。
  - `Dialogue`：对话台词播放。
  - `DialogueChoice`：对话选项。
  - `Inventory`：背包。
  - `Map`：地图。
  - `ServiceMenu`：系统服务菜单。
  - `Pause`：暂停。
  - `Cutscene`：演出或剧情控制。
- 关联：`GameLayerStack` 保存该类型；`GameInputReader` 和 `InputRouter` 根据它切换输入。

#### `Assets/_Game/Scripts/Runtime/Core/GameLayerRule.cs`

- 脚本职责：单条层规则数据，用于描述某层锁定哪些玩家动作。
- 字段：
  - `layerType`：规则对应的层。
  - `lockedPlayerActions`：该层锁定的 `PlayerActionType` 位标记。
- 关联：被 `GameLayerRuleDatabase` 序列化保存。

#### `Assets/_Game/Scripts/Runtime/Core/GameLayerRuleDatabase.cs`

- 脚本职责：ScriptableObject 数据库，集中配置不同游戏层的玩家动作锁定规则。
- 字段：
  - `layerRules`：层规则列表。
- 函数：
  - `GetLockedPlayerActions(GameLayerType layer)`：查找指定层的锁定动作；无规则时返回 `PlayerActionType.None`。
- 关联：`PlayerControlArbitration` 查询它来判断移动、跳跃、攻击、交互等是否可执行。

#### `Assets/_Game/Scripts/Runtime/Core/GameLayerStack.cs`

- 脚本职责：维护游戏层栈，支持临时压入对话、选项、暂停等层。
- 关键字段/属性：
  - `layerStack`：内部层栈。
  - `CurrentLayer`：当前栈顶层；空栈时为 `None`。
  - `CurrentLayerChanged`：当前层变化事件。
- 函数：
  - `ResetTo(GameLayerType layer)`：清空栈并设置唯一当前层。
  - `PushLayer(GameLayerType layer)`：压入新层，并通知层变化。
  - `PopLayer(GameLayerType layer)`：只有栈顶等于目标层时才弹出，避免错误弹层。
  - `IsCurrentLayer(GameLayerType layer)`：判断栈顶是否为指定层。
  - `ContainsLayer(GameLayerType layer)`：判断层栈中是否包含某层。
  - `GetActiveLayers()`：返回当前所有活动层快照。
  - `NotifyLayerChanged(GameLayerType previousLayer, GameLayerType currentLayer)`：触发层变化事件。
- 关联：`DialogueManager` 压入/弹出 `Dialogue`、`DialogueChoice`；`InputRouter` 监听层变化切换输入模式。

### Runtime/Core/FSM

#### `Assets/_Game/Scripts/Runtime/Core/FSM/Entity.cs`

- 脚本职责：可拥有状态机的实体基类。
- 关键字段：
  - `anim`：实体动画器。
  - `stateMachine`：实体状态机。
- 函数：
  - `Awake()`：创建 `StateMachine`。
- 关联：`Player` 继承它并创建具体玩家状态。

#### `Assets/_Game/Scripts/Runtime/Core/FSM/EntityState.cs`

- 脚本职责：通用实体状态基类。
- 关键字段：
  - `stateMachine`：状态切换入口。
  - `anim`：状态控制的动画器。
  - `stateName`：Animator 参数 hash。
- 函数：
  - `EntityState(StateMachine stateMachine, int stateName, Animator anim)`：保存状态机、动画器和动画参数。
  - `Enter()`：进入状态时把动画 bool 置为 true。
  - `LogicalUpdate()`：每帧逻辑更新入口，默认空实现。
  - `PhysicalUpdate()`：物理帧更新入口，默认空实现。
  - `Exit()`：退出状态时把动画 bool 置为 false。
- 关联：所有玩家状态继承它的动画开关和生命周期。

#### `Assets/_Game/Scripts/Runtime/Core/FSM/StateMachine.cs`

- 脚本职责：简单状态机，统一驱动当前状态生命周期。
- 属性：
  - `currentState`：当前状态。
- 函数：
  - `InitializeState(EntityState currentState)`：设置初始状态并调用 `Enter()`。
  - `ChangeState(EntityState stateChangeTo)`：退出旧状态，切换并进入新状态。
  - `LogicalUpdate()`：转发到当前状态的逻辑更新。
  - `PhysicalUpdate()`：转发到当前状态的物理更新。
- 关联：`Player.Update()`、`Player.FixedUpdate()` 分别调用逻辑和物理更新。

#### `Assets/_Game/Scripts/Runtime/Core/FSM/Movement.cs`

- 脚本职责：基础移动组件，封装 Rigidbody2D 速度设置和视觉翻转。
- 关键字段/属性：
  - `visualLayer`：需要水平翻转的视觉层。
  - `rb`：角色 Rigidbody2D。
  - `facingRight`：当前朝向。
- 函数：
  - `Awake()`：缓存 Rigidbody2D。
  - `HandleMoveAndFlip(Vector2 inputMove)`：基础移动和翻转入口，供子类覆盖。
  - `SetRigibodyVelocity(Vector2 velocity)`：直接设置刚体速度。
  - `GetCurrentVelocity()`：返回当前刚体速度。
  - `HandleFlip(Vector2 inputMove)`：根据输入方向决定是否翻转。
  - `Flip()`：切换 `facingRight` 并反转视觉层 X 方向。
- 关联：`PlayerMovement` 继承它；NPC 显示脚本也引用 `Movement` 获取朝向。

### Runtime/Core/Events

#### `Assets/_Game/Scripts/Runtime/Core/Events/IGameEvent.cs`

- 脚本职责：事件总线事件的标记接口。
- 关联：`GameEventBus` 只接受实现该接口的事件类型。

#### `Assets/_Game/Scripts/Runtime/Core/Events/GameEventBus.cs`

- 脚本职责：类型安全的运行时事件总线。
- 关键字段：
  - `eventTable`：事件类型到委托链的映射。
- 函数：
  - `Subscribe<T>(Action<T> listener)`：订阅指定事件类型。
  - `Unsubscribe<T>(Action<T> listener)`：取消订阅，并在无监听者时清理表项。
  - `Publish<T>(T gameEvent)`：发布事件给该类型所有订阅者。
- 关联：Flag、Quest、对话信号和调试监听都通过它解耦通信。

#### `Assets/_Game/Scripts/Runtime/Core/Events/GameSignalEvent.cs`

- 脚本职责：通用信号事件。
- 属性：
  - `SignalID`：信号 ID。
  - `Sender`：发送者。
  - `Instigator`：触发者。
- 函数：
  - `GameSignalEvent(string signalID, GameObject sender, GameObject instigator)`：构造不可变事件数据。
- 关联：`PublishSignalDialogueEvent` 和调试信号脚本发布它。

#### `Assets/_Game/Scripts/Runtime/Core/Events/GameFlagChangedEvent.cs`

- 脚本职责：记录布尔 Flag 变化。
- 属性：
  - `FlagID`：变化的 Flag ID。
  - `HadPreviousValue`：此前是否已经存在该 Flag。
  - `PreviousValue`：旧值。
  - `CurrentValue`：新值。
  - `Sender`：发送者。
  - `Instigator`：触发者。
- 函数：
  - `GameFlagChangedEvent(...)`：构造不可变事件数据。
- 关联：`GameFlagCenter.SetBool()` 在值变化时发布它。

#### `Assets/_Game/Scripts/Runtime/Core/Events/DebugGameSignalPublisher.cs`

- 脚本职责：调试用信号发布器。
- 关键字段：
  - `gameEventBus`：事件总线。
  - `signalID`：测试信号 ID。
  - `instigator`：测试触发者。
- 函数：
  - `Awake()`：补齐事件总线引用。
  - `PublishTestSignal()`：通过 Context Menu 发布测试 `GameSignalEvent`。
- 关联：与 `DebugGameSignalListener` 配合验证事件总线。

#### `Assets/_Game/Scripts/Runtime/Core/Events/DebugGameSignalListener.cs`

- 脚本职责：调试用信号监听器。
- 关键字段：
  - `gameEventBus`：事件总线。
- 函数：
  - `Awake()`：补齐事件总线引用。
  - `OnEnable()`：订阅 `GameSignalEvent`。
  - `OnDisable()`：取消订阅。
  - `OnGameSignal(GameSignalEvent gameSignalEvent)`：收到信号后输出日志。
- 关联：用于观察 `PublishSignalDialogueEvent` 或调试发布器是否正常发出信号。

### Runtime/Core/Flags

#### `Assets/_Game/Scripts/Runtime/Core/Flags/GameFlagData.cs`

- 脚本职责：单个布尔 Flag 的 ScriptableObject 定义。
- 属性：
  - `FlagID`：Flag 唯一 ID。
  - `DefaultValue`：默认值。
  - `Description`：说明文字。
- 关联：`GameFlagDatabase` 收集它；`FlagBoolCondition` 和 `SetFlagDialogueEvent` 引用它。

#### `Assets/_Game/Scripts/Runtime/Core/Flags/GameFlagDatabase.cs`

- 脚本职责：Flag 数据库。
- 属性：
  - `BoolFlags`：所有初始布尔 Flag。
- 关联：`GameFlagCenter.InitializeBoolFlags()` 从这里读默认值。

#### `Assets/_Game/Scripts/Runtime/Core/Flags/GameFlagCenter.cs`

- 脚本职责：运行时 Flag 中心，保存和广播布尔 Flag。
- 关键字段：
  - `eventBus`：广播 Flag 变化。
  - `initialBoolFlags`：初始 Flag 数据库。
  - `boolFlags`：运行时 Flag 值表。
- 函数：
  - `Awake()`：补齐事件总线并初始化默认 Flag。
  - `SetBool(string flagID, bool value, GameObject sender, GameObject instigator)`：设置 Flag；若值变化则发布 `GameFlagChangedEvent`。
  - `GetBool(string flagID, bool defaultValue = false)`：按 ID 读取 Flag，不存在时返回默认值。
  - `GetBool(GameFlagData flagData)`：按资源引用读取 Flag，资源为空时返回 false。
  - `HasBool(string flagID)`：判断运行时是否存在该 Flag。
  - `InitializeBoolFlags()`：把 `GameFlagDatabase` 中的默认值写入运行时表。
- 关联：条件、对话事件、剧情序列上下文都通过它读写剧情开关。

### Runtime/Core/Quests

#### `Assets/_Game/Scripts/Runtime/Core/Quests/QuestState.cs`

- 脚本职责：定义 Quest 生命周期状态。
- 枚举值：
  - `NotStarted`：未开始。
  - `InProgress`：进行中。
  - `Completed`：已完成。
  - `Rewarded`：已领奖。
  - `Failed`：失败。
- 关联：`QuestData` 默认状态、`QuestManager` 运行时状态、`QuestStateCondition` 条件都使用它。

#### `Assets/_Game/Scripts/Runtime/Core/Quests/QuestData.cs`

- 脚本职责：单个 Quest 的 ScriptableObject 定义。
- 属性：
  - `QuestID`：Quest 唯一 ID。
  - `Title`：标题。
  - `Description`：说明。
  - `DefaultState`：默认状态。
- 关联：`QuestDatabase` 收集它；对话事件和条件引用它。

#### `Assets/_Game/Scripts/Runtime/Core/Quests/QuestDatabase.cs`

- 脚本职责：Quest 数据库。
- 属性：
  - `QuestDatas`：所有初始 Quest。
- 关联：`QuestManager.InitializeQuests()` 从这里注册 Quest。

#### `Assets/_Game/Scripts/Runtime/Core/Quests/QuestManager.cs`

- 脚本职责：运行时 Quest 状态中心。
- 关键字段：
  - `questDatabase`：Quest 定义来源。
  - `eventBus`：广播状态变化。
  - `questDataByID`：Quest ID 到数据资源映射。
  - `questStates`：Quest ID 到当前状态映射。
- 函数：
  - `Awake()`：补齐事件总线并初始化 Quest。
  - `HasQuest(string questID)`：按 ID 判断 Quest 是否注册。
  - `HasQuest(QuestData questData)`：按资源判断 Quest 是否注册。
  - `GetQuestState(QuestData questData)`：按资源读取 Quest 状态。
  - `GetQuestState(string questID)`：按 ID 读取 Quest 状态；未注册时返回 `NotStarted`。
  - `SetQuestState(QuestData questData, QuestState newState, GameObject sender = null, GameObject instigator = null)`：设置状态并发布 `QuestStateChangedEvent`。
  - `InitializeQuests()`：把数据库中的 Quest 默认状态写入运行时表。
- 关联：`QuestStateCondition` 读取它；`SetQuestStateDialogueEvent` 写入它；`DebugQuestStateListener` 监听变化。

#### `Assets/_Game/Scripts/Runtime/Core/Quests/QuestStateChangedEvent.cs`

- 脚本职责：Quest 状态变化事件。
- 属性：
  - `QuestData`：Quest 资源。
  - `QuestID`：Quest ID。
  - `PreviousState`：旧状态。
  - `CurrentState`：新状态。
  - `Sender`：发送者。
  - `Instigator`：触发者。
- 函数：
  - `QuestStateChangedEvent(...)`：构造不可变事件数据。
- 关联：`QuestManager.SetQuestState()` 发布它。

#### `Assets/_Game/Scripts/Runtime/Core/Quests/DebugQuestStateListener.cs`

- 脚本职责：调试 Quest 状态变化。
- 关键字段：
  - `eventBus`：事件总线。
- 函数：
  - `Awake()`：补齐事件总线引用。
  - `OnEnable()`：订阅 `QuestStateChangedEvent`。
  - `OnDisable()`：取消订阅。
  - `OnQuestStateChanged(QuestStateChangedEvent questEvent)`：输出 Quest 状态变化日志。
- 关联：用于验证对话事件或其它系统是否正确改动 Quest。

### Runtime/Core/Conditions

#### `Assets/_Game/Scripts/Runtime/Core/Conditions/GameCondition.cs`

- 脚本职责：条件 ScriptableObject 抽象基类。
- 函数：
  - `IsMet(GameConditionContext context)`：由子类实现，返回条件是否满足。
- 关联：`ConditionalDialogueEntry` 通过它统一评估 Flag、Quest 等条件。

#### `Assets/_Game/Scripts/Runtime/Core/Conditions/GameConditionContext.cs`

- 脚本职责：条件检查上下文。
- 属性：
  - `FlagCenter`：Flag 读取入口。
  - `QuestManager`：Quest 读取入口。
  - `Sender`：条件请求发送者。
  - `Instigator`：触发者。
- 函数：
  - `GameConditionContext(...)`：构造不可变上下文。
- 关联：`NPCDialogueInteractable` 创建它传给 `NPCDialogueProfile`。

#### `Assets/_Game/Scripts/Runtime/Core/Conditions/FlagBoolCondition.cs`

- 脚本职责：布尔 Flag 条件。
- 关键字段：
  - `flagData`：要检查的 Flag 资源。
  - `expectedValue`：期望值。
- 函数：
  - `IsMet(GameConditionContext context)`：从 `context.FlagCenter` 读取 Flag 并比较期望值。
- 关联：用于控制 NPC 对话入口是否可用。

#### `Assets/_Game/Scripts/Runtime/Core/Conditions/QuestStateCondition.cs`

- 脚本职责：Quest 状态条件。
- 关键字段：
  - `questData`：要检查的 Quest 资源。
  - `expectedState`：期望状态。
- 函数：
  - `IsMet(GameConditionContext context)`：从 `context.QuestManager` 读取 Quest 状态并比较期望状态。
- 关联：用于按任务状态切换 NPC 对话。

### Runtime/GamePlay/Interaction

#### `Assets/_Game/Scripts/Runtime/GamePlay/Interaction/IIteractable.cs`

- 脚本职责：世界交互对象接口。
- 成员：
  - `InteractionTransform`：交互对象的位置。同时充当「从接口引用摸回 GameObject」的跳板，`InteractionDetector` 靠它拿到目标身上的 `InteractionPrompt`。
  - `CanInteract(InteractionContext context)`：判断当前上下文是否允许交互。
  - `Interact(InteractionContext context)`：执行交互。
  - `GetInteractionPrompt(InteractionContext context)`：返回该对象的交互提示文案（`WorldItem` 返回按键提示，`NPCDialogueInteractable` 返回 `"Talk"`）。提示 UI 只问接口，因此新增宝箱、门等交互物时无需改动 UI。
- 关联：`InteractionDetector` 只依赖该接口，不关心具体交互对象类型。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Interaction/InteractionContext.cs`

- 脚本职责：交互上下文。
- 属性：
  - `Interactor`：发起交互的 GameObject。
  - `IneractorTransform`：发起者 Transform，当前属性名存在拼写 `Ineractor`。
- 函数：
  - `InteractionContext(GameObject interactor)`：保存交互发起者和 Transform。
- 关联：由 `InteractionDetector.TryInteract()` 创建，传给所有 `IInteractable`。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Interaction/InteractionDetector.cs`

- 脚本职责：检测玩家范围内可交互对象，持续维护「当前目标」，并驱动其头顶提示。
- 关键字段：
  - `interactor`：发起交互的对象，通常是玩家。
  - `interactions`：当前触发器范围内的交互接口列表。
  - `current`：当前最近的可交互目标。**它是「谁会被交互」这个问题的唯一答案来源**——物品自己靠 `OnTriggerEnter2D` 判断要不要显示提示是错的，两个物品同时在范围内时会一起亮，而按键只会触发最近的那个。
  - `currentPrompt`：当前目标身上的 `InteractionPrompt` 组件缓存。**必须缓存**：切换目标时要关闭上一个提示，而上一个目标很可能已经被销毁（刚被捡走的物品），回头访问它的 `InteractionTransform` 会抛 `MissingReferenceException`。
- 函数：
  - `Awake()`：默认把自身对象作为 `interactor`。
  - `Update()`：先剔除列表中已销毁的引用，再求最近目标；目标变化时才刷新提示。
  - `IsAlive(IInteractable)`：`interaction as UnityEngine.Object != null`。**Unity 给 `UnityEngine.Object` 重载的 `==`（「假 null」）按编译期类型分派，接口类型的引用不会走那个重载**，因此 `current == null` 挡不住已销毁的对象。先 `as` 回 `UnityEngine.Object` 才能恢复保护。同理，`?.` 与 `??` 是 C# 语法，同样绕过该重载，**对 Unity 对象不可用**。
  - `RefreshPrompt(IInteractable)`：关闭缓存的旧提示，再从新目标的 `InteractionTransform` 上 `GetComponent<InteractionPrompt>()` 并显示 `GetInteractionPrompt()` 的返回值。`GetComponent` 只在目标切换时调用，不在每帧。
  - `TryInteract()`：直接使用 `current`（用 `IsAlive` 挡住 null 与已销毁），创建上下文并调用 `Interact()`。
  - `OnTriggerEnter2D(Collider2D collision)`：进入范围时收集 `IInteractable`。用的是 `collision.GetComponent`，因此**实现脚本必须与 `Collider2D` 挂在同一个 GameObject 上**，挂到父子物体上会静默失效。
  - `OnTriggerExit2D(Collider2D collision)`：离开范围时移除 `IInteractable`。**不能依赖它清理被销毁的对象**：它跟随物理更新，而 `Update` 每帧都跑，中间至少隔一帧。
  - `FindClosetInteraction()`：按距离选择最近的目标。
- 关联：`PlayerGround` 在消费世界交互输入后调用它；实现方为 `NPCDialogueInteractable` 与 `WorldItem`。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Interaction/WorldItem.cs`

- 脚本职责：世界中的一件物品实例。持有 `ItemData` 说明自己是什么，实现 `IInteractable` 响应拾取。
- 存在原因：`ItemData` 是 ScriptableObject——它是「草药这种东西」的定义，全项目只有一份；而地上这株草药是场景中的**实例**，有位置、有碰撞体、会被捡走后销毁。把交互逻辑写进 `ItemData`，等于让物品定义知道自己被摆在世界的哪个角落，多个实例会互相打架。
- 关键字段：
  - `itemData`：这件物品是什么。
  - `icon`：世界中显示用的 `SpriteRenderer`。
- 函数：
  - `OnValidate()`：编辑器中改动 `itemData` 后，自动同步 GameObject 名称与 `icon.sprite`，省去手工配置。
  - `Interact(InteractionContext context)`：从 `context.Interactor` 上取 `PlayerInventory`，调 `TryAdd`，成功才 `Destroy(gameObject)`。**背包引用来自 context 而非 `[SerializeField]`**：写死引用不但要给每个物品实例手工拖一次，还锁死了「只有这一个玩家能捡」；交互的发起者本来就该由交互系统告知。
- 关联：挂在 `GameScene` 的 `WorldItem_*` 对象上，需与 `Collider2D` 同处一个 GameObject。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Interaction/InteractionPrompt.cs`

- 脚本职责：显示在交互对象头顶的提示文字。挂在对象本体（始终 active）上，内部开关一个子物体做显隐。
- 关键字段：
  - `text`：3D 版 `TextMeshPro`（`MeshRenderer`），**不是 World Space Canvas**。Canvas 的意义在于把整套 UI 布局系统搬进世界空间，为一个字付出 `Canvas` + `CanvasRenderer` + 重建开销并不划算；3D TMP 的字号直接是世界单位，`Scale` 保持 1，前后关系由 `MeshRenderer` 的 `Sorting Layer` / `Order in Layer` 控制。若日后需要背景板、按键图标或可点击元素，再换回 Canvas。
- 函数：
  - `ShowPrompt(string prompt)` / `HidePrompt()`：设置文本并显隐子物体。
- 关联：由 `InteractionDetector.RefreshPrompt` 驱动。**挂在物品身上而非全场共用一个**：提示无需跟随代码（子物体天然跟着物体走），且不同高度的对象可各自调偏移；代价是「同时只有一个目标」必须由 `InteractionDetector` 主动维护。未挂此组件的交互对象（当前的 NPC）不会显示提示，也不会报错。

### Runtime/GamePlay/Dialogue

#### `Assets/_Game/Scripts/Runtime/GamePlay/Dialogue/DialogueData.cs`

- 脚本职责：完整对话数据资源。
- 属性：
  - `DialogueID`：对话 ID。
  - `StartingNodeID`：起始节点 ID，默认 `start`。
  - `DialogueNodes`：节点列表。
- 函数：
  - `GetNode(string nodeID)`：按节点 ID 查找节点。
- 关联：`DialogueManager` 运行对话时读取节点、台词和选项。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Dialogue/DialogueNode.cs`

- 脚本职责：对话节点数据。
- 属性：
  - `NodeID`：节点 ID。
  - `DialogueLines`：节点内台词列表。
  - `Choices`：节点结束后的选项列表。
- 关联：`DialogueManager.RunNode()` 顺序播放 `DialogueLines`，再按 `Choices` 进入选项模式。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Dialogue/DialogueLine.cs`

- 脚本职责：单句台词数据。
- 属性：
  - `LineID`：台词 ID。
  - `SpeakerName`：说话者。
  - `LineText`：台词文本。
  - `AutoAdvanceDelay`：自动推进延迟。
  - `WaitForInput`：是否等待输入推进。
  - `EventOnStart`：台词开始事件。
  - `EventOnEnd`：台词结束事件。
- 关联：`WorldDialogueView.ShowLine()` 显示它；`DialogueManager.ExecuteEvents()` 执行开始/结束事件。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Dialogue/DialogueChoice.cs`

- 脚本职责：对话选项数据。
- 属性：
  - `ChoiceID`：选项 ID。
  - `ChoiceText`：显示文本。
  - `NextNodeID`：确认后跳转节点。
  - `EventOnSelect`：确认选项后执行的事件。
- 关联：`WorldDialogueChoiceView` 显示它；`DialogueManager.HandleChoiceConfirmed()` 使用它跳转。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Dialogue/DialogueContext.cs`

- 脚本职责：对话事件执行上下文。
- 属性：
  - `EventBus`、`FlagCenter`、`QuestManager`：对话事件可使用的运行时服务。
  - `CurrentLine`、`CurrentNode`、`DialogueData`：当前对话位置。
  - `Sender`、`Instigator`：发送者与触发者。
- 函数：
  - `DialogueContext(...)`：构造不可变上下文。
- 关联：所有 `DialogueEventAction.Execute()` 都接收它。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Dialogue/DialogueEventAction.cs`

- 脚本职责：对话事件 ScriptableObject 抽象基类。
- 函数：
  - `Execute(DialogueContext context)`：由子类实现具体事件效果。
- 关联：`DialogueLine` 和 `DialogueChoice` 通过列表引用它。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Dialogue/DialogueManager.cs`

- 脚本职责：驱动对话流程、台词播放、选项选择、层切换和对话事件执行。
- 关键字段/属性：
  - `layerStack`：压入和弹出 `Dialogue`、`DialogueChoice` 层。
  - `eventBus`、`flagCenter`、`questManager`：提供给对话事件的服务。
  - `currentDialogue`、`currentNode`：当前对话数据和节点。
  - `currentDialogueView`、`currentChoiceView`：当前显示视图。
  - `currentSender`、`currentInstigator`：当前对话来源。
  - `dialogueCoroutine`：对话主协程。
  - `advanceRequested`：输入推进标记。
  - `IsRunning`：当前是否有对话运行。
- 函数：
  - `Awake()`：补齐层栈、事件总线、Flag 中心和 Quest 管理器引用。
  - `StartDialogue(...)`：启动新对话；若已有对话正在运行则先结束旧对话。
  - `HandleChoiceSelectedNavigate(Vector2 navigateInput)`：把 UI 导航交给选项视图。
  - `HandleChoiceConfirmed()`：读取选中选项、执行选项事件、退出选项层并跳转节点或结束对话。
  - `RequestAdvance()`：登记台词推进请求。
  - `RunDialogue()`：从起始节点开始运行完整对话。
  - `RunNode(DialogueNode node)`：逐句显示节点台词，处理等待输入和自动推进，之后进入选项或结束。
  - `EnterChoiceMode(DialogueNode node)`：显示选项并压入 `DialogueChoice` 层。
  - `ExitChoiceMode()`：隐藏选项并弹出 `DialogueChoice` 层。
  - `ExecuteEvents(IReadOnlyList<DialogueEventAction> events, DialogueLine line)`：构造 `DialogueContext` 并执行事件列表。
  - `EndDialogue()`：停止协程、隐藏视图、弹出对话层并清空当前状态。
  - `PushLayer(GameLayerType layerType)`：压入指定层，避免重复处理散落。
  - `PopDialogueLayer(GameLayerType layerType)`：弹出指定对话相关层。
- 关联：`NPCDialogueInteractable` 调用它；`InputRouter` 在对话层把交互、导航、确认输入交给它。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Dialogue/ConditionalDialogueEntry.cs`

- 脚本职责：一条带条件的对话入口。
- 属性：
  - `EntryID`：入口 ID。
  - `DialogueData`：满足条件后返回的对话数据。
  - `conditions`：必须全部满足的条件列表。
- 函数：
  - `IsMet(GameConditionContext context)`：逐个检查条件；没有条件时视为满足。
- 关联：`NPCDialogueProfile.SelectDialogue()` 按顺序评估入口。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Dialogue/NPCDialogueProfile.cs`

- 脚本职责：NPC 对话配置资源。
- 属性：
  - `ProfileID`：配置 ID。
  - `dialogueEntries`：按优先级排列的条件入口。
- 函数：
  - `SelectDialogue(GameConditionContext context)`：返回第一条满足条件的 `DialogueData`。
- 关联：`NPCDialogueInteractable` 交互时调用它选择对话。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Dialogue/NPCDialogueInteractable.cs`

- 脚本职责：让 NPC 成为世界交互对象，并在交互时启动对话。
- 关键字段：
  - `dialogueProfile`：NPC 对话配置。
  - `dialogueManager`：对话运行器。
  - `worldDialogueView`、`worldDialogueChoiceView`：显示视图。
  - `interactionPrompt`：交互提示文本。
  - `flagCenter`、`questManager`：条件评估服务。
- 函数：
  - `Awake()`：补齐对话管理器、Flag 中心、Quest 管理器和视图引用。
  - `InteractionTransform`：返回自身 Transform。
  - `CanInteract(InteractionContext context)`：当前有可选对话且对话管理器存在时可交互。
  - `GetInteractionPrompt(InteractionContext context)`：返回交互提示。
  - `Interact(InteractionContext context)`：选择对话数据并调用 `DialogueManager.StartDialogue()`。
  - `SelectDialogueData(InteractionContext interactionContext)`：创建 `GameConditionContext` 并从 Profile 中选对话。
- 关联：实现 `IInteractable`，被 `InteractionDetector` 发现。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Dialogue/WorldDialogueView.cs`

- 脚本职责：世界空间对话台词显示。
- 关键字段：
  - `root`：显示根对象。
  - `lineText`：TMP 文本组件。
- 函数：
  - `Awake()`：初始化隐藏。
  - `ShowLine(DialogueLine line)`：显示台词文本。
  - `Hide()`：隐藏视图。
  - `SetVisible(bool visible)`：统一切换根对象显隐。
- 关联：`DialogueManager.RunNode()` 每句台词调用它。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Dialogue/WorldDialogueChoiceView.cs`

- 脚本职责：世界空间对话选项显示和选择。
- 关键字段：
  - `root`：显示根对象。
  - `choiceSlots`：选项文本槽。
  - `navigationCooldown`：导航冷却，避免按住时过快跳选。
  - `currentChoices`：当前选项列表。
  - `selectedIndex`：当前选中索引。
  - `nextNavigationTime`：下次允许导航的时间。
- 函数：
  - `Awake()`：初始化隐藏。
  - `GetSelectedChoice()`：返回当前选中选项。
  - `ShowChoices(IReadOnlyList<DialogueChoice> choices)`：显示选项并选中第一项。
  - `MoveSelection(Vector2 navigateInput)`：根据上下输入移动选中项，受冷却限制。
  - `Hide()`：隐藏选项并清空状态。
  - `RefreshAllChoiceSlots()`：刷新全部选项槽。
  - `RefreshChoiceSlot(int index)`：刷新单个文本槽，包含选中显示。
  - `SetVisible(bool visible)`：统一切换根对象显隐。
- 关联：`DialogueManager.EnterChoiceMode()` 显示选项，`InputRouter` 通过 `DialogueManager` 间接驱动导航。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Dialogue/DebugDialogueStarter.cs`

- 脚本职责：调试用对话启动器。
- 关键字段：
  - `dialogueManager`、`dialogueData`、`dialogueView`、`choiceView`。
- 函数：
  - `Awake()`：补齐对话管理器和视图引用。
  - `StartDebugDialogue()`：通过 Context Menu 启动指定对话。
- 关联：用于不经过 NPC 交互直接验证对话数据。

### Runtime/GamePlay/Dialogue/Events

#### `Assets/_Game/Scripts/Runtime/GamePlay/Dialogue/Events/DebugLogDialogueEvent.cs`

- 脚本职责：对话事件调试日志。
- 字段：
  - `Message`：要输出的日志文本。
- 函数：
  - `Execute(DialogueContext context)`：输出日志。
- 关联：可挂在台词开始/结束或选项确认事件上。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Dialogue/Events/PublishSignalDialogueEvent.cs`

- 脚本职责：对话中发布通用信号。
- 字段：
  - `SignalID`：要发布的信号。
- 函数：
  - `Execute(DialogueContext context)`：通过 `context.EventBus` 发布 `GameSignalEvent`。
- 关联：用于让对话触发其它系统，但不直接引用具体系统。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Dialogue/Events/SetFlagDialogueEvent.cs`

- 脚本职责：对话中设置布尔 Flag。
- 字段：
  - `flagData`：目标 Flag。
  - `value`：目标值。
- 函数：
  - `Execute(DialogueContext context)`：通过 `context.FlagCenter.SetBool()` 改变 Flag。
- 关联：常用于对话选项后记录“已询问”“已接受”等剧情状态。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Dialogue/Events/SetQuestStateDialogueEvent.cs`

- 脚本职责：对话中设置 Quest 状态。
- 字段：
  - `questData`：目标 Quest。
  - `targetState`：目标状态。
- 函数：
  - `Execute(DialogueContext context)`：通过 `context.QuestManager.SetQuestState()` 改变 Quest。
- 关联：用于通过 NPC 对话开启、推进或完成任务。

### Runtime/GamePlay/Story

#### `Assets/_Game/Scripts/Runtime/GamePlay/Story/StoryContext.cs`

- 脚本职责：剧情步骤执行上下文。
- 属性：
  - `Runner`：当前序列运行器。
  - `LayerStack`：游戏层栈。
  - `FlagCenter`：Flag 中心。
  - `QuestManager`：Quest 管理器。
  - `SceneBindings`：当前场景剧情绑定表，用于把剧情资源中的 key 映射到场景对象或组件。
  - `Instigator`：触发剧情的对象。
- 函数：
  - `StoryContext(...)`：构造不可变上下文。
- 关联：`StorySequenceRunner` 创建它并传给每个 `StoryStepAction`。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Story/StoryStepAction.cs`

- 脚本职责：剧情步骤 ScriptableObject 基类，让剧情步骤可以作为资源复用和组合。
- 函数：
  - `Execute(StoryContext context)`：由子类实现具体步骤协程。
- 关联：`StorySequence` 按列表保存它，`StorySequenceRunner` 顺序执行；替代旧的 `StoryStepBehaviour` 场景组件基类。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Story/StoryBindingKey.cs`

- 脚本职责：剧情场景绑定 key 资源。
- 字段/属性：
  - `bindingID`：绑定 ID。
  - `description`：绑定用途说明。
  - `BindingID`、`Description`：只读访问属性。
- 关联：`StorySceneBindings` 使用它查找场景目标，剧情步骤通过 key 间接引用玩家、文本视图等对象。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Story/StorySceneBindings.cs`

- 脚本职责：维护剧情 key 到场景对象的映射。
- 关键字段：
  - `bindings`：Inspector 配置的 key/target 列表。
  - `targetByKey`：运行时查找字典。
- 函数：
  - `Awake()`：构建查找字典。
  - `TryGetGameObject()`、`GetGameObject()`：按 key 查找对象，缺失时可输出警告。
  - `TryGetComponent<T>()`、`GetComponent<T>()`：按 key 查找对象并取组件。
  - `BuildLookUp()`：跳过空 key、空 target 和重复 key，生成运行时字典。
- 关联：`StorySequenceRunner` 把它放进 `StoryContext`，`ShowStoryTextStepAction`、`HideStoryTextStepAction`、`SetPlayerMoveModeStoryStepAction`、`StopPlayerStoryStepAction`、`SwitchCameraStoryStepAction` 等步骤通过它访问场景对象。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Story/StoryCameraDirector.cs`

- 脚本职责：剧情镜头导演，集中控制 Cinemachine 虚拟相机的优先级切换。
- 关键字段/属性：
  - `activePriority`：目标镜头切换后的优先级。
  - `inactivePriority`：旧镜头降级后的优先级。
  - `lowerPreviousCameera`：切换新镜头时是否降低旧镜头优先级，字段名存在拼写 `Cameera`。
  - `currentCamera`、`CurrentCamera`：当前剧情镜头缓存和只读访问属性。
- 函数：
  - `SwitchTo(StorySceneBindings sceneBindings, StoryBindingKey targetCameraKey)`：通过场景绑定 key 查找 `CinemachineVirtualCameraBase`，必要时降低旧镜头优先级，再把目标镜头设为激活优先级。
- 关联：由 `SwitchCameraStoryStepAction` 调用；依赖 `StorySceneBindings` 中配置的镜头绑定 key。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Story/StoryTextView.cs`

- 脚本职责：剧情文本 UI 显示与隐藏。
- 关键字段：
  - `root`：可选的显示根对象。
  - `contentText`：TextMeshPro 文本组件。
- 函数：
  - `Awake()`：启动时隐藏文本。
  - `ShowText(string content)`：显示 root 或自身对象，并写入文本。
  - `Hide()`：清空文本并隐藏 root 或自身对象。
  - `SetVisible(bool visible)`：根据是否配置 `root` 切换显示状态。
- 关联：`ShowStoryTextStepAction` 和 `HideStoryTextStepAction` 控制它。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Story/StorySequence.cs`

- 脚本职责：剧情序列数据组件，保存一组步骤。
- 属性：
  - `SequenceID`：序列 ID。
  - `Steps`：`StoryStepAction` 步骤资源列表。
- 关联：`StoryTrigger` 引用它，`StorySequenceRunner` 运行它。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Story/StorySequenceRunner.cs`

- 脚本职责：运行剧情序列，保证同一时间只有一个序列协程执行。
- 关键字段/属性：
  - `layerStack`、`flagCenter`、`questManager`、`sceneBindings`：传入步骤上下文。
  - `currentSequenceCoroutine`：当前运行协程。
  - `IsRunning`：是否正在运行序列。
- 函数：
  - `Awake()`：补齐运行所需服务引用。
  - `TryRunSequence(StorySequence sequence, GameObject instigator)`：如果没有序列运行且参数有效，则启动序列。
  - `RunSequenceCoroutine(StorySequence sequence, GameObject instigator)`：构造上下文并顺序执行每个步骤。
- 关联：`StoryTrigger` 触发它；步骤可使用上下文访问 Flag、Quest、层栈和场景绑定对象。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Story/StoryTrigger.cs`

- 脚本职责：触发器式剧情入口。
- 关键字段：
  - `sequenceRunner`：运行器。
  - `storySequence`：要运行的序列。
  - `triggerOnce`：是否只触发一次。
  - `startConditions`：剧情启动前需要检查的条件列表。
  - `requireAllConditions`：为 true 时所有条件都满足才启动，为 false 时任一条件满足即可。
  - `flagCenter`、`questManager`：构造条件上下文所需的运行时服务。
  - `hasTriggered`：是否已经触发过。
- 函数：
  - `Awake()`：补齐序列运行器、Flag 中心和 Quest 管理器引用。
  - `OnTriggerEnter2D(Collider2D collision)`：玩家进入触发器时先检查启动条件，再尝试运行剧情。
  - `CanStart(GameObject instigator)`：根据条件列表和组合方式判断是否允许启动。
  - `AreAllConditionsMet()`、`IsAnyConditionMet()`：分别处理全满足和任一满足的条件组合。
- 关联：把物理触发区域、条件系统和 `StorySequenceRunner` 连接起来。

### Runtime/GamePlay/Story/Steps

#### `Assets/_Game/Scripts/Runtime/GamePlay/Story/Steps/DebugLogStoryStep.cs`

- 脚本职责：剧情步骤调试日志资源。
- 字段：
  - `message`：日志内容。
- 函数：
  - `Execute(StoryContext context)`：输出日志并立即结束。
- 关联：用于验证剧情步骤顺序。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Story/Steps/WaitSecondsStoryStepAction.cs`

- 脚本职责：剧情等待步骤资源。
- 字段：
  - `duration`：等待秒数。
- 函数：
  - `Execute(StoryContext context)`：当 `duration` 大于 0 时等待指定时间。
- 关联：作为 `StorySequence` 中的延时节点。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Story/Steps/WaitUntilGameConditionStoryStepAction.cs`

- 脚本职责：剧情条件等待步骤资源。
- 字段：
  - `conditions`：需要检查的条件列表。
  - `requireAllConditions`：控制全满足或任一满足。
- 函数：
  - `Execute(StoryContext context)`：每帧检查条件组，直到满足后继续序列。
  - `IsConditionGroupMet()`、`AreAllConditionsMet()`、`IsAnyConditionMet()`：组合条件判断。
- 关联：通过 `GameConditionContext` 读取 Flag、Quest、Runner 和触发者信息，可让剧情等待任务状态或 Flag 变化。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Story/Steps/ShowStoryTextStepAction.cs`

- 脚本职责：显示剧情文本步骤资源。
- 字段：
  - `textViewKey`：用于查找 `StoryTextView` 的场景绑定 key。
  - `content`：要显示的文本。
- 函数：
  - `Execute(StoryContext context)`：从 `StorySceneBindings` 取 `StoryTextView` 并调用 `ShowText()`。
- 关联：依赖 `StorySceneBindings`，用于把剧情提示显示到场景 UI。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Story/Steps/HideStoryTextStepAction.cs`

- 脚本职责：隐藏剧情文本步骤资源。
- 字段：
  - `textViewKey`：用于查找 `StoryTextView` 的场景绑定 key。
- 函数：
  - `Execute(StoryContext context)`：从 `StorySceneBindings` 取 `StoryTextView` 并调用 `Hide()`。
- 关联：通常与 `ShowStoryTextStepAction` 成对使用。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Story/Steps/SetPlayerMoveModeStoryStepAction.cs`

- 脚本职责：剧情中切换玩家移动模式的步骤资源。
- 字段：
  - `playerKey`：用于查找玩家对象的场景绑定 key。
  - `moveType`：目标移动模式，当前为 `Walk` 或 `Run`。
- 函数：
  - `Execute(StoryContext context)`：从 `StorySceneBindings` 取 `PlayerMovement` 并调用 `SetPlayerMoveMode()`。
- 关联：用于剧情中临时切换玩家步行/跑步速度。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Story/Steps/PushGameLayerStoryStepAction.cs`

- 脚本职责：剧情中压入指定游戏层的步骤资源，默认用于进入 `Cutscene` 层。
- 字段：
  - `layerType`：要压入的游戏层，默认 `Cutscene`。
- 函数：
  - `Execute(StoryContext context)`：从上下文读取 `GameLayerStack`，调用 `PushLayer(layerType)`。
- 关联：用于剧情开始时切换输入/控制层；通常和 `PopGameLayerStoryStepAction` 成对使用。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Story/Steps/PopGameLayerStoryStepAction.cs`

- 脚本职责：剧情中弹出指定游戏层的步骤资源，默认用于退出 `Cutscene` 层。
- 字段：
  - `layerType`：要弹出的游戏层，默认 `Cutscene`。
- 函数：
  - `Execute(StoryContext context)`：从上下文读取 `GameLayerStack`，调用 `PopLayer(layerType)`。
- 关联：用于剧情结束时恢复上一层输入/控制状态；依赖 `GameLayerStack` 的栈顶匹配保护。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Story/Steps/StopPlayerStoryStepAction.cs`

- 脚本职责：剧情中清空玩家移动输入并停止玩家刚体速度的步骤资源。
- 字段：
  - `playerkey`：用于查找玩家对象的场景绑定 key，字段名存在大小写不一致。
- 函数：
  - `Execute(StoryContext context)`：从 `StorySceneBindings` 取玩家对象，然后依次清空输入和停止速度。
  - `ClearPlayerInput(GameObject player)`：查找 `PlayerInputReceiver` 并调用 `ClearMoveInput()`。
  - `StopPlayerVelocity(GameObject player)`：查找 `PlayerMovement` 并调用 `SetRigibodyVelocity(Vector2.zero)`。
- 关联：常用于切入剧情或镜头演出前固定玩家位置；依赖玩家对象上同时存在 `PlayerInputReceiver` 和 `PlayerMovement`。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Story/Steps/SwitchCameraStoryStepAction.cs`

- 脚本职责：剧情中切换 Cinemachine 虚拟相机的步骤资源。
- 字段：
  - `cameraDirectorKey`：用于查找 `StoryCameraDirector` 的场景绑定 key。
  - `targetCameraKey`：用于查找目标虚拟相机的场景绑定 key。
  - `waitAfterSwitch`：切换成功后的等待秒数。
- 函数：
  - `Execute(StoryContext context)`：通过 `StorySceneBindings` 获取镜头导演并调用 `SwitchTo()`；切换成功且等待时间大于 0 时等待指定秒数。
- 关联：依赖 `StoryCameraDirector` 和 Cinemachine；用于在剧情序列中切到玩家镜头、老人镜头等场景镜头。

### Runtime/GamePlay/NPC

#### `Assets/_Game/Scripts/Runtime/GamePlay/NPC/Interaction.cs`

- 脚本职责：NPC 接近提示与朝向显示逻辑。
- 关键字段：
  - `npcName`：NPC 名称。
  - `speakerName`：显示名称的 TMP 文本。
  - `target`：检测到的目标。
  - `visualLayer`：NPC 视觉层。
  - `movement`：移动/朝向组件。
- 函数：
  - `Awake()`：初始化引用并隐藏提示。
  - `Update()`：有目标时根据目标相对位置更新朝向。
  - `OnTriggerEnter2D(Collider2D collision)`：玩家进入范围时记录目标并显示提示。
  - `OnTriggerExit2D(Collider2D collision)`：玩家离开时清空目标并隐藏提示。
  - `Show()`：显示 NPC 名称提示。
  - `Hide()`：隐藏提示。
- 关联：偏表现层，与 `NPCDialogueInteractable` 的实际对话交互可共同挂在 NPC 上。

### Runtime/GamePlay/Player

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerActionType.cs`

- 脚本职责：玩家动作位标记。
- 枚举值：
  - `None`：无动作。
  - `Move`：移动。
  - `Jump`：跳跃。
  - `Attack`：攻击。
  - `Dash`：冲刺。
  - `WorldInteract`：世界交互。
  - `UseItem`：使用物品。
- 关联：`GameLayerRuleDatabase` 用它配置锁定动作；`PlayerControlArbitration` 用它判断能否执行输入。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerMoveType.cs`

- 脚本职责：玩家移动模式枚举。
- 枚举值：
  - `Walk`：步行速度。
  - `Run`：跑步速度。
- 关联：`PlayerMovement.GetMoveVelocity()` 根据它返回不同速度。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerBaseConfig.cs`

- 脚本职责：玩家基础参数配置资源。
- 属性：
  - `RunVelocity`、`WalkVelocity`：跑步和步行速度。
  - `JumpForce`：跳跃施加速度。
  - `GravityScale`：重力缩放。
  - `WallSlideSlowSpeed`、`WallSlideFastSpeed`：贴墙时的普通滑落速度与按下方向时的快速滑落速度。
  - `CoastingDuration`：跑步结束滑行时间。
  - `CanEndRunEarlyDuration`：进入跑步后允许进入 RunEnd 的延迟窗口。
  - `RunBufferDuration`：跑步松手缓冲时间。
  - `ApexThreshold`：接近最高点时进入 Apex 的速度阈值。
- 关联：`PlayerMovement` 读取移动/跳跃参数；跑步和跳跃状态读取缓冲、滑行和 Apex 参数。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerAnimationTrigger.cs`

- 脚本职责：由动画事件驱动的状态标记组件。
- 属性：
  - `IsAnimationFinished`：当前动画是否结束。
  - `canPerformAction`：动画中是否允许执行动作。
- 函数：
  - `EndAnimation()`：动画事件调用，标记动画结束。
  - `StartAnimation()`：动画事件调用或状态进入时重置动画结束标记。
  - `EnableAction()`：允许动作。
  - `DisableAction()`：禁止动作。
- 关联：`Player_JumpStart`、跑步转身/结束等状态用它判断动画是否完成。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerInputReceiver.cs`

- 脚本职责：保存玩家输入缓存和一次性请求。
- 属性/字段：
  - `MoveInput`：当前移动输入。
  - `jumpPressed`、`attackPressed`、`dashPressed`、`worldInteractPressed`：一次性请求标记。
- 函数：
  - `SetMoveInput(Vector2 moveInput)`：写入移动输入。
  - `RequestJump()`：登记跳跃请求。
  - `RequestAttack()`：登记攻击请求。
  - `RequestDash()`：登记冲刺请求。
  - `RequestWorldInteract()`：登记世界交互请求。
  - `ClearMoveInput()`：清空移动输入。
  - `ConsumeJump()`：读取并清除跳跃请求。
  - `ConsumeAttack()`：读取并清除攻击请求。
  - `ConsumeDash()`：读取并清除冲刺请求。
  - `ConsumeWorldInteract()`：读取并清除世界交互请求。
- 关联：`InputRouter` 写入请求；玩家状态读取并消费请求。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerControlArbitration.cs`

- 脚本职责：根据当前游戏层和层规则判断玩家动作是否可执行。
- 属性：
  - `CanMove`、`CanJump`、`CanAttack`、`CanDash`、`CanWorldInteract`、`CanUseItem`。
- 函数：
  - `Awake()`：补齐层栈和规则数据库引用。
  - `CanDo(PlayerActionType actionType)`：遍历活动层，若任意层锁定该动作则返回 false。
  - `GetAllPlayerActions()`：组合所有玩家动作位标记。
- 关联：`InputRouter` 在转发输入前调用它；规则来自 `GameLayerRuleDatabase`。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerMovement.cs`

- 脚本职责：玩家移动和跳跃实现。
- 关键字段/属性：
  - `player`：玩家主体。
  - `runVelocity`、`walkVelocity`、`jumpForce`：从配置读取的参数。
  - `playerMoveType`：当前移动模式。
- 函数：
  - `Awake()`：调用基类并补齐玩家引用。
  - `Start()`：读取 `PlayerBaseConfig` 初始化移动参数。
  - `GetMoveVelocity()`：按 `playerMoveType` 返回步行或跑步速度。
  - `HandleJump()`：设置跳跃速度。
  - `HandleMoveAndFlip(Vector2 inputMove)`：移动并根据输入翻转。
  - `HandleMove(Vector2 inputMove)`：按当前速度设置水平速度，保留当前 Y 速度。
  - `SetPlayerMoveMode(PlayerMoveType playerMoveType)`：切换步行/跑步模式。
  - `InitializePlayerMovement(PlayerBaseConfig config)`：从配置读取速度、跳跃力等参数。
- 关联：所有玩家地面/空中状态都通过它移动角色。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/Player.cs`

- 脚本职责：玩家实体入口，创建和驱动所有玩家状态。
- 关键字段/属性：
  - `playerBaseConfig`：玩家参数。
  - `playerMovement`、`playerInputReceiver`、`playerAnimationTrigger`、`interaction`、`timeTool`、`groundSensor`、`wallSensor`：玩家子系统引用。
  - `idleState`、`walkState`、`runState`、`runTurnState`、`runEndState`、`jumpStartState`、`jumpUpState`、`apexState`、`fallState`、`wallSlideState`：所有玩家状态实例。
- 函数：
  - `Awake()`：调用 `Entity.Awake()` 创建状态机，并实例化所有状态。
  - `Start()`：初始化默认状态。
  - `Update()`：驱动状态机逻辑更新。
  - `FixedUpdate()`：驱动状态机物理更新。
- 关联：继承 `Entity`；把配置、输入、移动、交互、地面检测、动画触发器整合给各个 `PlayerState`。

### Runtime/GamePlay/Player/PlayerState

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/PlayerAnimationHash.cs`

- 脚本职责：集中保存 Animator 参数 hash。
- 字段：
  - `Idle`、`Run`、`RunTurn`、`RunEnd`、`Walk`、`JumpStart`、`JumpUp`、`Apex`、`Fall`、`BaseLand`、`RollingLand`、`wallSlide`。
- 关联：`Player` 创建状态时传入对应 hash，`EntityState.Enter/Exit()` 控制 Animator bool。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/PlayerState.cs`

- 脚本职责：玩家状态基类。
- 关键字段：
  - `player`：玩家主体。
  - `movement`：玩家移动组件。
  - `input`：输入缓存。
  - `groundSensor`：地面检测。
  - `wallSensor`：墙面检测。
  - `animationTrigger`：动画事件状态。
- 函数：
  - `PlayerState(...)`：缓存玩家相关子系统。
  - `LogicalUpdate()`：统一刷新 `GroundSensor` 的落地状态和 `WallSensor` 的贴墙状态。
  - `ChangeStateToMoveState()`：根据移动输入和当前 `PlayerMoveType` 切换到待机、步行或跑步。
- 关联：所有玩家具体状态继承它，避免重复拿组件。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/PlayerGround.cs`

- 脚本职责：地面状态公共逻辑。
- 函数：
  - `PlayerGround(...)`：调用玩家状态基类构造。
  - `Enter()`：进入地面状态时消费遗留的跳跃请求，避免旧输入在落地后再次触发。
  - `LogicalUpdate()`：处理跳跃、世界交互；确认仍在地面时才执行移动/待机/跑步转换。
- 关联：`Player_IdleState`、`Player_WalkState`、跑步相关状态继承或间接使用地面逻辑。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/PlayerAir.cs`

- 脚本职责：空中状态公共逻辑。
- 函数：
  - `PlayerAir(...)`：调用玩家状态基类构造。
  - `LogicalUpdate()`：落地后切回移动状态。
- 关联：`Player_JumpUp`、`Player_Apex`、`Player_Fall`、`Player_WallSlide` 继承它。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/Player_IdleState.cs`

- 脚本职责：玩家待机状态。
- 函数：
  - `Player_IdleState(...)`：调用地面状态构造。
  - `Enter()`：进入待机时把移动模式设为 `Walk`，并执行基类进入动画。
- 关联：`PlayerState.ChangeStateToMoveState()` 在没有输入时回到它。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/Player_WalkState.cs`

- 脚本职责：玩家步行状态。
- 函数：
  - `Player_WalkState(...)`：调用地面状态构造。
  - `PhysicalUpdate()`：按当前输入执行步行移动和翻转。
- 关联：`PlayerState.ChangeStateToMoveState()` 在移动模式为 `Walk` 且有水平输入时进入它；`SetPlayerMoveModeStoryStepAction` 可通过剧情切换移动模式。

### Runtime/GamePlay/Player/PlayerState/Player_Run

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/Player_Run/Player_RunTransition.cs`

- 脚本职责：跑步相关过渡状态基类。
- 函数：
  - `Player_RunTransition(...)`：调用玩家状态构造。
  - `LogicalUpdate()`：若落地则处理跑步方向、转身、结束等逻辑；若离地且竖直速度向下则进入下落。
  - `IsSameDirection()`：判断输入方向和当前朝向是否一致。
- 关联：`Player_RunState`、`Player_RunTurnState`、`Player_RunEndState` 继承它。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/Player_Run/Player_RunState.cs`

- 脚本职责：持续跑步状态。
- 关键字段：
  - `isFirstTimeRelese`：记录是否第一次松开方向输入，变量名存在拼写 `Relese`。
  - `RunBufferCoroutineID`：松手缓冲计时 ID。
  - `RunEndEarlyCoroutineID`：过早结束窗口计时 ID。
- 函数：
  - `Player_RunState(...)`：调用跑步过渡状态构造。
  - `Enter()`：设置跑步移动模式，开启过早结束窗口，重置松手记录。
  - `LogicalUpdate()`：处理跳跃、切回步行模式、松手缓冲、跑步结束、反向转身等状态转换。
  - `PhysicalUpdate()`：根据输入或缓冲窗口继续移动。
- 关联：依赖 `TimeTool` 判断窗口是否存在；跳跃时进入 `Player_JumpStart`。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/Player_Run/Player_RunTurnState.cs`

- 脚本职责：跑步反向转身状态。
- 函数：
  - `Player_RunTurnState(...)`：调用跑步过渡状态构造。
  - `LogicalUpdate()`：动画结束后回到跑步状态，期间也保留跑步过渡公共判断。
  - `Exit()`：退出时根据输入方向执行翻转。
- 关联：`Player_RunTransition` 检测到反向输入后进入它。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/Player_Run/Player_RunEndState.cs`

- 脚本职责：跑步结束刹车滑行状态。
- 关键字段：
  - `coastingVelocity`：进入状态时记录的滑行速度。
  - `coastingTimer`：滑行计时。
- 函数：
  - `Player_RunEndState(...)`：调用跑步过渡状态构造。
  - `Enter()`：记录当前速度和滑行持续时间。
  - `LogicalUpdate()`：处理滑行、动画结束、重新输入后的状态转换。
  - `HandleCoasting()`：按剩余时间逐渐降低水平速度。
- 关联：`Player_RunState` 在允许结束跑步时进入它。

### Runtime/GamePlay/Player/PlayerState/Player_Jump

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/Player_Jump/Player_JumpStart.cs`

- 脚本职责：跳跃起跳动画状态。
- 关键字段：
  - `hadJumped`：确保跳跃力只施加一次。
- 函数：
  - `Player_JumpStart(...)`：调用玩家状态构造。
  - `Enter()`：重置动画触发器和跳跃施加标记。
  - `LogicalUpdate()`：在动画允许动作后调用 `movement.HandleJump()`；动画结束后进入 `Player_JumpUp`。
- 关联：地面状态或跑步状态消费跳跃输入后进入它。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/Player_Jump/Player_JumpUp.cs`

- 脚本职责：向上跳跃状态。
- 函数：
  - `Player_JumpUp(...)`：调用空中状态构造。
  - `LogicalUpdate()`：执行空中公共逻辑；当竖直速度低于 Apex 阈值时进入 `Player_Apex`，速度转负后进入 `Player_Fall`。
- 关联：从 `Player_JumpStart` 进入，之后连接 Apex 或 Fall。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/Player_Apex.cs`

- 脚本职责：跳跃最高点状态。
- 函数：
  - `Player_Apex(...)`：调用空中状态构造。
  - `LogicalUpdate()`：执行空中公共逻辑；当角色开始下落时进入 `Player_Fall`。
- 关联：让跳跃最高点有独立动画状态。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/Player_Fall.cs`

- 脚本职责：下落状态。
- 函数：
  - `Player_Fall(...)`：调用空中状态构造。
  - `LogicalUpdate()`：执行空中公共逻辑；接触朝向一侧的墙面时进入 `Player_WallSlide`。
  - `PhysicalUpdate()`：处理空中水平移动。
- 关联：从离地、跳跃上升结束或 Apex 进入；落地后通过 `PlayerAir.LogicalUpdate()` 回到地面移动状态。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/Player_WallSlide.cs`

- 脚本职责：控制玩家贴墙时的竖直滑落速度。
- 关键字段：
  - `originalGravity`：进入贴墙状态前的重力缩放，用于退出时恢复。
- 函数：
  - `Enter()`：保存并临时关闭刚体重力，把速度设为配置中的普通滑落速度。
  - `LogicalUpdate()`：向下输入时使用快速滑落速度，否则保持普通滑落速度；落地转换由 `PlayerAir` 处理。
  - `Exit()`：恢复进入状态前的重力缩放。
- 关联：`Player_Fall` 检测到墙面后进入；当前尚未实现离开墙面时返回 `Player_Fall` 的转换。

## 8. 脚本之间的主要关联流程

### 启动与场景流程

1. `GameBootstrap` 在 BootScene 中启动，确保 `SceneLoader` 和 `GameLayerStack` 存在。
2. `GameBootstrap.Start()` 把层重置到合适初始状态，并通过 `SceneLoader.LoadMainMenu()` 进入主菜单。
3. `MainMenuController` 注册按钮事件，点击新游戏或继续游戏时调用 `SceneLoader.LoadGameScene()`。
4. `SceneNames` 统一提供场景名，避免场景加载字符串散落在多个脚本。

### 输入到玩家动作流程

1. Unity Input System 触发 `GameInputReader` 中的 Action 回调。
2. `GameInputReader` 更新 `MoveInput` 或触发按键事件。
3. `InputRouter` 根据 `GameLayerStack.CurrentLayer` 判断当前层。
4. `InputRouter` 再通过 `PlayerControlArbitration` 判断当前层规则是否锁定该动作。
5. 可执行的输入写入 `PlayerInputReceiver`。
6. 玩家状态在 `LogicalUpdate()` 中消费输入，例如 `PlayerGround` 消费跳跃和世界交互，`Player_RunState` 消费跳跃并处理跑步输入。

### 玩家状态机流程

1. `Player` 继承 `Entity`，在 `Awake()` 中创建状态实例。
2. `StateMachine.InitializeState()` 进入默认状态。
3. `Player.Update()` 调用 `StateMachine.LogicalUpdate()`，处理输入、动画完成、状态转换。
4. `Player.FixedUpdate()` 调用 `StateMachine.PhysicalUpdate()`，处理刚体移动。
5. `EntityState.Enter()` 和 `Exit()` 用 `PlayerAnimationHash` 控制 Animator bool。
6. `PlayerMovement` 负责速度和翻转，`GroundSensor` 负责落地检测，`PlayerAnimationTrigger` 负责动画事件回传。

### 世界交互到对话流程

1. 玩家按交互键，`InputRouter.OnInteractPressed()` 写入 `PlayerInputReceiver.RequestWorldInteract()`。
2. `PlayerGround.LogicalUpdate()` 消费世界交互请求。
3. `PlayerGround` 调用 `Player.interaction.TryInteract()`。
4. `InteractionDetector` 从触发器范围内的 `IInteractable` 中选最近目标。
5. `NPCDialogueInteractable.CanInteract()` 判断是否存在可用对话。
6. `NPCDialogueInteractable.Interact()` 创建 `GameConditionContext`，通过 `NPCDialogueProfile.SelectDialogue()` 选出 `DialogueData`。
7. `DialogueManager.StartDialogue()` 压入 `Dialogue` 层并显示台词。
8. 对话进入选项时压入 `DialogueChoice` 层，输入路由转向选项导航和确认。

### 对话、Flag、Quest 与事件流程

1. `DialogueManager.RunNode()` 按顺序播放 `DialogueLine`。
2. 每句台词开始和结束时，`DialogueManager.ExecuteEvents()` 构造 `DialogueContext`。
3. `DebugLogDialogueEvent` 输出日志，`PublishSignalDialogueEvent` 发布 `GameSignalEvent`。
4. `SetFlagDialogueEvent` 调用 `GameFlagCenter.SetBool()`，可能发布 `GameFlagChangedEvent`。
5. `SetQuestStateDialogueEvent` 调用 `QuestManager.SetQuestState()`，可能发布 `QuestStateChangedEvent`。
6. 下一次 NPC 交互时，`FlagBoolCondition` 和 `QuestStateCondition` 会读取新的运行时状态，从而选择不同对话入口。

### 剧情序列流程

1. 物体进入 `StoryTrigger` 的 2D 触发器。
2. `StoryTrigger` 调用 `StorySequenceRunner.TryRunSequence()`。
3. `StoryTrigger` 根据 `GameCondition` 列表判断是否满足启动条件。
4. `StorySequenceRunner` 创建 `StoryContext`，其中包含 Flag、Quest、层栈、场景绑定和触发者。
5. `StorySequenceRunner` 顺序执行 `StorySequence.Steps` 中每个 `StoryStepAction.Execute()`。
6. 剧情开始或结束可通过 `PushGameLayerStoryStepAction`、`PopGameLayerStoryStepAction` 压入/弹出 `Cutscene` 等层，影响输入路由。
7. 剧情可通过 `StopPlayerStoryStepAction` 清空玩家移动输入并停止刚体速度。
8. 剧情可通过 `SwitchCameraStoryStepAction` 调用 `StoryCameraDirector`，按绑定 key 切换 Cinemachine 虚拟相机。
9. 当前已有调试日志、等待秒数、等待条件、显示/隐藏剧情文本、设置玩家移动模式、停止玩家、游戏层压栈/弹栈和切换剧情镜头等步骤资源。

## 9. 对话、条件、Flag 与 Quest 资源流程

当前对话流程以 `NPCDialogueProfile` 为 NPC 入口配置，以 `DialogueData` 为实际对话数据源，以 `DialogueManager` 为运行时驱动：

1. 玩家在 `Gameplay` 层按交互键。
2. `InputRouter` 将交互请求发送给 `PlayerInputReceiver`。
3. `PlayerGround` 消费请求，并调用 `InteractionDetector.TryInteract()`。
4. `NPCDialogueInteractable` 检查是否可交互，并通过 `NPCDialogueProfile.SelectDialogue()` 选择对话。
5. `NPCDialogueProfile` 按列表顺序检查 `ConditionalDialogueEntry`，第一条满足全部条件的入口会返回对应 `DialogueData`。
6. 条件检查可使用 `FlagBoolCondition` 和 `QuestStateCondition`。
7. `DialogueManager` 压入 `Dialogue` 层，逐句显示台词。
8. 台词结束后如果节点有选项，显示 `WorldDialogueChoiceView` 并压入 `DialogueChoice` 层。
9. 选项确认后跳转到下一个节点，并执行选项事件。
10. 对话事件可通过 `SetFlagDialogueEvent` 改变 Flag，或通过 `SetQuestStateDialogueEvent` 改变 Quest 状态。
11. 对话结束时隐藏 UI，并弹出对话相关层。

当前对话相关资源：

- `Assets/_Game/Data/Dialogue/OldManDialogueProfile.asset`
- `Assets/_Game/Data/Dialogue/DialogueData/OldManIntroDialogueInVillageAfterBeingSaved.asset`
- `Assets/_Game/Data/Dialogue/DialogueData/OldManExplainMossAreaDialogue.asset`
- `Assets/_Game/Data/Dialogue/DialogueData/FallBackDialogueAboutMossArea.asset`
- `Assets/_Game/Data/Dialogue/Events/Condition/Flag/Condition_NotAskedOldManAboutMossArea.asset`
- `Assets/_Game/Data/Dialogue/Events/Condition/Flag/Condition_AfterAskedOldManAboutMossArea.asset`
- `Assets/_Game/Data/Dialogue/Events/Condition/Flag/Condition_NotAcceptedHelpOldMan.asset`
- `Assets/_Game/Data/Dialogue/Events/Condition/Flag/Condition_AfterAcceptedHelpOldMan.asset`
- `Assets/_Game/Data/Dialogue/Events/Condition/Quest/Condition_HelpOldMan_NotStarted.asset`
- `Assets/_Game/Data/Dialogue/Events/Condition/Quest/Condition_HelpOldMan_InProgress.asset`
- `Assets/_Game/Data/Dialogue/Events/SetFlag/SetFlag_AskedOldManAboutMossArea.asset`
- `Assets/_Game/Data/Dialogue/Events/SetFlag/SetFlag_AcceptedHelpOldMan.asset`
- `Assets/_Game/Data/Dialogue/Events/SetQuest/SetQuest_HelpOldMan_InProgress.asset`
- `Assets/_Game/Data/Dialogue/Events/DebugEvent_OldManLineStart.asset`
- `Assets/_Game/Data/Dialogue/Events/Signal_OldManMentionsStoneGate.asset`
- `Assets/_Game/Data/Flags/DefaultGameFlagDatabase.asset`
- `Assets/_Game/Data/Flags/AskedOldManAboutMossArea.asset`
- `Assets/_Game/Data/Flags/AccptedHelpOldMan.asset`
- `Assets/_Game/Data/Quests/DefaultQuestDatabase.asset`
- `Assets/_Game/Data/Quests/HelpOldManQuest.asset`

当前剧情相关资源：

- `Assets/_Game/Data/Story/BindingKeys/BindingKey_Player.asset`
- `Assets/_Game/Data/Story/BindingKeys/BindingKey_StoryText_RightPrompt.asset`
- `Assets/_Game/Data/Story/BindingKeys/BindingKey_StoryCameraDirector.asset`
- `Assets/_Game/Data/Story/BindingKeys/BindingKey_Camera_Player.asset`
- `Assets/_Game/Data/Story/BindingKeys/BindingKey_Camera_OldMan.asset`
- `Assets/_Game/Data/Story/Steps/Debug.asset`
- `Assets/_Game/Data/Story/Steps/Step_Show_HelpCry.asset`
- `Assets/_Game/Data/Story/Steps/Step_Hide_StoryText.asset`
- `Assets/_Game/Data/Story/Steps/Step_SetMoveMode_Walk.asset`
- `Assets/_Game/Data/Story/Steps/Step_SetMoveMode_Run.asset`
- `Assets/_Game/Data/Story/Steps/Step_Wait_HelpOldMan_InProgress.asset`
- `Assets/_Game/Data/Story/Steps/PushGameLayerStoryStep.asset`
- `Assets/_Game/Data/Story/Steps/PopGameLayerStoryStep.asset`
- `Assets/_Game/Data/Story/Steps/StopPlayerStoryStep.asset`
- `Assets/_Game/Data/Story/Steps/Camera/Step_SwitchCamera_Player.asset`
- `Assets/_Game/Data/Story/Steps/Camera/Step_SwitchCamera_OldMan.asset`
- `Assets/_Game/Data/Story/Steps/Wait/Step_Wait_1s.asset`
- `Assets/_Game/Data/Story/Steps/Wait/Step_Wait_4s.asset`

## 10. 玩家跑步与跳跃状态设计

当前已经移除旧的 `Player_JumpState.cs`，跳跃拆分为 `JumpStart`、`JumpUp`、`Apex`、`Fall`。跑步逻辑集中在 `Player_Run` 子目录，步行状态由 `Player_WalkState` 和 `Walk` 动画承接。

跑步相关动画状态：

- `Walk`：步行移动。
- `Run`：持续跑步。
- `RunTurn`：跑步中反向转身。
- `RunEnd`：跑步结束后的刹车滑行。

跳跃相关动画状态：

- `JumpStart`：起跳准备/起跳动作，等待动画事件允许施加跳跃。
- `JumpUp`：向上运动。
- `Apex`：最高点附近。
- `Fall`：下落。
- `BaseLand`、`RollingLand`：落地动画 hash 已准备，具体落地状态脚本尚未接入。

当前 `Player_RunState` 使用两个计时窗口：

- `RunEndEarly`：进入 `Run` 后开启。窗口存在时，松手会被视为过早松手；窗口结束后松手可进入 `RunEnd`。
- `RunBuffer`：第一次松手时开启。窗口存在时，角色继续按当前朝向移动，用于避免点按方向键导致动画抖动。

建议参数起点：

| 参数 | 当前用途 | 建议范围 |
| --- | --- | --- |
| `CanEndRunEarlyDuration` | 控制进入 `Run` 后多久才允许播放 `RunEnd` | `0.18` - `0.35` |
| `RunBufferDuration` | 松手缓冲，过滤点按导致的抖动 | `0.08` - `0.15` |
| `CoastingDuration` | `RunEnd` 滑行时长 | `0.12` - `0.22` |
| `ApexThreshold` | 进入最高点状态的竖直速度阈值 | `0.05` - `0.5` |

## 11. 当前资源更新

- 对话事件目录已整理为 `Condition/Flag`、`Condition/Quest`、`SetFlag` 和 `SetQuest` 子目录。
- 旧的手填字符串 Flag 方式已改为 `GameFlagData` 资源引用。
- 新增 Quest 数据目录 `Assets/_Game/Data/Quests/`。
- 新增 `DefaultQuestDatabase.asset` 与 `HelpOldManQuest.asset`。
- 新增 `QuestManager`、`QuestData`、`QuestDatabase`、`QuestState`、`QuestStateCondition`、`QuestStateChangedEvent` 和 `SetQuestStateDialogueEvent`。
- 新增剧情序列脚本目录 `Assets/_Game/Scripts/Runtime/GamePlay/Story/`。
- 剧情步骤已由 `StoryStepBehaviour` 场景组件改为 `StoryStepAction` ScriptableObject 资源。
- 新增 `StoryBindingKey`、`StorySceneBindings`、`StoryCameraDirector` 和 `StoryTextView`，剧情步骤可通过绑定 key 控制场景对象、剧情镜头和剧情文本 UI。
- 新增剧情资源目录 `Assets/_Game/Data/Story/`，包含 `BindingKeys/` 和 `Steps/` 资源。
- 新增剧情步骤资源：显示/隐藏剧情文本、等待 1 秒/4 秒、等待老人帮助任务进入进行中、切换玩家 Walk/Run 移动模式、压入/弹出游戏层、停止玩家、切换玩家/老人剧情镜头和调试日志。
- 新增基础移动组件 `Movement`，玩家移动组件继承它。
- 新增玩家 `Walk` 动画状态与 `Player_WalkState` 实际移动逻辑，`PlayerBaseConfig.asset` 中 `walkVelocity` 当前为 4。
- 玩家跳跃状态拆分为 `Player_JumpStart`、`Player_JumpUp`、`Player_Apex`、`Player_Fall`。
- 玩家下落状态已接入 `WallSensor` 与初版 `Player_WallSlide`：接触墙面后按配置速度滑落，向下输入可加速，并新增 `Player_WallSlide.anim` 与 Animator 状态。
- 跑步过渡状态移动到 `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/Player_Run/`。
- `GameScene.unity` 已加入 `StorySceneBindings`、`StoryCameraDirector`、`StoryCanvas`、剧情文本视图、剧情相机绑定和 Input System UI EventSystem。
- `GameScene.unity` 已加入 `HPBar` 血条 UI，挂载 `UI_HPBarView` 并绑定 `FillClip` 裁剪节点。
- `GameScene.unity` 已加入 `Canvas_Inventory` 背包界面雏形，当前包含滚动视图和多个 Slot 图像节点。
- 新增 UI 图片资源目录 `Assets/_Game/Art/UI/`，包含 `04.png`、`06.png`、`07.png` 及对应 `.meta`。
- 背包系统脚本目录 `Assets/_Game/Scripts/Runtime/Systems/InventorySystem/` 已扩展为 `ItemData`、`ItemCategory`、`InventoryItem` 和 `InventoryGrid`。
- `ItemData` 已加入物品 ID、显示名、描述、图标、分类、占格尺寸、最大堆叠和旋转配置，并提供 `Game/Inventory/Item Data` 资源创建入口。
- 新增物品资源目录 `Assets/_Game/Data/Items/`，包含第一个物品 `GreenHerb.asset`（1×1，Consumable，最大堆叠 3）。
- 背包系统已拆分为独立程序集 `FirstGame.Inventory`（`FirstGame.Inventory.asmdef`），其余脚本仍在 `Assembly-CSharp`。
- 新增测试目录 `Assets/_Game/Tests/EditMode/`，包含 `FirstGame.Inventory.Tests.asmdef` 与 `InventoryGridTests.cs`，这是项目第一批自动化测试。
- `InventoryGrid` 新增 `IsInside` 矩形重载、`IsAreaEmpty`、`Place`、`GetItemAt`。
- 新增 `PlayerInventory`，挂在 `GameScene` 的 `Player` 对象上，负责在运行时创建 `InventoryGrid`（当前 4 × 8）。
- 老人对话资源已调整为“村庄救下后介绍”“苔藓区域说明”“苔藓区域兜底”三组数据。
- `BootScene.unity` 已接入 Quest 管理相关运行时对象引用。
- `InputRouter` 删除了确认对话选项时的调试日志输出。
- 背包详情由右键弹出菜单改为常驻面板：新增 `Assets/_Game/Scripts/Runtime/UI/ItemDetailPanel.cs`，删除 `ItemContextMenu.cs` 与 `ContextMenuBlocker.cs`；`GameScene.unity` 中删除 `Blocker` 与 `ItemContextMenu` 两个对象，新增 `InventoryPart`（含 `InventoryBG` / `Inventory` / `ItemDetailPanel`）与 `ItemLayer/SelectedBoard` 选中框。
- `InventoryGrid` 新增 `TryGetItemPosition`，`PlayerInventory` 同步新增转发方法。
- `Assets/_Game/Art/UI/` 新增背包与面板用图：`Slot.png`（32 个格子底图共用）、`Inventory Slots and Selection Frame Set.png`（选中框来源）以及四张 `ChatGPT Image Aug 30, 2026, *.png` 面板底板，均连同 `.meta` 一起提交。同目录的 `Icy Fantasy RPG UI Asset Sheet.png` 是尚未被任何场景引用的素材原图，暂未纳入版本库。

## 12. 已处理事项与仍需留意

已处理：

- `GameFlagEntry` 已迁移为 `GameFlagData`。
- `FlagBoolCondition` 与 `SetFlagDialogueEvent` 已通过 `GameFlagData` 获取 Flag ID。
- `DialogueContext` 和 `GameConditionContext` 已加入 `QuestManager`。
- `DialogueManager` 和 `NPCDialogueInteractable` 会在缺省时查找 `QuestManager`。
- 对话条件现在同时支持 Flag 条件和 Quest 状态条件。
- 对话事件现在支持设置 Quest 状态。
- 玩家跳跃拆分为空中多阶段状态，便于匹配动画。
- 玩家步行状态已接入状态机、动画和物理移动。
- 剧情序列系统已具备条件触发、顺序执行、等待、等待条件、日志、剧情文本显示和玩家移动模式切换步骤。

仍需留意：

- `FlagBoolCondition` 和 `SetFlagDialogueEvent` 中的 `flagData` 需要在资源里正确赋值，避免空引用。
- `QuestManager` 只能设置已注册在 `QuestDatabase` 中的 Quest。
- `InteractionContext.IneractorTransform` 当前属性名存在拼写问题；若改名会影响引用处，需要统一重构。
- `Player_RunState` 中 `isFirstTimeRelese` 存在拼写问题；若改名需要同步所有引用。
- `Movement.SetRigibodyVelocity` 方法名存在拼写问题；若改名需要同步所有调用。
- `Player_WallSlide` 当前只处理落地退出，尚未处理离开墙面后回到 `Player_Fall`，因此这是可继续开发的中间检查点，不是完整墙滑行为。
- `QuestData` 中 `descrition` 字段存在拼写问题；因为是序列化字段，改名前应考虑 `[FormerlySerializedAs]`。
- `StoryCameraDirector` 中 `lowerPreviousCameera` 字段存在拼写问题；因为是序列化字段，改名前应考虑 `[FormerlySerializedAs]`。
- `StopPlayerStoryStepAction` 中 `playerkey` 字段命名大小写不一致；因为是序列化字段，改名前应考虑 `[FormerlySerializedAs]`。
- `HideStoryTextStepAction` 和 `ShowStoryTextStepAction` 中缺失 `StorySceneBindings` 时的警告文本仍写成了设置玩家移动模式，后续可统一改文案。
- `UI_HPBarView` 中 `fullWidth` 当前硬编码为 `600f`，如果血条尺寸改为响应式或换图，需要改为从 `fillClip` 初始宽度或配置读取。
- `InventoryGrid` 已实现边界判断、空区域判断、放置和取格；仍缺移除、堆叠合并、旋转与网格联动、查找空位。
- `InventoryItem` 目前只支持初始化校验和旋转，后续需要根据交互需求补充数量变化、拆分堆叠或使用消耗逻辑。
- `InventoryItem.Rotate()` 是 public 的，物品已放入 `InventoryGrid` 之后再调用它，会让 `CurrentWidth`/`CurrentHeight` 与 `cells` 中的实际占用不一致。实现旋转交互前必须先决定旋转的归属（大概率要改由 `InventoryGrid` 提供 `TryRotate`）。
- `PlayerInventory.Awake()` 中放置 `debugItem` 和调用 `PrintGrid()` 属于临时调试代码，拾取流程接通后应移除。
- `ItemData.itemId` 是手填字符串，没有唯一性校验。它将是存档系统的主键，实现存档前需要加编辑器校验或改为资源引用。
- `InputRouter.OnDisable()` 在 `inputReader` 为 null 时会抛 `NullReferenceException`（`OnEnable()` 有 null 判断并提前返回，`OnDisable()` 没有）。直接从 `GameScene` 开始 Play、跳过 `BootScene` 时会复现。
- 当前项目没有在命令行中接入 Unity 编译流程，脚本改动后建议在 Unity Editor 中观察 Console。
- 纯逻辑代码（当前为 `InventorySystem`）已接入 EditMode 单元测试，改动后应在 Test Runner 中 Run All 确认。注意：**Play 模式表现正常不等于逻辑正确**，两种验证都要做。

## 13. 更新规则

以后更新本文档时必须按以下顺序执行，并保持“每个脚本、每个函数、脚本关联”三个层级都同步：

1. 重新扫描 `Assets/_Game/` 的目录和文件。
2. 重新读取 `Assets/_Game/Scripts/**/*.cs`，包括新增、移动、删除的脚本。
3. 对每个脚本更新以下内容：脚本职责、关键字段/属性、所有函数职责、与其它脚本或资源的关联。
4. 对每个新增函数、删除函数、改名函数、职责变化函数，都必须在本文件对应脚本条目中同步说明。
5. 若脚本之间调用关系变化，必须同步更新“脚本之间的主要关联流程”。
6. 若状态机、输入流、交互流、对话流、Quest/Flag 流程变化，必须同步更新对应流程说明。
7. 检查 `ProjectSettings/EditorBuildSettings.asset` 的启用场景是否变化。
8. 检查 `Assets/Settings/InputSystem_Actions.inputactions` 的 Action Map、Action 和 Binding 是否变化。
9. 检查 `Assets/_Game/Data/Core/GameLayerRuleDatabase.asset` 的层规则是否变化。
10. 检查对话、Flag、Quest、Condition、Story 相关 ScriptableObject 是否新增、移动或删除。
11. 检查 `Packages/manifest.json` 是否新增或移除关键依赖。
12. 检查 `.gitignore` 和 `.gitattributes` 是否符合 Unity 提交规范。
13. 提交前使用 `git status --short` 确认只提交本次应提交的文件；如果工作区有用户未提交改动，不要把无关文件加入提交。
