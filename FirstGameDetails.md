# First Game Project Details

本文档用于记录当前 Unity 项目的结构、核心系统、资源变化、脚本职责、函数职责、脚本之间的关系和 Git 提交规则。项目结构、脚本职责、函数、输入绑定、场景、资源或 ScriptableObject 发生变化后，应同步更新本文档。

最后更新时间：2026-05-26。

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
  - 世界交互：`IInteractable` + `InteractionContext` + `InteractionDetector`。
  - 对话系统：`DialogueManager` + `DialogueData` + `NPCDialogueProfile` + `ConditionalDialogueEntry` + `WorldDialogueView` + `WorldDialogueChoiceView`。
  - 事件总线：`GameEventBus` + `IGameEvent` + `GameSignalEvent` + `GameFlagChangedEvent` + `QuestStateChangedEvent`。
  - Flag 与条件：`GameFlagCenter` + `GameFlagDatabase` + `GameFlagData` + `GameCondition` + `FlagBoolCondition`。
  - Quest 系统：`QuestManager` + `QuestDatabase` + `QuestData` + `QuestState` + `QuestStateCondition`。
  - 剧情序列：`StoryTrigger` + `StorySequenceRunner` + `StorySequence` + `StoryStepAction` + `StorySceneBindings` + `StoryContext`。

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

## 6. 输入与游戏层

输入配置来自 `Assets/Settings/InputSystem_Actions.inputactions`，生成代码为 `Assets/Settings/InputSystem_Actions.cs`。

- `Player` Action Map：移动、跳跃、攻击、冲刺、交互、使用物品。
- `Game` Action Map：暂停、打开背包、打开地图。
- `UI` Action Map：导航、确认、取消。

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
  - `InteractionTransform`：交互对象的位置。
  - `CanInteract(InteractionContext context)`：判断当前上下文是否允许交互。
  - `Interact(InteractionContext context)`：执行交互。
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

- 脚本职责：检测玩家范围内可交互对象，并选择最近目标交互。
- 关键字段：
  - `interactor`：发起交互的对象，通常是玩家。
  - `interactions`：当前触发器范围内的交互接口列表。
- 函数：
  - `Awake()`：默认把自身对象作为 `interactor`。
  - `TryInteract()`：创建上下文，查找最近可交互对象并调用 `Interact()`。
  - `OnTriggerEnter2D(Collider2D collision)`：进入范围时收集 `IInteractable`。
  - `OnTriggerExit2D(Collider2D collision)`：离开范围时移除 `IInteractable`。
  - `FindClosetInteraction()`：按距离选择最近且 `CanInteract()` 为 true 的目标。
- 关联：`PlayerGround` 在消费世界交互输入后调用它；`NPCDialogueInteractable` 是当前主要交互实现。

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
- 关联：`StorySequenceRunner` 把它放进 `StoryContext`，`ShowStoryTextStepAction`、`HideStoryTextStepAction`、`SetPlayerMoveModeStoryStepAction` 等步骤通过它访问场景对象。

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
  - `playerMovement`、`playerInputReceiver`、`playerAnimationTrigger`、`interaction`、`timeTool`、`groundSensor`：玩家子系统引用。
  - `idleState`、`walkState`、`runState`、`runTurnState`、`runEndState`、`jumpStartState`、`jumpUpState`、`apexState`、`fallState`：所有玩家状态实例。
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
  - `Idle`、`Run`、`RunTurn`、`RunEnd`、`Walk`、`JumpStart`、`JumpUp`、`Apex`、`Fall`、`BaseLand`、`RollingLand`。
- 关联：`Player` 创建状态时传入对应 hash，`EntityState.Enter/Exit()` 控制 Animator bool。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/PlayerState.cs`

- 脚本职责：玩家状态基类。
- 关键字段：
  - `player`：玩家主体。
  - `movement`：玩家移动组件。
  - `input`：输入缓存。
  - `groundSensor`：地面检测。
  - `animationTrigger`：动画事件状态。
- 函数：
  - `PlayerState(...)`：缓存玩家相关子系统。
  - `LogicalUpdate()`：统一刷新 `GroundSensor` 的落地状态。
  - `ChangeStateToMoveState()`：根据移动输入和当前 `PlayerMoveType` 切换到待机、步行或跑步。
- 关联：所有玩家具体状态继承它，避免重复拿组件。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/PlayerGround.cs`

- 脚本职责：地面状态公共逻辑。
- 函数：
  - `PlayerGround(...)`：调用玩家状态基类构造。
  - `LogicalUpdate()`：处理跳跃、世界交互；确认仍在地面时才执行移动/待机/跑步转换。
- 关联：`Player_IdleState`、`Player_WalkState`、跑步相关状态继承或间接使用地面逻辑。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/PlayerAir.cs`

- 脚本职责：空中状态公共逻辑。
- 函数：
  - `PlayerAir(...)`：调用玩家状态基类构造。
  - `LogicalUpdate()`：空中仍允许水平移动；落地后切回移动状态。
- 关联：`Player_JumpUp`、`Player_Apex`、`Player_Fall` 继承它。

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
  - `PhysicalUpdate()`：处理空中水平移动。
- 关联：从离地、跳跃上升结束或 Apex 进入；落地后通过 `PlayerAir.LogicalUpdate()` 回到地面移动状态。

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
6. 当前已有调试日志、等待秒数、等待条件、显示/隐藏剧情文本、设置玩家移动模式等步骤资源。

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
- `Assets/_Game/Data/Story/Steps/Step_Show_HelpCry.asset`
- `Assets/_Game/Data/Story/Steps/Step_Hide_StoryText.asset`
- `Assets/_Game/Data/Story/Steps/Step_SetMoveMode_Walk.asset`
- `Assets/_Game/Data/Story/Steps/Step_SetMoveMode_Run.asset`
- `Assets/_Game/Data/Story/Steps/Step_Wait_HelpOldMan_InProgress.asset`
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
- 新增 `StoryBindingKey`、`StorySceneBindings` 和 `StoryTextView`，剧情步骤可通过绑定 key 控制场景对象和剧情文本 UI。
- 新增剧情资源目录 `Assets/_Game/Data/Story/`，包含 `BindingKeys/` 和 `Steps/` 资源。
- 新增剧情步骤资源：显示/隐藏剧情文本、等待 1 秒/4 秒、等待老人帮助任务进入进行中、切换玩家 Walk/Run 移动模式。
- 新增基础移动组件 `Movement`，玩家移动组件继承它。
- 新增玩家 `Walk` 动画状态与 `Player_WalkState` 实际移动逻辑，`PlayerBaseConfig.asset` 中 `walkVelocity` 当前为 4。
- 玩家跳跃状态拆分为 `Player_JumpStart`、`Player_JumpUp`、`Player_Apex`、`Player_Fall`。
- 跑步过渡状态移动到 `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/Player_Run/`。
- `GameScene.unity` 已加入 `StorySceneBindings`、`StoryCanvas`、剧情文本视图和 Input System UI EventSystem。
- 老人对话资源已调整为“村庄救下后介绍”“苔藓区域说明”“苔藓区域兜底”三组数据。
- `BootScene.unity` 已接入 Quest 管理相关运行时对象引用。
- `InputRouter` 删除了确认对话选项时的调试日志输出。

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
- `QuestData` 中 `descrition` 字段存在拼写问题；因为是序列化字段，改名前应考虑 `[FormerlySerializedAs]`。
- `HideStoryTextStepAction` 和 `ShowStoryTextStepAction` 中缺失 `StorySceneBindings` 时的警告文本仍写成了设置玩家移动模式，后续可统一改文案。
- 当前项目没有在命令行中接入 Unity 编译/测试流程，脚本改动后建议在 Unity Editor 中观察 Console。

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
