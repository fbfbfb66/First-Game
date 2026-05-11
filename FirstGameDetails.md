# First Game Project Details

本文档用于手动追踪当前 Unity 项目的结构、核心文件和游戏脚本职责。它是当前项目快照：当目录、场景、脚本或包配置变化后，需要重新扫描项目并更新本文档。

## 1. 项目概览

- 项目类型：Unity 2D 游戏项目。
- 主要游戏内容目录：`Assets/_Game/`。
- 当前场景流：
  - `BootScene` 作为启动场景。
  - `GameBootstrap` 在启动后通过 `SceneLoader` 进入 `MainMenuScene`。
  - 主菜单按钮通过 `MainMenuController` 进入 `GameScene` 或退出游戏。
- 当前核心脚本数量：3 个，全部位于 `Assets/_Game/Scripts/Runtime/`。
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
├─ Data/                         游戏数据资源目录，当前仅有 .meta
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
│     │  └─ SceneLoader.cs       场景加载与退出游戏
│     ├─ GameFlow/
│     │  └─ GameBootstrap.cs     游戏启动流程
│     ├─ GamePlay/               玩法脚本目录，当前无脚本
│     ├─ Input/                  输入脚本目录，当前无脚本
│     ├─ Systems/                系统脚本目录，当前无脚本
│     └─ UI/
│        └─ MainMenuController.cs 主菜单按钮控制
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

当前脚本假设场景名分别为 `MainMenuScene` 和 `GameScene`，并通过 `SceneManager.LoadScene` 按名称加载。

## 5. 包与技术栈

主要依赖来自 `Packages/manifest.json`：

- Unity 2D 工具链：
  - `com.unity.2d.animation`
  - `com.unity.2d.aseprite`
  - `com.unity.2d.psdimporter`
  - `com.unity.2d.sprite`
  - `com.unity.2d.spriteshape`
  - `com.unity.2d.tilemap`
  - `com.unity.2d.tilemap.extras`
  - `com.unity.2d.tooling`
- 渲染：
  - `com.unity.render-pipelines.universal`，项目使用 URP。
- 输入：
  - `com.unity.inputsystem`，项目包含生成文件 `Assets/Settings/InputSystem_Actions.cs`。
- UI：
  - `com.unity.ugui`，当前主菜单脚本使用 `UnityEngine.UI.Button`。
  - TextMesh Pro 资源位于 `Assets/TextMesh Pro/`。
- 编辑器与工作流：
  - `com.unity.ide.visualstudio`
  - `com.unity.ide.rider`
  - `com.unity.collab-proxy`
- AI 相关：
  - `com.unity.ai.assistant`
  - `com.unity.ai.inference`
- 测试与流程：
  - `com.unity.test-framework`
  - `com.unity.timeline`
  - `com.unity.visualscripting`

## 6. Git 配置

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

## 7. 游戏脚本详解

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
  - 使用 `SceneManager.LoadScene(sceneName)` 同步加载指定名称的场景。
  - 这是本类的通用加载入口，其他具体加载函数都调用它。
- `public void LoadMainMenue()`
  - 调用 `LoadScene("MainMenuScene")` 加载主菜单场景。
  - 当前函数名拼写为 `LoadMainMenue`，文档保留当前代码状态。
- `public void LoadGameScene()`
  - 调用 `LoadScene("GameScene")` 加载游戏主场景。
- `public void QuitGame()`
  - 先通过 `Debug.Log(...)` 输出退出日志。
  - 再调用 `Application.Quit()` 退出应用。
  - 在 Unity 编辑器里 `Application.Quit()` 通常不会关闭编辑器播放模式，构建后才会真正退出应用。

### `Assets/_Game/Scripts/Runtime/GameFlow/GameBootstrap.cs`

职责：作为游戏启动流程入口，初始化场景加载器引用，并在游戏开始后进入主菜单。

引用：

- `UnityEngine`

声明：

- `public class GameBootstrap : MonoBehaviour`

字段：

- `[SerializeField] private SceneLoader sceneLoader`
  - 在 Inspector 中可见的私有字段。
  - 运行时会在 `Awake()` 中被同对象上的 `SceneLoader` 组件覆盖赋值。

函数：

- `private void Awake()`
  - 调用 `GetComponent<SceneLoader>()` 获取同一个 GameObject 上的 `SceneLoader`。
  - 调用 `DontDestroyOnLoad(gameObject)`，让启动对象跨场景保留。
- `private void Start()`
  - 调用 `sceneLoader.LoadMainMenue()` 加载主菜单场景。
  - 当前依赖 `SceneLoader` 组件存在于同一个 GameObject 上。

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
  - 运行时缓存的场景加载器引用。

函数：

- `private void Awake()`
  - 调用 `FindAnyObjectByType<SceneLoader>()` 在当前已加载场景对象中查找 `SceneLoader`。
  - 该引用用于后续加载游戏场景或退出游戏。
- `private void OnEnable()`
  - 给 `newGameButton.onClick` 添加 `OnNewGameClicked` 监听。
  - 给 `continueGameButton.onClick` 添加 `OnContinueGameClicked` 监听。
  - 给 `quitGameButton.onClick` 添加 `OnQuitGameClicked` 监听。
- `private void OnDisable()`
  - 移除 `OnEnable()` 中绑定的三个按钮监听，避免对象反复启用时重复绑定。
- `private void OnNewGameClicked()`
  - 调用 `sceneLoader.LoadGameScene()` 进入 `GameScene`。
- `private void OnContinueGameClicked()`
  - 当前仅输出日志，表达“存档系统待开发”这一占位行为。
  - 当前源码中的中文日志存在编码显示异常，文档只记录行为，不修改源码。
- `private void OnQuitGameClicked()`
  - 调用 `sceneLoader.QuitGame()` 触发退出游戏逻辑。

## 8. 外部与生成内容说明

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
- 当前 IDE 打开的 `Library/PackageCache/com.unity.ai.assistant.../ApiAccessibleState.cs` 属于 Unity 包缓存，不属于项目手写源码，不应提交，也不在本文档中详细追踪。

## 9. 当前注意事项

- `SceneLoader.LoadMainMenue()` 当前函数名拼写为 `Menue`，和常见英文 `Menu` 不一致；本次只记录现状，不修改代码。
- `SceneLoader.QuitGame()` 和 `MainMenuController.OnContinueGameClicked()` 中的中文日志在当前读取结果里显示为乱码，可能是文件编码或保存格式导致；本次只记录现状，不修改代码。
- `MainMenuController` 依赖 Inspector 正确绑定 3 个按钮，否则 `OnEnable()` 绑定监听时可能出现空引用。
- `GameBootstrap` 依赖同一个 GameObject 上存在 `SceneLoader` 组件，否则 `Start()` 调用加载主菜单时可能出现空引用。
- 当前 `_Game` 下多个资源目录只有 `.meta` 文件，说明目录已建立但资源内容尚未填充。

## 10. 更新规则

以后更新本文档时建议按以下顺序执行：

1. 重新扫描 `Assets/_Game/` 的目录和文件。
2. 重新读取 `Assets/_Game/Scripts/**/*.cs`。
3. 对每个游戏脚本更新引用、类声明、字段、函数和职责说明。
4. 检查 `ProjectSettings/EditorBuildSettings.asset` 的启用场景是否变化。
5. 检查 `Packages/manifest.json` 是否新增或移除关键依赖。
6. 检查 `.gitignore` 和 `.gitattributes` 是否变化。
