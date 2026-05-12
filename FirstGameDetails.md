# First Game Project Details

本文档用于手动追踪当前 Unity 项目的结构、核心资源、脚本职责和 Git 上传规则。项目目录、场景、脚本、输入绑定或包配置变化后，需要重新扫描项目并更新本文档。

## 1. 项目概览

- 项目类型：Unity 2D 游戏项目。
- 主要游戏内容目录：`Assets/_Game/`。
- 当前分支：`main`。
- 远端仓库：`https://github.com/fbfbfb66/First-Game.git`。
- 当前场景流：
  - `BootScene` 作为启动场景。
  - `GameBootstrap` 保留启动对象，加载 `MainMenuScene`，并将游戏层重置为 `MainMenu`。
  - `MainMenuController` 点击新游戏后加载 `GameScene`，并将游戏层重置为 `Gameplay`。
- 当前主要系统：
  - 场景加载：`SceneLoader` + `SceneNames`。
  - 游戏层管理：`GameLayerStack` + `GameLayerRuleDatabase` + `GameLayerRule` + `GameLayerType`。
  - 输入读取与路由：`GameInputReader` + `InputRouter`。
  - 玩家输入、移动与状态：`PlayerInputReceiver` + `PlayerMovement` + `Player` + FSM 状态类。
  - 世界交互：`IIteractable` + `InteractionContext` + `InteractionDetector`。

## 2. Unity Git 上传规范

需要提交：

- `Assets/`：手写脚本、场景、预制体、动画、图片、音频、ScriptableObject 和对应 `.meta`。
- `Packages/manifest.json` 与 `Packages/packages-lock.json`。
- `ProjectSettings/`：项目设置、Build Settings、Tags/Layers、URP/Physics/Input 等配置。
- `.gitignore`、`.gitattributes`、项目文档。

不提交：

- `Library/`、`Temp/`、`Obj/`、`Logs/`、`UserSettings/`。
- IDE 自动生成文件：`*.csproj`、`*.sln`、`*.slnx`、`*.csproj.lscache`、`.vs/`、`.idea/`。
- Build 输出、录屏、崩溃日志和本机缓存。

当前 `.gitattributes` 使用 Git LFS 管理常见二进制资源：`*.png`、`*.jpg`、`*.jpeg`、`*.psd`、`*.aseprite`、`*.wav`、`*.mp3`、`*.ogg`、`*.fbx`、`*.mp4`、`*.mov`、`*.blend`。

## 3. 根目录结构

```text
First Game/
├── .git/                Git 仓库数据
├── .vscode/             本机 IDE 配置，不提交
├── Assets/              Unity 资源与游戏源代码，提交
├── Library/             Unity 自动缓存，不提交
├── Logs/                Unity 日志，不提交
├── Packages/            Unity 包依赖清单，提交
├── ProjectSettings/     Unity 项目设置，提交
├── Temp/                Unity 临时文件，不提交
├── UserSettings/        本机用户设置，不提交
├── .gitattributes       Git LFS 规则
├── .gitignore           Git 忽略规则
└── FirstGameDetails.md  当前项目追踪文档
```

## 4. `Assets/_Game` 结构

```text
Assets/_Game/
├── Animation/
│   ├── AnimationController/
│   │   └── Player.controller
│   └── AnimationStates/
│       ├── Player_Idle.anim
│       └── Player_Run.anim
├── Art/
│   ├── aerial dash/
│   ├── atk/
│   ├── back dodge/
│   ├── crouching/
│   ├── dodge atk/
│   ├── healing/
│   ├── hit fx/
│   ├── hurt/
│   ├── idle/
│   ├── jump/
│   ├── jump attack/
│   ├── ladder action/
│   ├── ledge action/
│   ├── run/
│   ├── slide/
│   ├── walk/
│   └── wall jump/
├── Audio/
├── Data/
│   ├── Core/
│   │   └── GameLayerRuleDatabase.asset
│   └── Player/
│       └── PlayerBaseConfig.asset
├── Materials/
├── Prefabs/
├── Scenes/
│   ├── BootScene.unity
│   ├── GameScene.unity
│   └── MainMenuScene.unity
├── Scripts/
│   ├── Editor/
│   └── Runtime/
├── Settings/
├── Shaders/
└── TileMaps/
```

说明：Unity 的 `.meta` 文件保存资源 GUID 和导入设置，必须和对应资源一起提交。

## 5. 场景列表

`ProjectSettings/EditorBuildSettings.asset` 当前启用 3 个场景：

| 顺序 | 场景路径 | 用途 |
| --- | --- | --- |
| 0 | `Assets/_Game/Scenes/BootScene.unity` | 启动场景，承载启动流程对象 |
| 1 | `Assets/_Game/Scenes/MainMenuScene.unity` | 主菜单场景 |
| 2 | `Assets/_Game/Scenes/GameScene.unity` | 游戏主场景 |

当前脚本通过 `SceneNames.MainMenuScene` 和 `SceneNames.GameScene` 保存场景名，再由 `SceneLoader` 调用 `SceneManager.LoadScene` 加载。

## 6. 输入与游戏层

输入配置来自 `Assets/Settings/InputSystem_Actions.inputactions`，生成包装代码为 `Assets/Settings/InputSystem_Actions.cs`。

- `Player` Action Map：移动、跳跃、攻击、冲刺、交互、使用物品。
- `Game` Action Map：暂停、打开背包、打开地图。
- `UI` Action Map：导航、确认、取消。

当前游戏层枚举：

- `None`
- `MainMenu`
- `Gameplay`
- `Dialogue`
- `DialogueChoice`
- `Inventory`
- `Map`
- `ServiceMenu`
- `Pause`
- `Cutscene`

`GameInputReader.SetInputMode(GameLayerType layerType)` 会按当前层启用对应 Action Map：

- `Gameplay`：启用 `Player` 和 `Game`。
- 菜单和 UI 层：启用 `UI`。
- `Dialogue`：启用 `Player`。
- `Cutscene`：关闭输入。

## 7. 核心脚本职责

### Core

- `SceneNames.cs`：集中保存项目使用的场景名常量。
- `SceneLoader.cs`：封装场景加载、主菜单加载、游戏场景加载和退出游戏逻辑。
- `GameLayerType.cs`：定义当前游戏逻辑层。
- `GameLayerStack.cs`：维护层栈，支持重置、压栈、出栈、查询和层变化事件。
- `GameLayerRule.cs`：序列化单条层规则，描述某层锁定哪些玩家动作。
- `GameLayerRuleDatabase.cs`：ScriptableObject 数据库，按游戏层查询锁定动作。

### Core/FSM

- `Entity.cs`：实体基类，持有 `Animator` 和 `StateMachine`。
- `EntityState.cs`：状态基类，提供 `Enter`、`LogicalUpdate`、`PhysicalUpdate`、`Exit`。
- `StateMachine.cs`：保存当前状态，支持初始化、切换和更新。

### GameFlow

- `GameBootstrap.cs`：启动入口，保留启动对象，加载主菜单，并初始化游戏层。

### Input

- `GameInputReaders.cs`：封装 Unity Input System 生成类，将输入回调转换成项目事件。
- `InputRouter.cs`：订阅输入事件，根据当前游戏层和动作仲裁结果将输入路由到玩家、层栈或 UI 占位逻辑。

### GamePlay/Interaction

- `IIteractable.cs`：世界交互对象接口，提供交互点、可交互判断、执行交互和提示文本。
- `InteractionContext.cs`：交互上下文，保存交互发起者和发起者 Transform。
- `InteractionDetector.cs`：通过 2D Trigger 收集可交互对象，选择最近目标并执行交互。

### GamePlay/Player

- `Player.cs`：玩家实体入口，绑定移动、输入、刚体与动画状态，驱动 FSM 更新。
- `PlayerBaseConfig.cs`：玩家基础配置 ScriptableObject，包含跑步速度、走路速度、跳跃力和重力缩放。
- `PlayerMoveType.cs`：玩家移动模式枚举，当前包含 `Walk` 和 `Run`。
- `PlayerMovement.cs`：处理水平移动、跳跃、朝向翻转、刚体速度和移动参数初始化。
- `PlayerActionType.cs`：可被游戏层规则锁定的玩家动作 Flags。
- `PlayerControlArbitration.cs`：聚合当前激活层规则，判断移动、跳跃、攻击、冲刺、交互、使用物品是否允许。
- `PlayerInputReceiver.cs`：玩家输入接收点，缓存移动输入，并提供一次性动作请求消费接口。

### GamePlay/Player/PlayerState

- `PlayerState.cs`：玩家状态基类，持有 `Player`、`PlayerMovement` 和 `PlayerInputReceiver`。
- `PlayerGround.cs`：地面状态，按水平输入切换 Idle/Run。
- `PlayerMoveState.cs`：移动状态，调用 `PlayerMovement.HandleMove`。
- `Player_IdleState.cs`：玩家待机状态。
- `Player_RunState.cs`：玩家跑步状态，进入时设置移动模式为 `Run`。
- `PlayerAir.cs`：当前仍是 Unity 模板占位脚本。
- `PlayerAnimationHash.cs`：集中保存动画状态 Hash，包括 Idle、Run、Walk、Jump。

### UI

- `MainMenuController.cs`：管理主菜单按钮事件，将新游戏、继续游戏、退出游戏转换为对应流程。

## 8. 当前资源更新

- 新增玩家动画控制器：`Assets/_Game/Animation/AnimationController/Player.controller`。
- 新增玩家 Idle/Run 动画状态：`Player_Idle.anim`、`Player_Run.anim`。
- 新增大量角色动作美术资源，包含 `.aseprite` 源文件和导出的 `.png` sprite sheets。
- 新增玩家配置资源：`Assets/_Game/Data/Player/PlayerBaseConfig.asset`。
- `GameScene.unity` 已更新，包含玩家相关对象、动画、输入或物理配置改动。
- `ProjectSettings/TagManager.asset` 已更新，通常对应 Tag/Layer/Sorting Layer 等 Unity 项目设置变化。

## 9. 包与技术栈

主要依赖来自 `Packages/manifest.json`：

- Unity 2D 工具链：2D Animation、Aseprite、PSD Importer、Sprite、SpriteShape、Tilemap、Tilemap Extras。
- 渲染：Universal Render Pipeline。
- 输入：Unity Input System。
- UI：UGUI 和 TextMesh Pro。
- 编辑器与工作流：Visual Studio、Rider、Unity Collab Proxy。
- AI 相关：Unity AI Assistant、Unity AI Inference。
- 测试与流程：Unity Test Framework、Timeline、Visual Scripting。

## 10. 当前注意事项

- `PlayerInputReceiver` 中的 `jumpPressed`、`attackPressed`、`dashPressed`、`worldInteractPressed` 已有消费接口，但当前请求函数仍只输出日志，没有设置这些布尔值；后续接入 FSM 动作时需要补齐。
- `StateMachine.InitializeState` 当前只赋值初始状态，没有调用初始状态的 `Enter()`；如果依赖动画进入逻辑，可能需要检查。
- `StateMachine.LogicialUpdate` 存在拼写问题，但当前 `Player.Update()` 使用同名调用，功能上可运行。
- `InteractionContext.IneractorTransform` 存在拼写问题，若后续对外 API 固化，建议统一命名。
- `InputRouter.OnEnable()` 如果 `inputReader` 为空会直接返回，因此不会订阅任何输入事件；当前依赖场景中能找到 `GameInputReader`。
- `Game.Pause` 和 `UI.Cancel` 都可能使用 Escape，需要继续留意同键位在不同 Action Map 下的行为。

## 11. 更新规则

以后更新本文档时建议按以下顺序执行：

1. 重新扫描 `Assets/_Game/` 的目录和文件。
2. 重新读取 `Assets/_Game/Scripts/**/*.cs`。
3. 更新每个游戏脚本的职责、字段、函数和注意事项。
4. 检查 `ProjectSettings/EditorBuildSettings.asset` 的启用场景是否变化。
5. 检查 `Assets/Settings/InputSystem_Actions.inputactions` 的 Action Map、Action 和 Binding 是否变化。
6. 检查 `Assets/_Game/Data/Core/GameLayerRuleDatabase.asset` 的层规则是否变化。
7. 检查 `Packages/manifest.json` 是否新增或移除关键依赖。
8. 检查 `.gitignore` 和 `.gitattributes` 是否符合 Unity 上传规范。
