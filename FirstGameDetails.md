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
  - 对话系统：`DialogueManager` + `DialogueData` + `NPCDialogueProfile` + `ConditionalDialogueEntry` + `WorldDialogueView` + `WorldDialogueChoiceView`。
  - 事件总线：`GameEventBus` + `IGameEvent` + `GameSignalEvent` + `GameFlagChangedEvent` + `QuestStateChangedEvent`。
  - Flag 与条件：`GameFlagCenter` + `GameFlagDatabase` + `GameFlagData` + `GameCondition` + `FlagBoolCondition`。
  - Quest 系统：`QuestManager` + `QuestDatabase` + `QuestData` + `QuestState` + `QuestStateCondition`。

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
|   |   |-- DialogueData/
|   |   |-- Events/
|   |   |   |-- Condition/
|   |   |   |-- SetFlag/
|   |   |   `-- SetQuest/
|   |   `-- OldManDialogueProfile.asset
|   |-- Flags/
|   |-- Player/
|   `-- Quests/
|-- Fonts/
|-- Materials/
|-- Prefabs/
|-- Scenes/
|-- Scripts/
|   |-- Editor/
|   |-- Runtime/
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

- `Gameplay` 的交互输入进入 `PlayerInputReceiver.RequestWorldInteract()`。
- 玩家地面状态中消费交互请求，并调用 `InteractionDetector.TryInteract()`。
- `Dialogue` 的交互输入调用 `DialogueManager.RequestAdvance()`。
- `DialogueChoice` 的导航与确认输入调用 `DialogueManager.HandleChoiceSelectedNavigate()` 和 `DialogueManager.HandleChoiceConfirmed()`。

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
- `GameFlagChangedEvent.cs`：Flag 变化事件，包含 Flag ID、旧值、新值、发送者和触发者。
- `QuestStateChangedEvent.cs`：Quest 状态变化事件，包含 Quest 数据、ID、旧状态、新状态、发送者和触发者。

### Core/Flags

- `GameFlagData.cs`：单个布尔 Flag 定义，包含 `FlagID`、默认值和描述。
- `GameFlagDatabase.cs`：ScriptableObject Flag 数据库，保存初始 `GameFlagData` 列表。
- `GameFlagCenter.cs`：运行时 Flag 中心，初始化默认 Flag，提供 `SetBool`、`GetBool` 和 `HasBool`，并在值变化时发布 `GameFlagChangedEvent`。

`GameFlagEntry.cs` 已被 `GameFlagData.cs` 替代；条件和对话事件现在通过 ScriptableObject 引用 Flag 数据，而不是手填字符串 ID。

### Core/Quests

- `QuestState.cs`：Quest 状态枚举，包含 `NotStarted`、`InProgress`、`Completed`、`Rewarded` 和 `Failed`。
- `QuestData.cs`：单个 Quest 定义，包含 `QuestID`、标题、描述和默认状态。
- `QuestDatabase.cs`：ScriptableObject Quest 数据库，保存初始 Quest 列表。
- `QuestManager.cs`：运行时 Quest 中心，初始化 Quest 状态，提供 `HasQuest`、`GetQuestState` 和 `SetQuestState`。
- `DebugQuestStateListener.cs`：调试监听器，订阅 `QuestStateChangedEvent` 并输出状态变化日志。

### Core/Conditions

- `GameCondition.cs`：条件 ScriptableObject 基类。
- `GameConditionContext.cs`：条件检查上下文，保存 `GameFlagCenter`、`QuestManager`、发送者和触发者。
- `FlagBoolCondition.cs`：布尔 Flag 条件，检查指定 `GameFlagData` 是否等于期望值。
- `QuestStateCondition.cs`：Quest 状态条件，检查指定 `QuestData` 是否等于期望状态。

### GamePlay/Dialogue

- `DialogueData.cs`：对话 ScriptableObject，保存对话 ID、起始节点 ID 和节点列表。
- `DialogueNode.cs`：对话节点，包含台词列表和选项列表。
- `DialogueLine.cs`：单句台词，包含说话者、文本、自动推进时间、等待输入标记、开始/结束事件。
- `DialogueChoice.cs`：对话选项，包含选项文本、下一个节点 ID 和选择事件。
- `DialogueContext.cs`：对话事件执行上下文，包含事件总线、Flag 中心、Quest 管理器、当前台词、当前节点、对话数据、发送者和触发者。
- `DialogueEventAction.cs`：对话事件 ScriptableObject 基类。
- `DialogueManager.cs`：驱动对话流程，管理对话层、选项层、台词推进、选项确认和事件执行；现在会查找并传递 `QuestManager`。
- `ConditionalDialogueEntry.cs`：一条条件对话入口，持有 `DialogueData` 和条件列表，所有条件满足时可被选中。
- `NPCDialogueProfile.cs`：NPC 对话配置，按 `ConditionalDialogueEntry` 顺序选择第一条满足条件的对话。
- `NPCDialogueInteractable.cs`：让 NPC 通过世界交互启动对话，使用 `NPCDialogueProfile` 按 Flag 和 Quest 条件选择对话数据。
- `WorldDialogueView.cs`：显示和隐藏世界空间台词文本。
- `WorldDialogueChoiceView.cs`：显示选项、处理上下导航、返回当前选中项。
- `Events/SetFlagDialogueEvent.cs`：通过 `GameFlagCenter` 设置 `GameFlagData` 对应的布尔 Flag。
- `Events/SetQuestStateDialogueEvent.cs`：通过 `QuestManager` 设置 `QuestData` 对应的 Quest 状态。
- `Events/PublishSignalDialogueEvent.cs`：通过 `GameEventBus` 发布 `GameSignalEvent`。
- `Events/DebugLogDialogueEvent.cs`：执行时输出调试日志。

## 8. 对话、条件、Flag 与 Quest 流程

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

## 9. 玩家跑步状态设计

当前已经移除 `RunStart` 状态，地面状态检测到跑步输入后直接进入 `Run`。

跑步相关动画状态：

- `Run`：持续跑步。
- `RunTurn`：跑步中反向转身。
- `RunEnd`：跑步结束后的刹车滑行。

当前 `Player_RunState` 使用两个计时窗口：

- `RunEndEarly`：进入 `Run` 后开启。窗口存在时，松手会被视为过早松手；窗口结束后松手可进入 `RunEnd`。
- `RunBuffer`：第一次松手时开启。窗口存在时，角色继续按当前朝向移动，用于避免点按方向键导致动画抖动。

建议参数起点：

| 参数 | 当前用途 | 建议范围 |
| --- | --- | --- |
| `CanEndRunEarlyDuration` | 控制进入 `Run` 后多久才允许播放 `RunEnd` | `0.18` - `0.35` |
| `RunBufferDuration` | 松手缓冲，过滤点按导致的抖动 | `0.08` - `0.15` |
| `CoastingDuration` | `RunEnd` 滑行时长 | `0.12` - `0.22` |

## 10. 当前资源更新

- 对话事件目录已整理为 `Condition/Flag`、`Condition/Quest`、`SetFlag` 和 `SetQuest` 子目录。
- 旧的手填字符串 Flag 方式已改为 `GameFlagData` 资源引用。
- 新增 Quest 数据目录 `Assets/_Game/Data/Quests/`。
- 新增 `DefaultQuestDatabase.asset` 与 `HelpOldManQuest.asset`。
- 新增 `QuestManager`、`QuestData`、`QuestDatabase`、`QuestState`、`QuestStateCondition`、`QuestStateChangedEvent` 和 `SetQuestStateDialogueEvent`。
- 老人对话资源已调整为“村庄救下后介绍”“苔藓区域说明”“苔藓区域兜底”三组数据。
- `BootScene.unity` 已接入 Quest 管理相关运行时对象引用。
- `InputRouter` 删除了确认对话选项时的调试日志输出。

## 11. 已处理事项与仍需留意

已处理：

- `GameFlagEntry` 已迁移为 `GameFlagData`。
- `FlagBoolCondition` 与 `SetFlagDialogueEvent` 已通过 `GameFlagData` 获取 Flag ID。
- `DialogueContext` 和 `GameConditionContext` 已加入 `QuestManager`。
- `DialogueManager` 和 `NPCDialogueInteractable` 会在缺省时查找 `QuestManager`。
- 对话条件现在同时支持 Flag 条件和 Quest 状态条件。
- 对话事件现在支持设置 Quest 状态。

仍需留意：

- `FlagBoolCondition` 和 `SetFlagDialogueEvent` 中的 `flagData` 需要在资源里正确赋值，避免空引用。
- `QuestManager` 只能设置已注册在 `QuestDatabase` 中的 Quest。
- 当前项目没有在命令行中接入 Unity 编译/测试流程，脚本改动后建议在 Unity Editor 中观察 Console。

## 12. 更新规则

以后更新本文档时建议按以下顺序执行：

1. 重新扫描 `Assets/_Game/` 的目录和文件。
2. 重新读取 `Assets/_Game/Scripts/**/*.cs`。
3. 更新每个游戏脚本的职责、字段、函数和注意事项。
4. 检查 `ProjectSettings/EditorBuildSettings.asset` 的启用场景是否变化。
5. 检查 `Assets/Settings/InputSystem_Actions.inputactions` 的 Action Map、Action 和 Binding 是否变化。
6. 检查 `Assets/_Game/Data/Core/GameLayerRuleDatabase.asset` 的层规则是否变化。
7. 检查对话、Flag、Quest、Condition 相关 ScriptableObject 是否新增、移动或删除。
8. 检查 `Packages/manifest.json` 是否新增或移除关键依赖。
9. 检查 `.gitignore` 和 `.gitattributes` 是否符合 Unity 提交规范。
