# First Game Project Details

本文档用于手动追踪当前 Unity 项目的结构、核心文件和脚本职责。它是当前项目快照：当目录、场景、脚本、输入绑定或包配置变化后，需要重新扫描项目并更新本文档。

## 1. 项目概览

- 项目类型：Unity 2D 游戏项目。
- 主要游戏内容目录：`Assets/_Game/`。
- 当前场景流：
  - `BootScene` 作为启动场景。
  - `GameBootstrap` 保留启动对象，加载 `MainMenuScene`，并把游戏层重置为 `MainMenu`。
  - `MainMenuController` 点击新游戏后加载 `GameScene`，并把游戏层重置为 `Gameplay`。
- 当前核心脚本数量：12 个，全部位于 `Assets/_Game/Scripts/Runtime/`。
- 当前核心系统：
  - 场景加载：`SceneLoader` + `SceneNames`。
  - 游戏层管理：`GameLayerStack` + `GameLayerRuleDatabase` + `GameLayerRule` + `GameLayerType`。
  - 输入读取与路由：`GameInputReader` + `InputRouter`。
  - 玩家输入接收与动作仲裁：`PlayerInputReceiver` + `PlayerControlArbitration` + `PlayerActionType`。
- 文档维护方式：手动同步。以后项目变化时，重新扫描目录和脚本后更新本文档。

## 2. 根目录结构

```text
First Game/
├─ .git/                         Git 仓库数据
├─ .vscode/                      IDE 配置，通常不提交
├─ Assets/                       Unity 资源与游戏源码
├─ Library/                      Unity 自动生成缓存，不提交
├─ Logs/                         Unity 日志，不提交
├─ Packages/                     Unity 包依赖清单
├─ ProjectSettings/              Unity 项目设置
├─ Temp/                         Unity 临时文件，不提交
├─ UserSettings/                 本机用户设置，通常不提交
├─ .gitattributes                Git LFS 跟踪规则
├─ .gitignore                    Git 忽略规则
└─ FirstGameDetails.md           当前项目追踪文档
```

根目录还可能出现 Unity 或 IDE 生成的 `.csproj`、`.slnx`、`.csproj.lscache` 等文件。这些属于编辑器/IDE 生成内容，不作为项目源码详细追踪。

## 3. `Assets/_Game` 文件结构

```text
Assets/_Game/
├─ Art/                          游戏美术资源目录，当前仅有 .meta
├─ Audio/                        游戏音频资源目录，当前仅有 .meta
├─ Data/
│  └─ Core/
│     └─ GameLayerRuleDatabase.asset  游戏层规则资产
├─ Materials/                    游戏材质目录，当前仅有 .meta
├─ Prefabs/                      游戏预制体目录，当前仅有 .meta
├─ Scenes/
│  ├─ BootScene.unity            启动场景
│  ├─ GameScene.unity            游戏主场景
│  └─ MainMenuScene.unity        主菜单场景
├─ Scripts/
│  ├─ Editor/                    编辑器脚本目录，当前无脚本
│  └─ Runtime/
│     ├─ Core/
│     │  ├─ GameLayerRule.cs
│     │  ├─ GameLayerRuleDatabase.cs
│     │  ├─ GameLayerStack.cs
│     │  ├─ GameLayerType.cs
│     │  ├─ SceneLoader.cs
│     │  └─ SceneNames.cs
│     ├─ GameFlow/
│     │  └─ GameBootstrap.cs
│     ├─ GamePlay/
│     │  └─ Player/
│     │     ├─ PlayerActionType.cs
│     │     ├─ PlayerControlArbitration.cs
│     │     └─ PlayerInputReceiver.cs
│     ├─ Input/
│     │  ├─ GameInputReaders.cs
│     │  └─ InputRouter.cs
│     ├─ Systems/                系统脚本目录，当前无脚本
│     └─ UI/
│        └─ MainMenuController.cs
├─ Settings/                     游戏自定义设置目录，当前仅有 .meta
├─ Shaders/                      游戏 Shader 目录，当前仅有 .meta
└─ TileMaps/                     Tilemap 资源目录，当前仅有 .meta
```

Unity 的 `.meta` 文件用于保存资源 GUID 和导入设置，必须和对应资源一起保留并提交。

## 4. 场景列表

`ProjectSettings/EditorBuildSettings.asset` 中当前启用 3 个场景：

| 顺序 | 场景路径 | 用途 |
| --- | --- | --- |
| 0 | `Assets/_Game/Scenes/BootScene.unity` | 启动场景，承载启动流程对象 |
| 1 | `Assets/_Game/Scenes/MainMenuScene.unity` | 主菜单场景 |
| 2 | `Assets/_Game/Scenes/GameScene.unity` | 游戏主场景 |

当前脚本通过 `SceneNames.MainMenuScene` 和 `SceneNames.GameScene` 保存场景名，再由 `SceneLoader` 调用 `SceneManager.LoadScene` 加载。

## 5. 输入与游戏层概览

输入配置来自 `Assets/Settings/InputSystem_Actions.inputactions`，生成包装代码为 `Assets/Settings/InputSystem_Actions.cs`。

- `Player` Action Map：
  - 负责移动、跳跃、攻击、冲刺、交互、使用物品等玩家动作。
  - `Interact` 绑定了 `<Keyboard>/f`。
- `Game` Action Map：
  - `Pause` 绑定 `<Keyboard>/escape`。
  - `OpenInventory` 绑定 `<Keyboard>/i`。
  - `OpenMap` 绑定 `<Keyboard>/m`。
- `UI` Action Map：
  - 负责导航、确认、取消等 UI 输入。
  - 当前 `Cancel` 也包含 `<Keyboard>/escape` 相关路径，可能和 `Game.Pause` 在同一按键上产生行为重叠。

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

`GameInputReader.SetInputMode(GameLayerType layerType)` 会根据当前层启用不同 Action Map：

- `Gameplay`：启用 `Player` 和 `Game`。
- `MainMenu`、`Inventory`、`Map`、`ServiceMenu`、`Pause`、`DialogueChoice`：启用 `UI`。
- `Dialogue`：启用 `Player`。
- `Cutscene`：不启用输入。

## 6. 包与技术栈

主要依赖来自 `Packages/manifest.json`：

- Unity 2D 工具链：2D Animation、Aseprite、PSD Importer、Sprite、SpriteShape、Tilemap、Tilemap Extras。
- 渲染：`com.unity.render-pipelines.universal`，项目使用 URP。
- 输入：`com.unity.inputsystem`。
- UI：`com.unity.ugui` 和 TextMesh Pro。
- 编辑器与工作流：Visual Studio、Rider、Unity Collab Proxy。
- AI 相关：Unity AI Assistant、Unity AI Inference。
- 测试与流程：Unity Test Framework、Timeline、Visual Scripting。

## 7. Git 配置

### `.gitignore`

当前 `.gitignore` 用于排除 Unity 和 IDE 自动生成内容，包括：

- Unity 缓存和临时目录：`Library/`、`Temp/`、`Obj/`、`Build/`、`Builds/`、`Logs/`、`UserSettings/`。
- 采样、录制、崩溃和日志文件：`MemoryCaptures/`、`Recordings/`、`sysinfo.txt`、`*.log`。
- IDE 生成文件：`.vs/`、`.idea/`、`*.csproj`、`*.sln`、`*.slnx`、`*.csproj.lscache` 等。
- 系统文件：`.DS_Store`、`Thumbs.db`。

### `.gitattributes`

当前 `.gitattributes` 将常见二进制资源交给 Git LFS 管理：

- 图片和设计源文件：`*.png`、`*.jpg`、`*.jpeg`、`*.psd`、`*.aseprite`。
- 音频：`*.wav`、`*.mp3`、`*.ogg`。
- 模型和源工程：`*.fbx`、`*.blend`。
- 视频：`*.mp4`、`*.mov`。

## 8. 游戏脚本详解

### `Assets/_Game/Scripts/Runtime/Core/SceneNames.cs`

职责：集中保存项目使用的场景名常量，避免在加载逻辑里散落字符串。

引用：

- 当前无 `using` 引用。

声明：

- `public static class SceneNames`

常量：

- `public const string MainMenuScene = "MainMenuScene"`
  - 主菜单场景名。
- `public const string GameScene = "GameScene"`
  - 游戏主场景名。

函数：

- 当前无函数。

### `Assets/_Game/Scripts/Runtime/Core/SceneLoader.cs`

职责：集中封装场景加载和退出游戏逻辑，供启动流程和 UI 控制器调用。

引用：

- `UnityEngine`
- `UnityEngine.SceneManagement`

声明：

- `public class SceneLoader : MonoBehaviour`

字段：

- 当前无显式字段。

函数：

- `public void LoadScene(string sceneName)`
  - 如果 `sceneName` 为空或空白，输出日志并返回。
  - 否则调用 `SceneManager.LoadScene(sceneName)` 同步加载指定名称的场景。
- `public void LoadMainMenu()`
  - 调用 `LoadScene(SceneNames.MainMenuScene)` 加载主菜单场景。
- `public void LoadGameScene()`
  - 调用 `LoadScene(SceneNames.GameScene)` 加载游戏主场景。
- `public void QuitGame()`
  - 输出退出日志。
  - 调用 `Application.Quit()` 退出应用。

### `Assets/_Game/Scripts/Runtime/Core/GameLayerType.cs`

职责：定义游戏当前所处的逻辑层，用于输入模式切换、玩家动作锁定和 UI 路由。

引用：

- 当前无 `using` 引用。

声明：

- `public enum GameLayerType`

枚举值：

- `None`：无有效层。
- `MainMenu`：主菜单层。
- `Gameplay`：正常游戏层。
- `Dialogue`：对话播放层。
- `DialogueChoice`：对话选项层。
- `Inventory`：背包层。
- `Map`：地图层。
- `ServiceMenu`：服务菜单层。
- `Pause`：暂停层。
- `Cutscene`：过场层。

### `Assets/_Game/Scripts/Runtime/Core/GameLayerStack.cs`

职责：维护游戏层栈，提供当前层查询、压栈、出栈、重置和层变化事件。

引用：

- `System`
- `System.Collections.Generic`
- `UnityEngine`

声明：

- `public class GameLayerStack : MonoBehaviour`

字段与属性：

- `private readonly Stack<GameLayerType> layerStack = new()`
  - 保存当前激活层，栈顶为当前层。
- `public GameLayerType CurrentLayer`
  - 如果栈为空返回 `GameLayerType.None`，否则返回栈顶。
- `public event Action<GameLayerType, GameLayerType> CurrentLayerChanged`
  - 当前层变化事件，参数为上一层和当前层。

函数：

- `public void ResetTo(GameLayerType layer)`
  - 清空层栈并把指定层作为唯一当前层。
  - 禁止设置 `None`，非法时输出警告。
  - 成功后通知层变化。
- `public void PushLayer(GameLayerType layer)`
  - 把新层压入栈顶。
  - 禁止 `None`，也禁止重复压入已经存在的层。
  - 成功后通知层变化。
- `public void PopLayer(GameLayerType layer)`
  - 只有当指定层就是当前层时才弹出。
  - 禁止 `None`，非法时输出警告。
  - 成功后通知层变化。
- `public bool IsCurrentLayer(GameLayerType layer)`
  - 判断指定层是否为当前层。
- `public bool ContainsLayer(GameLayerType layer)`
  - 判断指定层是否已在层栈中。
- `public GameLayerType[] GetActiveLayers()`
  - 返回当前层栈中的所有激活层。
- `private void NotifyLayerChanged(GameLayerType previousLayer, GameLayerType currentLayer)`
  - 输出层变化日志。
  - 触发 `CurrentLayerChanged` 事件。

### `Assets/_Game/Scripts/Runtime/Core/GameLayerRule.cs`

职责：作为可序列化规则项，描述某个游戏层会锁定哪些玩家动作。

引用：

- `System`

声明：

- `[Serializable] public class GameLayerRule`

字段：

- `public GameLayerType layerType`
  - 规则对应的游戏层。
- `public PlayerActionType lockedPlayerActions`
  - 该层锁定的玩家动作标记集合。

函数：

- 当前无函数。

### `Assets/_Game/Scripts/Runtime/Core/GameLayerRuleDatabase.cs`

职责：作为 ScriptableObject 数据库，按游戏层查询该层锁定的玩家动作。

引用：

- `System.Collections.Generic`
- `UnityEngine`

声明：

- `[CreateAssetMenu(fileName = "GameLayerRuleDatabase", menuName = "Game/Core/Game Layer Rule Database")]`
- `public class GameLayerRuleDatabase : ScriptableObject`

字段：

- `[SerializeField] private List<GameLayerRule> layerRules = new()`
  - Inspector 中配置的层规则列表。

函数：

- `public PlayerActionType GetLockedPlayerActions(GameLayerType layer)`
  - 遍历 `layerRules`，找到匹配 `layerType` 的规则。
  - 找到后返回该层的 `lockedPlayerActions`。
  - 找不到时输出警告，并返回 `PlayerActionType.None`。

关联资产：

- `Assets/_Game/Data/Core/GameLayerRuleDatabase.asset`
  - 当前记录了多个层的锁定动作。
  - `Gameplay` 当前锁定值为 `0`，代表不锁定玩家动作。
  - `Dialogue` 当前锁定值为 `6`，对应 `Jump` 和 `Attack`。
  - 多个 UI/菜单层使用 `-1`，代表所有标记位都被视为锁定。

### `Assets/_Game/Scripts/Runtime/GameFlow/GameBootstrap.cs`

职责：作为游戏启动流程入口，初始化启动对象上的核心组件，加载主菜单并设置初始层。

引用：

- `UnityEngine`

声明：

- `[RequireComponent(typeof(SceneLoader))]`
- `[RequireComponent(typeof(GameLayerStack))]`
- `public class GameBootstrap : MonoBehaviour`

字段：

- `[SerializeField] private SceneLoader sceneLoader`
  - 场景加载器引用，空时从同对象获取。
- `[SerializeField] private GameLayerStack layerStack`
  - 游戏层栈引用，空时从同对象获取。

函数：

- `private void Awake()`
  - 如果 `sceneLoader` 为空，调用 `GetComponent<SceneLoader>()` 获取。
  - 如果 `layerStack` 为空，调用 `GetComponent<GameLayerStack>()` 获取。
  - 调用 `DontDestroyOnLoad(gameObject)` 保留启动对象跨场景存在。
- `private void Start()`
  - 调用 `sceneLoader.LoadMainMenu()` 加载主菜单场景。
  - 调用 `layerStack.ResetTo(GameLayerType.MainMenu)` 设置当前层为主菜单。

### `Assets/_Game/Scripts/Runtime/UI/MainMenuController.cs`

职责：管理主菜单按钮事件，把 UI 点击转成新游戏、继续游戏和退出游戏行为。

引用：

- `UnityEngine.UI`
- `UnityEngine`

声明：

- `public class MainMenuController : MonoBehaviour`

字段：

- `[SerializeField] private Button newGameButton`
  - Inspector 绑定的新游戏按钮。
- `[SerializeField] private Button continueGameButton`
  - Inspector 绑定的继续游戏按钮。
- `[SerializeField] private Button quitGameButton`
  - Inspector 绑定的退出游戏按钮。
- `private SceneLoader sceneLoader`
  - 运行时查找到的场景加载器。
- `private GameLayerStack layerStack`
  - 运行时查找到的游戏层栈。

函数：

- `private void Awake()`
  - 调用 `FindAnyObjectByType<SceneLoader>()` 查找场景加载器。
  - 调用 `FindAnyObjectByType<GameLayerStack>()` 查找游戏层栈。
- `private void OnEnable()`
  - 给 `newGameButton.onClick` 添加 `OnNewGameClicked` 监听。
  - 给 `continueGameButton.onClick` 添加 `OnContinueGameClicked` 监听。
  - 给 `quitGameButton.onClick` 添加 `OnQuitGameClicked` 监听。
- `private void OnDisable()`
  - 移除 `OnEnable()` 中绑定的三个按钮监听，避免重复绑定。
- `private void OnNewGameClicked()`
  - 调用 `sceneLoader.LoadGameScene()` 进入 `GameScene`。
  - 调用 `layerStack.ResetTo(GameLayerType.Gameplay)` 设置当前层为游戏层。
- `private void OnContinueGameClicked()`
  - 当前只输出“存档系统待开发”类日志。
- `private void OnQuitGameClicked()`
  - 调用 `sceneLoader.QuitGame()` 触发退出游戏逻辑。

### `Assets/_Game/Scripts/Runtime/Input/GameInputReaders.cs`

职责：封装 Unity Input System 生成类，把 Input Action 回调转换成项目内部事件，并根据游戏层切换启用的 Action Map。

引用：

- `System`
- `UnityEngine`
- `UnityEngine.InputSystem`

声明：

- `public class GameInputReader : MonoBehaviour`

字段与属性：

- `private InputSystem_Actions inputActions`
  - Unity Input System 生成的输入包装类实例。
- `public Vector2 MoveInput { get; private set; }`
  - 当前玩家移动输入。
- `public Vector2 UINavigateInput { get; private set; }`
  - 当前 UI 导航输入。

事件：

- `public event Action<Vector2> MoveChanged`
- `public event Action JumpPressed`
- `public event Action AttackPressed`
- `public event Action DashPressed`
- `public event Action InteractPressed`
- `public event Action UseItemPressed`
- `public event Action PausePressed`
- `public event Action OpenInventoryPressed`
- `public event Action OpenMapPressed`
- `public event Action<Vector2> UINavigateChanged`
- `public event Action UISubmitPressed`
- `public event Action UICancelPressed`

函数：

- `private void Awake()`
  - 创建 `InputSystem_Actions` 实例。
  - 调用 `BindPlayerInput()`、`BindGameInput()`、`BindUIInput()` 绑定输入回调。
- `private void OnEnable()`
  - 默认启用 `Player`、`Game`、`UI` 三个 Action Map。
- `private void OnDisable()`
  - 禁用 `Player`、`Game`、`UI` 三个 Action Map。
- `public void SetInputMode(GameLayerType layerType)`
  - 先禁用全部 Action Map。
  - 根据当前游戏层启用对应输入模式。
- `private void OnDestroy()`
  - 调用 `inputActions.Dispose()` 释放输入资源。
- `private void BindPlayerInput()`
  - 绑定 `Move` 的 `performed` 和 `canceled`，更新 `MoveInput` 并触发 `MoveChanged`。
  - 绑定 `Jump`、`Attack`、`Dash`、`Interact`、`UseItem` 的 `performed`，触发对应事件。
- `private void BindGameInput()`
  - 绑定 `Pause`、`OpenInventory`、`OpenMap` 的 `performed`，触发对应事件。
- `private void BindUIInput()`
  - 绑定 `Navigate` 的 `performed` 和 `canceled`，更新 `UINavigateInput` 并触发 `UINavigateChanged`。
  - 绑定 `Submit`、`Cancel` 的 `performed`，触发对应事件。

### `Assets/_Game/Scripts/Runtime/Input/InputRouter.cs`

职责：订阅 `GameInputReader` 的输入事件，根据当前游戏层和玩家动作仲裁结果，把输入路由到玩家接收器、层栈或 UI/对话占位逻辑。

引用：

- `UnityEngine`

声明：

- `public class InputRouter : MonoBehaviour`

字段：

- `[SerializeField] private GameInputReader inputReader`
  - 输入事件来源。
- `[SerializeField] private GameLayerStack gameLayerStack`
  - 当前游戏层来源。
- `[SerializeField] private PlayerControlArbitration playerControlArbitration`
  - 判断玩家动作是否允许。
- `[SerializeField] private PlayerInputReceiver playerInputReceiver`
  - 玩家输入接收目标。

函数：

- `private void Awake()`
  - 如果引用为空，分别通过 `FindAnyObjectByType<T>()` 查找 `GameInputReader`、`GameLayerStack`、`PlayerControlArbitration`、`PlayerInputReceiver`。
- `private void OnEnable()`
  - 订阅 `gameLayerStack.CurrentLayerChanged`。
  - 订阅玩家输入、游戏输入和 UI 输入事件。
- `private void OnDisable()`
  - 取消输入事件订阅。
  - 当前代码中 `gameLayerStack.CurrentLayerChanged` 使用了 `+= OnCurrentLayerChanged`，这看起来像应为 `-=` 的解绑笔误。
- `private void OnCurrentLayerChanged(GameLayerType previousLayer, GameLayerType currentLayer)`
  - 调用 `inputReader.SetInputMode(currentLayer)`，按当前层切换启用的 Action Map。
- `private void OnMoveChanged(Vector2 moveInput)`
  - 仅在 `Gameplay` 且 `CanMove` 时调用 `playerInputReceiver.SetMoveInput(moveInput)`。
- `private void OnJumpPressed()`
  - 仅在 `Gameplay` 且 `CanJump` 时调用 `playerInputReceiver.RequestJump()`。
- `private void OnAttackPressed()`
  - 仅在 `Gameplay` 且 `CanAttack` 时调用 `playerInputReceiver.RequestAttack()`。
- `private void OnDashPressed()`
  - 仅在 `Gameplay` 且 `CanDash` 时调用 `playerInputReceiver.RequestDash()`。
- `private void OnInteractPressed()`
  - 在 `Gameplay` 且 `CanWorldInteract` 时调用 `playerInputReceiver.RequestWorldInteract()`。
  - 在 `Dialogue` 时输出路由到下一句对话的日志。
  - 在 `DialogueChoice` 时输出确认选项的日志。
  - 其他层输出忽略交互的日志。
- `private void OnUseItemPressed()`
  - 仅在 `Gameplay` 且 `CanUseItem` 时输出使用物品路由日志。
- `private void OnPausePressed()`
  - 在 `Gameplay` 时压入 `Pause` 层并输出暂停打开日志。
  - 在 `Pause` 时弹出 `Pause` 层并输出暂停关闭日志。
- `private void OnOpenInventoryPressed()`
  - 在 `Gameplay` 时压入 `Inventory` 层。
  - 在 `Inventory` 时弹出 `Inventory` 层。
- `private void OnOpenMapPressed()`
  - 在 `Gameplay` 时压入 `Map` 层。
  - 在 `Map` 时弹出 `Map` 层。
- `private void OnUINavigateChanged(Vector2 navigateInput)`
  - 根据当前 UI/菜单层输出对应导航日志。
- `private void OnUISubmitPressed()`
  - 根据当前 UI/菜单层输出确认日志。
- `private void OnUICancelPressed()`
  - 在 `Inventory`、`Map`、`ServiceMenu`、`Pause` 时弹出对应层。
  - 在 `DialogueChoice` 时输出取消对话选项日志。
- `private bool IsCurrentLayer(GameLayerType layerType)`
  - 判空后调用 `gameLayerStack.IsCurrentLayer(layerType)`。

### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerActionType.cs`

职责：定义可被层规则锁定的玩家动作标记。

引用：

- `System`

声明：

- `[Flags] public enum PlayerActionType`

枚举值：

- `None = 0`
- `Move = 1 << 0`
- `Jump = 1 << 1`
- `Attack = 1 << 2`
- `Dash = 1 << 3`
- `WorldInteract = 1 << 4`
- `UseItem = 1 << 5`

### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerControlArbitration.cs`

职责：根据当前所有激活游戏层的规则，判断玩家某个动作是否允许执行。

引用：

- `UnityEngine`

声明：

- `public class PlayerControlArbitration : MonoBehaviour`

字段与属性：

- `[SerializeField] private GameLayerStack layerStack`
  - 当前激活层来源。
- `[SerializeField] private GameLayerRuleDatabase gameLayerRuleDatabase`
  - 玩家动作锁定规则数据。
- `public bool CanMove => CanDo(PlayerActionType.Move)`
- `public bool CanJump => CanDo(PlayerActionType.Jump)`
- `public bool CanAttack => CanDo(PlayerActionType.Attack)`
- `public bool CanDash => CanDo(PlayerActionType.Dash)`
- `public bool CanWorldInteract => CanDo(PlayerActionType.WorldInteract)`
- `public bool CanUseItem => CanDo(PlayerActionType.UseItem)`

函数：

- `private void Awake()`
  - 调用 `FindAnyObjectByType<GameLayerStack>()` 查找层栈引用。
- `private bool CanDo(PlayerActionType actionType)`
  - 如果缺少 `layerStack` 或 `gameLayerRuleDatabase`，输出警告并返回 `false`。
  - 聚合所有激活层的锁定动作。
  - 如果锁定集合不包含当前动作，则返回 `true`。
- `private PlayerActionType GetAllPlayerActions()`
  - 调用 `layerStack.GetActiveLayers()` 获取全部激活层。
  - 对每个层调用 `gameLayerRuleDatabase.GetLockedPlayerActions(layer)`。
  - 使用按位或合并所有锁定动作并返回。

### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerInputReceiver.cs`

职责：作为玩家输入最终接收点，当前以 Debug 日志形式确认玩家动作请求。

引用：

- `UnityEngine`

声明：

- `public class PlayerInputReceiver : MonoBehaviour`

属性：

- `public Vector2 MoveInput { get; private set; }`
  - 当前缓存的玩家移动输入。

函数：

- `public void SetMoveInput(Vector2 moveInput)`
  - 更新 `MoveInput`。
  - 输出收到移动输入的日志。
- `public void RequestJump()`
  - 输出收到跳跃请求的日志。
- `public void RequestAttack()`
  - 输出收到攻击请求的日志。
- `public void RequestDash()`
  - 输出收到冲刺请求的日志。
- `public void RequestWorldInteract()`
  - 输出收到世界交互请求的日志。
- `public void ClearMoveInput()`
  - 把 `MoveInput` 重置为 `Vector2.zero`。
  - 输出移动输入清空日志。

## 9. 外部与生成内容说明

### `Assets/Settings/InputSystem_Actions.cs`

- 这是 Unity Input System 根据 `Assets/Settings/InputSystem_Actions.inputactions` 生成的 C# 包装代码。
- 它属于输入配置的生成产物，不作为当前游戏手写业务脚本逐函数展开。
- 如果输入动作表变化，该文件可能由 Unity 自动重新生成。

### `Assets/TextMesh Pro/`

- 包含 TextMesh Pro 的字体、材质、Shader、示例场景、示例脚本和示例贴图。
- `Assets/TextMesh Pro/Examples & Extras/Scripts/` 下有大量官方示例脚本。
- 这些脚本用于 TextMesh Pro 示例展示，不属于当前游戏核心逻辑，因此本文档只做概览，不逐函数追踪。

### `Library/PackageCache/`

- 这是 Unity 包缓存目录，包含包源码和编辑器工具代码。
- IDE 打开的 `Library/PackageCache/...` 文件属于 Unity 包缓存，不属于项目手写源码，不应提交，也不在本文档中详细追踪。

## 10. 当前注意事项

- `SceneLoader.QuitGame()`、`SceneLoader.LoadScene()` 的空场景名日志，以及 `MainMenuController.OnContinueGameClicked()` 中的中文日志在当前读取结果里显示为乱码，可能是文件编码或保存格式导致；本次只记录现状，不修改代码。
- `InputRouter.OnDisable()` 中 `gameLayerStack.CurrentLayerChanged += OnCurrentLayerChanged;` 疑似应为 `-=`，否则对象反复启用/禁用时层变化事件可能重复订阅。
- `GameLayerRuleDatabase.asset` 当前有两条 `layerType: 6` 规则，`GameLayerType` 中 `6` 对应 `Map`，可能是漏配了某个层或重复配置。
- `Game.Pause` 和 `UI.Cancel` 当前都可能由 `Escape` 触发，因此按下 `Escape` 时可能出现打开暂停后又立刻取消的现象，取决于当前启用的 Action Map。
- `MainMenuController` 依赖 Inspector 正确绑定 3 个按钮，否则 `OnEnable()` 绑定监听时可能出现空引用。
- `GameBootstrap` 依赖同一个 GameObject 上存在 `SceneLoader` 和 `GameLayerStack`，当前通过 `RequireComponent` 强化了这一点。
- `PlayerControlArbitration` 依赖 `gameLayerRuleDatabase` 在 Inspector 中正确绑定，否则所有玩家动作都会被锁定。

## 11. 更新规则

以后更新本文档时建议按以下顺序执行：

1. 重新扫描 `Assets/_Game/` 的目录和文件。
2. 重新读取 `Assets/_Game/Scripts/**/*.cs`。
3. 对每个游戏脚本更新引用、类声明、字段、函数和职责说明。
4. 检查 `ProjectSettings/EditorBuildSettings.asset` 的启用场景是否变化。
5. 检查 `Assets/Settings/InputSystem_Actions.inputactions` 的 Action Map、Action 和 Binding 是否变化。
6. 检查 `Assets/_Game/Data/Core/GameLayerRuleDatabase.asset` 的层规则是否变化。
7. 检查 `Packages/manifest.json` 是否新增或移除关键依赖。
8. 检查 `.gitignore` 和 `.gitattributes` 是否变化。
