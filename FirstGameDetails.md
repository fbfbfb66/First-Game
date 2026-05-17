# First Game Project Details

本文档用于记录当前 Unity 项目的结构、核心系统、资源变化和 Git 提交规则。项目结构、脚本职责、输入绑定、场景或资源发生变化后，应同步更新本文档。

## 1. 项目概览

- 项目类型：Unity 2D 游戏项目。
- 主要游戏内容目录：`Assets/_Game/`。
- 当前分支：`main`。
- 远程仓库：`https://github.com/fbfbfb66/First-Game.git`。
- 当前核心系统：
  - 场景加载：`SceneLoader` + `SceneNames`。
  - 游戏层管理：`GameLayerStack` + `GameLayerRuleDatabase` + `GameLayerRule` + `GameLayerType`。
  - 输入读取与路由：`GameInputReader` + `InputRouter`。
  - 玩家输入、移动与状态机：`PlayerInputReceiver` + `PlayerMovement` + `Player` + FSM 状态类。
  - 世界交互：`IInteractable` + `InteractionContext` + `InteractionDetector`。
  - 对话系统：`DialogueManager` + `DialogueData` + `DialogueNode` + `WorldDialogueView` + `WorldDialogueChoiceView`。
  - 事件总线：`GameEventBus` + `IGameEvent` + `GameSignalEvent`。

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
|   |-- AnimationController/
|   |   |-- NPC.controller
|   |   `-- Player.controller
|   `-- AnimationStates/
|       |-- NPC_OldMan/
|       `-- Player/
|-- Art/
|   |-- NPC/
|   `-- Player/
|-- Audio/
|-- Data/
|   |-- Core/
|   |-- Dialogue/
|   |   |-- Events/
|   |   `-- OldManIntroDialogue.asset
|   `-- Player/
|-- Fonts/
|-- Materials/
|-- Prefabs/
|-- Scenes/
|   |-- BootScene.unity
|   |-- GameScene.unity
|   `-- MainMenuScene.unity
|-- Scripts/
|   |-- Editor/
|   |-- Runtime/
|   `-- Tool/
|-- Settings/
|-- Shaders/
`-- TileMaps/
```

说明：Unity 的 `.meta` 文件保存资源 GUID 和导入设置，必须和对应资源一起提交。

## 5. 场景列表

`ProjectSettings/EditorBuildSettings.asset` 当前启用 3 个场景：

| 顺序 | 场景路径 | 用途 |
| --- | --- | --- |
| 0 | `Assets/_Game/Scenes/BootScene.unity` | 启动场景，承载启动流程对象 |
| 1 | `Assets/_Game/Scenes/MainMenuScene.unity` | 主菜单场景 |
| 2 | `Assets/_Game/Scenes/GameScene.unity` | 游戏主场景 |

## 6. 输入与游戏层

输入配置来自 `Assets/Settings/InputSystem_Actions.inputactions`，生成包装代码为 `Assets/Settings/InputSystem_Actions.cs`。

- `Player` Action Map：移动、跳跃、攻击、冲刺、交互、使用物品。
- `Game` Action Map：暂停、打开背包、打开地图。
- `UI` Action Map：导航、确认、取消。

`GameInputReader.SetInputMode(GameLayerType layerType)` 会按当前游戏层启用对应 Action Map：

- `Gameplay`：启用 `Player` 和 `Game`。
- 菜单和 UI 层：启用 `UI`。
- `Dialogue`：启用 `Player`。
- `Cutscene`：关闭输入。

`InputRouter` 负责把输入派发到当前层：

- `Gameplay` 层的交互输入会进入 `PlayerInputReceiver.RequestWorldInteract()`。
- 玩家地面状态中消费交互请求，并调用 `InteractionDetector.TryInteract()`。
- `Dialogue` 层的交互输入调用 `DialogueManager.RequestAdvance()`。
- `DialogueChoice` 层的导航和确认输入调用 `DialogueManager.HandleChoiceSelectedNavigate()` 与 `DialogueManager.HandleChoiceConfirmed()`。

## 7. 核心脚本职责

### Core

- `SceneNames.cs`：集中保存项目使用的场景名常量。
- `SceneLoader.cs`：封装场景加载、主菜单加载、游戏场景加载和退出游戏逻辑。
- `GameLayerType.cs`：定义当前游戏逻辑层，包含 `Dialogue` 和 `DialogueChoice`。
- `GameLayerStack.cs`：维护层栈，支持重置、压栈、出栈、查询和层变化事件。
- `GameLayerRule.cs`：序列化单条层规则，描述某层锁定哪些玩家动作。
- `GameLayerRuleDatabase.cs`：ScriptableObject 数据库，按游戏层查询锁定动作。

### Core/Events

- `IGameEvent.cs`：事件总线事件的标记接口。
- `GameEventBus.cs`：按事件类型保存订阅者，支持 `Subscribe`、`Unsubscribe` 和 `Publish`。
- `GameSignalEvent.cs`：通用信号事件，包含 `SignalID`、`Sender` 和 `Instigator`。
- `DebugGameSignalPublisher.cs`：调试用信号发布组件。
- `DebugGameSignalListener.cs`：调试用信号监听组件。

### Core/FSM

- `Entity.cs`：实体基类，持有 `Animator` 和 `StateMachine`。
- `EntityState.cs`：状态基类，提供 `Enter`、`LogicalUpdate`、`PhysicalUpdate`、`Exit`。
- `StateMachine.cs`：保存当前状态，支持初始化、切换和更新。

### GamePlay/Interaction

- `IInteractable.cs`：世界交互接口，提供交互位置、可交互判断、提示文本和交互执行。
- `InteractionContext.cs`：交互上下文，保存交互发起者。
- `InteractionDetector.cs`：收集触发范围内的 `IInteractable`，选择最近目标并执行交互。

### GamePlay/Dialogue

- `DialogueData.cs`：对话 ScriptableObject，保存对话 ID、起始节点 ID 和节点列表。
- `DialogueNode.cs`：对话节点，包含台词列表和选项列表。
- `DialogueLine.cs`：单句台词，包含说话者、文本、自动推进时间、等待输入标记、开始/结束事件。
- `DialogueChoice.cs`：对话选项，包含选项文本、下一节点 ID 和选择事件。
- `DialogueContext.cs`：对话事件执行上下文，包含事件总线、当前台词、当前节点、对话数据、发送者和触发者。
- `DialogueEventAction.cs`：对话事件 ScriptableObject 基类。
- `DialogueManager.cs`：驱动对话流程，管理对话层、选项层、台词推进、选项确认和事件执行。
- `NPCDialogueInteractable.cs`：让 NPC 通过世界交互启动指定对话。
- `WorldDialogueView.cs`：显示和隐藏世界空间台词文本。
- `WorldDialogueChoiceView.cs`：显示选项、处理上下导航、返回当前选中项。
- `DebugDialogueStarter.cs`：编辑器上下文菜单启动对话的调试组件。
- `Events/DebugLogDialogueEvent.cs`：对话事件，执行时输出调试日志。
- `Events/PublishSignalDialogueEvent.cs`：对话事件，执行时通过 `GameEventBus` 发布 `GameSignalEvent`。

### GamePlay/NPC

- `Interaction.cs`：NPC 名字显示与朝向处理组件，在玩家进入触发范围时显示名字。

### GamePlay/Player

- `Player.cs`：玩家实体入口，绑定移动、输入、动画触发器、交互检测器、刚体、计时工具和玩家状态。
- `PlayerBaseConfig.cs`：玩家基础配置 ScriptableObject，包含移动速度、跳跃力、重力、跑步条件和滑行参数。
- `PlayerInputReceiver.cs`：玩家输入接收点，缓存移动输入，并提供一次性动作请求消费接口。
- `PlayerMovement.cs`：处理水平移动、跳跃、朝向翻转、刚体速度和移动参数初始化。
- `PlayerAnimationTrigger.cs`：接收动画事件，记录当前动画是否结束。
- `Player_RunTransition.cs`：跑步相关状态的共同基类，提供输入方向和角色朝向是否一致的判断。
- `PlayerControlArbitration.cs`：结合当前游戏层规则判断玩家动作是否被锁定。

### GamePlay/Player/PlayerState

- `PlayerState.cs`：玩家状态基类，缓存 `Player`、`PlayerMovement`、`PlayerInputReceiver` 和 `PlayerAnimationTrigger`。
- `PlayerGround.cs`：地面状态，消费世界交互请求，并按水平输入切换 `Idle` 与 `Run`。
- `PlayerAir.cs`：空中状态基类。
- `Player_IdleState.cs`：待机状态，进入时清空刚体速度。
- `Player_WalkState.cs`：行走状态。
- `Player_JumpState.cs`：跳跃状态。
- `Player_RunState.cs`：跑步状态，处理持续奔跑、松手缓冲、提前松手、正常刹车和反向转身。
- `Player_RunEndState.cs`：跑步结束状态，按当前朝向进行短时间滑行，结束后回到待机。
- `Player_RunTurnState.cs`：跑步转身状态，等待转身动画结束，再根据输入回到跑步或待机。
- `PlayerAnimationHash.cs`：集中保存动画状态 hash。

## 8. 对话与事件设计

当前对话流程以 `DialogueData` 为数据源，以 `DialogueManager` 为运行时驱动：

1. 玩家在 `Gameplay` 层按交互键。
2. `InputRouter` 将交互请求发送给 `PlayerInputReceiver`。
3. `PlayerGround` 消费请求，并调用 `InteractionDetector.TryInteract()`。
4. `NPCDialogueInteractable` 检查可交互条件后调用 `DialogueManager.StartDialogue()`。
5. `DialogueManager` 压入 `Dialogue` 层，逐句显示台词。
6. 台词结束后如果节点有选项，显示 `WorldDialogueChoiceView` 并压入 `DialogueChoice` 层。
7. 选项确认后跳转到下一节点，并执行选项事件。
8. 对话结束时隐藏 UI，并弹出对话相关层。

事件扩展点：

- 台词开始事件：`DialogueLine.EventOnStart`。
- 台词结束事件：`DialogueLine.EventOnEnd`。
- 选项选择事件：`DialogueChoice.EventOnSelect`。
- 通用事件可继承 `DialogueEventAction`。
- 跨系统信号优先通过 `PublishSignalDialogueEvent` 发布 `GameSignalEvent`。

当前数据资产：

- `Assets/_Game/Data/Dialogue/OldManIntroDialogue.asset`。
- `Assets/_Game/Data/Dialogue/Events/DebugEvent_HelpOldMan.asset`。
- `Assets/_Game/Data/Dialogue/Events/DebugEvent_OldManLineStart.asset`。
- `Assets/_Game/Data/Dialogue/Events/Signal_OldManMentionsStoneGate.asset`。

## 9. 玩家跑步状态设计

当前已经移除 `RunStart` 状态，地面状态检测到跑步输入后直接进入 `Run`。

跑步相关动画状态：

- `Run`：持续跑步。
- `RunTurn`：跑步中反向转身。
- `RunEnd`：跑步结束后的刹车滑行。

跑步计时由 `TimeTool` 管理。`TimeTool` 使用 `Dictionary<string, Coroutine>` 以字符串 ID 同时管理多个计时器；计时器结束后会从字典中移除，状态逻辑通过 `ContainsKey` 判断计时窗口是否仍然存在。

当前 `Player_RunState` 使用两个计时窗口：

- `RunEndEarly`：进入 `Run` 后开启。窗口存在时，松手会被视为过早松手；窗口结束后松手可进入 `RunEnd`。
- `RunBuffer`：第一次松手时开启。窗口存在时，角色继续按当前朝向移动，用于避免点按方向键导致动画抖动。

当前跑步逻辑要点：

- 松手过早且 `RunBuffer` 已结束：进入 `Idle`。
- 松手且 `RunEndEarly` 已结束：进入 `RunEnd`。
- 跑步时间足够后输入反方向：进入 `RunTurn`。
- 跑步早期输入反方向：直接翻面并重置早期跑步计时，避免起跑阶段频繁转身抖动。
- `PlayerMovement.HandleMove` 只负责设置速度，不自动翻面。
- `PlayerMovement.HandleMoveAndFlip` 保留给需要移动并同步朝向的场景。
- `RunTurnState.Exit` 会调用 `movement.HandleFlip(input.MoveInput)`，让转身动画结束后再同步朝向。

建议参数起点：

| 参数 | 当前用途 | 建议范围 |
| --- | --- | --- |
| `CanEndRunEarlyDuration` | 控制进入 `Run` 后多久才允许播放 `RunEnd` | `0.18` - `0.35` |
| `RunBufferDuration` | 松手缓冲，过滤点按导致的抖动 | `0.08` - `0.15` |
| `CoastingDuration` | `RunEnd` 滑行时长 | `0.12` - `0.22` |

## 10. 当前资源更新

- 资源目录重组：旧的 `Art` 动作分散目录被移除，新增 `Art/Player` 与 `Art/NPC`。
- 动画状态目录重组：新增 `AnimationStates/Player` 与 `AnimationStates/NPC_OldMan`。
- 新增 `NPC.controller`，更新 `Player.controller`。
- 新增 `Fonts/`，并更新 TextMesh Pro 字体材质资源。
- 新增对话数据目录 `Data/Dialogue/` 和对话事件资产。
- 更新 `BootScene.unity` 与 `GameScene.unity`，接入事件总线、对话管理器、NPC、世界交互和对话 UI。
- 更新 `ProjectSettings/TagManager.asset` 与 `ProjectSettings/Physics2DSettings.asset`。
- 更新 `PlayerBaseConfig.asset` 中的玩家移动/跑步参数。

## 11. 当前注意事项

- `NPCDialogueInteractable.GetInteractionPrompt()` 目前仍抛出 `NotImplementedException`，若后续需要显示交互提示，需要补齐实现。
- `WorldDialogueChoiceView` 字段名 `nvaigationCooldown` 存在拼写问题，但会影响序列化字段名，重命名前要考虑 Unity 资产迁移。
- `Interaction` 中 `faingRight` 字段存在拼写问题，不影响当前逻辑。
- `StateMachine.LogicialUpdate` 存在拼写问题，但当前 `Player.Update()` 使用同名调用，功能上可运行。
- `StateMachine.InitializeState` 目前只赋值初始状态，没有调用初始状态的 `Enter()`；如后续依赖初始状态进入逻辑，需要检查。
- `GameEventBus.Publish()` 在没有订阅者时会输出 warning，调试阶段有帮助；正式版本可视情况降低日志级别。
- `RunBufferDuration` 不宜过长，否则角色松手后会显得不够听话，通常 `0.1` 秒左右比较自然。

## 12. 更新规则

以后更新本文档时建议按以下顺序执行：

1. 重新扫描 `Assets/_Game/` 的目录和文件。
2. 重新读取 `Assets/_Game/Scripts/**/*.cs`。
3. 更新每个游戏脚本的职责、字段、函数和注意事项。
4. 检查 `ProjectSettings/EditorBuildSettings.asset` 的启用场景是否变化。
5. 检查 `Assets/Settings/InputSystem_Actions.inputactions` 的 Action Map、Action 和 Binding 是否变化。
6. 检查 `Assets/_Game/Data/Core/GameLayerRuleDatabase.asset` 的层规则是否变化。
7. 检查 `Packages/manifest.json` 是否新增或移除关键依赖。
8. 检查 `.gitignore` 和 `.gitattributes` 是否符合 Unity 提交规范。
