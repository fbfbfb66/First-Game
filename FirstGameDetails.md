# First Game Project Details

本文档用于记录当前 Unity 项目的结构、核心系统、资源变化、脚本职责、函数职责、脚本之间的关系和 Git 提交规则。项目结构、脚本职责、函数、输入绑定、场景、资源或 ScriptableObject 发生变化后，应同步更新本文档。

最后更新时间：2026-11-01。

## 1. 项目概览

- 项目类型：Unity 2D 游戏项目。
- 主要游戏内容目录：`Assets/_Game/`。
- 当前分支：`main`。
- 远程仓库：`https://github.com/fbfbfb66/First-Game.git`。
- 当前核心系统：
  - 场景加载：`SceneLoader` + `SceneNames` + `GameBootstrap`。
  - 游戏层管理：`GameLayerStack` + `GameLayerRuleDatabase` + `GameLayerRule` + `GameLayerType`。
  - 输入读取与路由：`GameInputReader` + `InputRouter`。
  - 玩家输入、移动与状态机：`PlayerCommandInput`（持续方向）+ Motion / Combat 的 ActionRequest（一次性请求）+ `PlayerMovement` + `Player` + `StateMachine` + 玩家状态类。
  - 世界交互：`IInteractable` + `InteractionContext` + `InteractionDetector` + `InteractionPrompt`。
  - 世界物品拾取：`WorldItem` → `PlayerInventory.TryAdd()` → `ItemPlaced` → `InventoryView`。
  - 对话系统：`DialogueManager` + `DialogueData` + `NPCDialogueProfile` + `ConditionalDialogueEntry` + `WorldDialogueView` + `WorldDialogueChoiceView`。
  - 事件总线：`GameEventBus` + `IGameEvent` + `GameSignalEvent` + `GameFlagChangedEvent` + `QuestStateChangedEvent`。
  - Flag 与条件：`GameFlagCenter` + `GameFlagDatabase` + `GameFlagData` + `GameCondition` + `FlagBoolCondition`。
  - Quest 系统：`QuestManager` + `QuestDatabase` + `QuestData` + `QuestState` + `QuestStateCondition`。
  - 剧情序列：`StoryTrigger` + `StorySequenceRunner` + `StorySequence` + `StoryStepAction` + `StorySceneBindings` + `StoryCameraDirector` + `StoryContext`。
  - 背包系统：数据层 `ItemData` + `ItemCategory` + `InventoryItem` + `InventoryGrid` + `PlayerInventory`；表现层 `InventoryView` + `InventoryPointerHandler` + `ItemView` + `ItemDetailPanel`。已打通「显示 → 悬停高亮 → 拖拽 → 旋转 → 落格改数据」与「左键选中 → 选中框 + 常驻详情面板 → 按数量丢弃 → 移除数据并销毁显示对象」。
  - Falling Sand：已完成 CPU Grid 数据、逐 Tick 重力/斜滑模拟、动态 Texture 渲染，以及首次触底时按外圈/内部概率执行的一次性撞击破碎；当前高处沙块仍是临时测试入口。
  - 其他 UI：`UI_HPBarView`、`UI_MPBarView` 继承可配置的 `UI_ResourceBarView` 并重写比例更新入口；`UIPolygonMask` 借用 `PolygonCollider2D` 原生轮廓编辑工具生成 UI 遮罩。

### 本阶段状态：动作管理共用父类，玩家 / 敌人分支已拆分（2026-10-31）

- 请求：Gameplay 的 Jump / Dash / Interact 提交到 PlayerMotionController，Attack / Launcher / Dive 提交到 PlayerCombatActionController；两个玩家子类分别持有独立的 PlayerActionRequestBuffer，后一次输入覆盖前一次。ActionController / MotionController / CombatActionController 不再持有输入缓存或缓冲配置。EnemyMotionController 接收 AI 直接指定的移动动作，Enemy 持有的 CombatActionController.TryStartAction 接收直接指定的攻击，不经过请求有效期判断。Move 使用 PlayerCommandInput 持续方向缓存；Dialogue 层交互键仍直接推进对话。
- 玩家许可：目标 Controller 持有请求，Player / 当前执行状态把目标 Command 交给源 Controller.AllowsCancel 查询。无活动 Action 允许尝试；有活动 Action 必须由当前 Data 的活动取消窗口接受该 Command。查询不复制、不消费请求，目标入口条件通过后由请求持有方消费。
- 执行：FSM Exit / Enter 管理状态生命周期。Motion 的 BeginState / EndState 登记当前动作、选中的 Stage、动画身份并清理窗口；Combat 保留攻击链、Hit Window、派生及结束运行时。玩家 Action 的入场速度读取 PlayerActionStage / PlayerCombatActionStage 的 EntryVelocityRule，CombatActionStage 保存玩家战斗水平减速规则；AI 的 Combat/Motion 阶段读取 AIActionStage / AICombatActionStage 的 AIDisplacement（距离/时长/曲线），由 AIDisplacementMotor 执行；普通移动、未 Action 化的 Jump/WallJump 和重力仍读取 PlayerBaseConfig。
- 条件：ActionDefinition 持有 ActionCondition 数组。技能 Condition 指定 SkillData，通过 Context 查询当前角色技能；Context 不保存二段跳解锁专用布尔值。Dash / 二段跳次数仍由 Player 跨状态持有，真正进入时标记使用，落地和墙滑时刷新。
- 动画与清理：PlayerMotionAnimationTrigger 负责普通动画结束、起跳时机及 Motion 取消窗口；PlayerCombatAnimationTrigger 只负责战斗事件。命中效果在窗口关闭、动作取消/完成及阶段切换时同步结束，先恢复旧效果，再由新动作设置速度和重力。
- 前一阶段已验证的行为（本次未全部重跑）：左右转向 Dash 速率与位移一致；Dash→普通跳/二段跳/攻击、攻击→Dash；真实空中攻击命中暂停后取消 Dash、落地再跳跃；普通关窗和空中连段；站立/跑步拾取、NPC 对话启动与翻页、交互请求过期。临时测试对象和技能解锁仅存在于 Play Mode，未更改初始资源配置。
- 当前边界：普通跳、墙跳、攀爬仍是消费 Command 后切换既有状态，没有独立动作 Data；Move Command 资源保留但未接入提交/消费。二段跳和墙滑 Data 尚无取消窗口，执行期间拒绝输入取消；自然结束、落地和离墙不受此限制。通用动作 Effect 尚未实现，当前命中 Effect 属于 Combat。
- 资源：Data/Actions 下分 Combat / Motions，目录与资源迁移保留原 GUID，ActionCommandKey 脚本 GUID 变化对应的 Command 脚本引用已修复；旧 PlayerInputReceiver、PlayerState 窗口名单和 StateCancelTransitionRule 已移除。
- 本次分支拆分验证：Unity 编译通过，Play Mode 临时对象的 45 项检查通过，覆盖玩家缓存匹配 / 保留 / 消费、Motion 阶段准备与 FSM 登记、取消窗口、敌人无缓存启动、Combat 命中 / 手动与自动衔接 / 提交 / 完成及清理回调。另用场景玩家验证真实经过 11.04 游戏秒后 Dash 请求过期清空、攻击请求进入 BaseAttack_01_Normal 后自动完成；Dash 通过同帧提交请求并调用当前 Idle.LogicalUpdate 的受控检查，消费输入、进入 Dash，速度为 (15, 0)，结束后重力恢复为 4。场景检查 Missing Scripts 为 0，两个玩家子类保留 0.1 / 0.3 秒配置。未重复完整命中伤害、拾取 / 对话及所有空中连段回归；未实现敌人 AI 与命中检测接入。

### 世界时间与曲线恢复

- 脚本目录：`Assets/_Game/Scripts/Runtime/Systems/WorldTime/`。`WorldTime` 保存自定义 `TimeScale`（限制在 0～1），提供乘以该倍率的 `deltaTime` / `fixedDeltaTime`；不修改 Unity 的 `Time.timeScale`。
- `WorldTimeController` 挂在 GameScene 的 `GameScene` 对象上，持有单例和恢复计时。`RequestSlowMotion(float targetScale, float duration)` 立即设置倍率，保存实际起始倍率、总时长和剩余时长；`duration` 表示恢复过程的现实秒数，不包含独立保持阶段。恢复中再次请求会覆盖起始倍率和计时。
- `Update()` 用 `Time.unscaledDeltaTime` 扣减剩余时间，以 `1 - remainingDuration / totalDuration` 得到 0～1 进度；序列化字段 `recoveryCurve`（默认 EaseInOut，从 `(0,0)` 到 `(1,1)`）通过 `Evaluate` 输出恢复程度，再用 `Mathf.Lerp(startScale, 1f, recoveryAmount)` 写入倍率。空曲线回退为线性恢复，插值程度限制在 0～1。到期或单例销毁时，`ResetWorldTime()` 清空计时并准确恢复倍率为 1。
- `WorldTimeAnimator` 缓存 Animator 原始速度，每帧设置为原始速度乘世界倍率，禁用时恢复原始速度。其他移动、物理和战斗逻辑需要各自接入，自定义倍率不会自动作用于整个游戏。
- Play Mode 测试入口：`WorldTimeController` 组件菜单 `Test Slow Motion`，请求倍率 `0.2`、恢复时长 `4` 秒。预期动画立即变慢，随后沿曲线逐渐恢复，到期输出一次恢复日志；该入口用于验证恢复链路。

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

### 在另一台电脑继续开发

- 安装 Git、Git LFS 和 Unity Hub，并通过 Hub 安装 `ProjectSettings/ProjectVersion.txt` 指定的 Unity `6000.4.6f1`。
- 首次获取项目：执行 `git lfs install`、`git clone https://github.com/fbfbfb66/First-Game.git`，进入 `First-Game` 后执行 `git lfs pull`。
- 已有克隆：先保存并提交当地改动，再执行 `git pull --ff-only origin main` 和 `git lfs pull`。
- 在 Unity Hub 的 Projects 中添加克隆目录，等待资源导入与 Package Manager 恢复依赖，然后打开 `Assets/_Game/Scenes/GameScene.unity` 检查 Console 并进入 Play Mode。
- `Library/` 等缓存由 Unity 重建；`.codex-remote-attachments/` 为本机聊天附件，不属于游戏资源，不提交。

### 当前地图与物品资源

- 秋季水井与工坊素材（本批上传顺序）：`Assets/_Game/Art/Environment/Props/Prop_Autumn_Well.png`（带顶棚的石砌水井，1366×1151）、`Assets/_Game/Art/Environment/Buildings/Building_Autumn_CarpenterWorkshop.png`（木工作坊，1671×941）、`Assets/_Game/Art/Environment/Buildings/Building_Autumn_FruitShop.png`（水果商店，1672×941）。三张原件均为带有假棋盘格背景的 JPG，已逐张使用原图作为图像编辑目标去底并另存为真实透明 PNG；灰底检查包括水井支架、水桶提手、工具架与工作台下方等镂空区域。导入配置为 Single Sprite / Full Rect、100 PPU、Point 过滤、无压缩，未绑定场景；上传原件仍保留在聊天附件目录。

- 秋季单体建筑位于 `Assets/_Game/Art/Environment/Buildings/`，按本次上传顺序命名：`Building_Autumn_CottagePorch.png`（带左侧门廊的小屋）、`Building_Autumn_Hall.png`（双旗帜大厅）、`Building_Autumn_ShopAwning.png`（条纹遮阳棚商店）、`Building_Autumn_CottageWide.png`（宽屋檐小屋）。第一张上传原件为 JPG，棋盘格属于实际图像背景，已使用原图作为编辑目标去底并另存为透明 PNG（输出 1536×1024）；后三张原件已有真实透明通道，直接复制保留原始像素与 alpha。四张均经灰底合成检查，按 Single Sprite / Full Rect、100 PPU、Point 过滤、无压缩导入，尚未切片或绑定场景。

- 秋季树木透明素材（按上传顺序）：`Assets/_Game/Art/Environment/Trees/Autumn/Tree_Autumn_RoundCanopy.png`（圆叶树冠）、`Tree_Autumn_Tall.png`（高树）、`Tree_Autumn_Curved.png`（弯曲树干）、`Tree_Autumn_Wide.png`（宽树冠）。四张原图均已有透明通道，已通过灰底合成检查；保留原始 PNG，按 Single Sprite / Full Rect、Point 过滤、无压缩导入，尚未绑定场景。

- UI 装备栏素材：`Assets/_Game/Art/UI/Equipment/EquipmentSlotsAndIcons.png`，包含普通框、红色选中框，以及剑、护甲、靴子、手套、项链和戒指图标；保留上传原图，按 Single Sprite 导入，尚未切分或绑定场景。

- `Assets/_Game/TileMaps/GroundPhysics.prefab` 保存 Grid 与地面 Tilemap 配置；`TileMaps/TileSet/` 保存 oak woods Tile 资源及对应 `.meta`。
- `Assets/_Game/Art/Environment/oak_woods_v1.0/` 作为完整素材包保留角色、背景、装饰、tileset 与来源说明的内部结构；独立建筑图集归入 `Art/Environment/Buildings/`，秋林树木归入 `Art/Environment/Trees/Autumn/`，二段跳技能石归入 `Art/Items/SkillStones/`。
- 物品预制体位于 `Assets/_Game/Prefabs/Items/GreenHerb.prefab`、`Assets/_Game/Prefabs/Items/ItemView.prefab` 和 `Assets/_Game/Prefabs/KuroganeKatana.prefab`；旧的根目录 `Item.prefab` 已移除，`ItemView` 移动时保留原 GUID。
- `Assets/_Game/Data/Skills/Skill_DoubleJump.asset` 的显示名为“二段跳”。

## 3. 根目录结构

当前补充资源：

- BackDodge 动画资源为 `Assets/_Game/Animation/AnimationStates/Player/Player_BackDodge_Normal.anim` 与 `Player_BackDodge_Perfect.anim`，Animator 状态名为 `BackDodge_Normal` / `BackDodge_Perfect`。`Action_BackDodge.asset` 已配置 Command_BackDodge、Negative Y Input 与现有 Condition_RequireGrounded 入口条件，以及上述两个候选 Stage（Normal 在前）。两个 Stage 的选择条件为空，目前选择首个 Normal，Perfect 选择规则尚未添加；二者取消窗口 0 均仅接受 Dash。两段 Clip 已关闭循环，0 秒调用 StartAnimation / OpenMotionCancelWindow(0)，0.6857143 秒调用 CloseMotionCancelWindow(0) / EndAnimation。BackDodge 已接入地面系 FSM，执行与落地规则见 Player_BackDodge。
- `Assets/_Game/Materials/NoFriction.physicsMaterial2D` 由 `New Physics Material 2D` 重命名，保留原 GUID；摩擦和弹性均为 0，既有引用保持有效。
- 根目录 `Artifacts/` 保存五张背包与角色属性面板的界面截图，作为布局参考，通过 Git LFS 管理 PNG。

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
|   |-- Environment/    Buildings、Props、Trees/Autumn、oak_woods_v1.0 完整素材包
|   |-- Items/          Icons、SkillStones
|   |-- NPC/            NPC 动作序列图
|   |-- Player/         按动作保留 Aseprite 源文件与 sprite sheets
|   `-- UI/             HUD、Panels、Buttons、Inventory、Equipment
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

### `GameScene` 场景环境容器

- Hierarchy 根节点 `Environment`：收纳场景环境，其中 `Forest` 挂载 `ParallaxController`，通过 `Main Camera` 的水平位移带动 `F_S1`、`F_S2`、`F_S3`，对应系数为 0.5、0.7、0.9。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Environment/ParallaxController.cs`

- 原名 `ParallaxControl.cs`，文件与类统一改为 `ParallaxController`，保留原 `.meta` GUID 和已有相机、背景引用。
- `Awake()` 保存相机初始 X；`LateUpdate()` 计算相机 X 增量并调用 `Backgrounds.Move(float dx)`，各层以 `dx * factor` 更新世界位置。
- 执行顺序：当前已设置 `[DefaultExecutionOrder(200)]`，排在 `CinemachineBrain` 的 100 之后。此前未指定顺序时，Play Mode 采样曾发现视差使用上一帧相机位置。
- `Backgrounds.Move(float dx)` 当前使用 `Vector2` 暂存世界坐标，首次非零移动会把 Z 清零；当前三层背景的世界 Z 原本均为 0，因此不是本次跳动的已证实原因。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Environment/BackgroundLoopController.cs`

- 分别挂载于 `GameScene/Environment/Forest/F_S1`、`F_S2`、`F_S3`，各自负责本层水平循环；Forest 根节点只保留 `ParallaxController` 负责三层连续视差，不再挂载整体循环组件。各层视差系数不同，必须独立判断和瞬移，避免长距离移动后相对位置累积导致露边。
- Inspector 引用：三个组件的 `targetCamera` 均为 `Main Camera`；`middleImage` 分别拖入本层中间图片 `F_S1/background_layer_3_0`、`F_S2/background_layer_2_0`、`F_S3/background_layer_1_0` 的 SpriteRenderer；场景中三个 offset 均为 1.5 世界单位。
- `private void LateUpdate()` 由 Unity 每帧调用，执行顺序 300，在相机 100、视差 200 之后执行。每帧从 `middleImage.bounds` 读取世界宽度 w 和边界；左触发边界为 `min.x + offset`，右触发边界为 `max.x - offset`。相机半宽为 `orthographicSize * aspect`，适用于当前无旋转的正交 2D 相机。
- 相机左边界到达或越过本层中间图片右触发边界时，该层根节点向右瞬移 w；相机右边界到达或越过本层中间图片左触发边界时，向左瞬移 w。大幅越界时一次补齐整数个 w，只改变本层世界 X，不影响其他层。
- 引用缺失、非正交相机、中间图片不属于当前组件所在层级，或两倍 offset 不小于图片宽度/相机视野宽度时，输出一次含对象和原因的警告并跳过。`private static bool IsLoopRangeValid(float imageWidth, float inset, float cameraHalfWidth)` 由运行逻辑和 Gizmos 共用：要求图片左右触发边界不交叉，并保证瞬移后相反触发条件仍有 `相机视野宽度 - 2 * offset` 的正间隔。不再要求收缩后的图片范围能容纳整个视野。
- `private void OnDrawGizmosSelected()`：选中 F_S1、F_S2 或 F_S3 并开启 Scene 窗口 Gizmos 时，黄色线框显示本层中间图片原始范围，绿色竖线显示 offset 向内收缩后的左右触发边界，横向短线显示收缩距离，标签显示 offset 数值；配置无效时触发线变红。编辑和运行时均可查看，不修改运行数据。
- 验证：Play Mode 中受控调用实际组件 `LateUpdate()`，offset = 0.02 和修正后的 offset = 1.5 均通过视野内、右边界前、右边界对齐、左边界前、左边界对齐、跨多张图片共 6 个用例，检查移动距离、再次调用不重复跳、Y/Z 不变。另确认 offset 达到相机半宽或图片半宽时被共用校验拒绝。当前 w = 24.27328，offset = 1.5 时触发线为 X = -10.63664 / 10.63664（各层初始位置）。
- 长距离回归：按实际相机位移依次调用视差和循环组件，路线为起点→右500→左1000→右500，每0.5世界单位检查各层三张图片组成的连续区间是否覆盖整个相机视野。旧整体循环在4000个采样中，F_S2/F_S3分别有1943/2988次覆盖不足；改为三层独立循环后，编辑态及Play Mode均为三层0次覆盖不足。验证后恢复位置/相机缓存并删除临时验证脚本。这是受控调用和几何覆盖验证，未验证纹理接缝的逐帧视觉效果。

### `GameScene` 背包 UI 结构

```text
Canvas_Inventory (Screen Space - Overlay)
|-- BackgroundDim                       <- 黑色半透明遮罩
|-- BackGround
|-- InventoryPart
|   |-- InventoryBG
|   |-- CharacterEquipment              <- 静态角色预览，左右各 3 个装备槽
|   |   |-- CharacterPortrait (RawImage) <- idle.png 第一帧，不驱动角色动画
|   |   `-- Equipment*Slot / Frame / Icon
|   |-- Inventory                        <- InventoryView
|   |   `-- InventoryArea (ScrollRect)
|   |       `-- Viewport (Mask)          <- 裁剪来源
|   |           `-- Content              <- GridLayoutGroup (Cell 156, Spacing 12，固定 4 列) + ContentSizeFitter
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
|-- CharacterStatsPanel                  <- 右侧半透明黑灰属性面板
|   `-- Health / Attack / Defense / CriticalChance / CriticalDamage / BaseMana Row
`-- DragLayer                            <- 拖拽期间物品的临时父节点
```

要点：

- 右侧 `CharacterStatsPanel` 使用双向拉伸，左偏移为 1354，右、上、下偏移均为 0，背景与边框贴满屏幕右边及上下边缘；使用 Alpha 0.8 的黑灰底板，含生命值、攻击力、防御、暴击率、暴击伤害和基础法力六行。`UI_CharacterStatsPanel` 已绑定 Player 与六行的 `Value` 文本：生命值显示 `Player.Health.CurrentHealth/Player.Stats.MaxHealth.Value`，其余读取运行时属性的 `Value`；暴击率及暴击伤害倍率乘 100 后显示百分号。打开时刷新，显示期间检查变化并仅重写变化的文字。当前项目没有运行时当前法力或等级字段；基础法力显示 `Stats.BaseMana.Value`，不能把 HUD MP 图形比例当作角色法力数据。
- 背包 Canvas 使用 `Scale With Screen Size`，参考分辨率 1920×1080、宽高匹配 0.5。主体宽 1330，贴齐屏幕左边并通过垂直拉伸填满上下，右侧保留空白；左侧装备与背包区宽 800，右侧物品简介宽 514，中间留 16 间隔。黑灰底板配灰色细框、暗红选中框。
- `EquipmentSlotsAndIcons.png` 已切成灰框、红色选中框与剑/胸甲/手套/项链/戒指/靴子六个图标。装备槽为 128×128，图标区域 80×84，左右各三个，行间距 16；装备区尺寸 800×416。装备槽目前仅作视觉展示，不包含装备逻辑；角色通过 334×400 的 `RawImage` 与 UV (0, 0, 0.1, 1) 显示现有待机图集第一帧。
- 保留 4×8 共 32 个背包格；可视区域 660×492，显示 3 行，其余行通过滚轮浏览。`InventoryView` 的序列化 `cellSize=156`、`spacing=12` 与布局组件一致，预览偏移归零；交互脚本未因本次美术调整而修改。
- 静态格子使用灰黑底色与独立灰框子节点；`ItemView.prefab` 使用 `InventoryCharcoal.asset` 中的纯色 Sprite 作为底板、独立灰框子节点作为边缘，兼容现有拖拽时隐藏底板的行为。框线使用 Sliced，`Pixels Per Unit Multiplier=4`。
- 物品简介与按钮文字的旧负边距已清零，数量加减按钮使用文本符号；选中框和拖放预览框均引用装备资产中的红框。
- `ItemLayer` 是 `Content` 的**最后一个子物体**，因此在渲染与射线顺序上位于所有 Slot 之上。
- `ItemLayer` 挂 `LayoutElement`（勾选 `Ignore Layout`），否则会被 `GridLayoutGroup` 当成第 33 个格子摆走。
- `ItemLayer` 的 anchor/pivot 均为 (0, 1)，尺寸为 660×1332，与 Content 的 4×8 网格范围一致；挂一个 `Alpha = 0` 且开启 `Raycast Target` 的 `Image` 作为隐形鼠标热区。**pivot 一旦被改动，`TryGetCellAt` 的换算结果会整体偏移。**
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

- `Player` Action Map：移动、跳跃、普通攻击、上挑攻击（`LauncherAttack`，绑定 <Keyboard>/e）、冲刺、交互、使用物品。
- `Game` Action Map：暂停、打开背包、打开地图。
- `UI` Action Map：导航、确认、取消、旋转物品（`RotateItem`，R）、使用所选物品（`UseItem`，F）、丢弃所选物品（`DropItem`，Q）。背包打开时 `SetInputMode` 只启用 `UI`；`Player/UseItem` 的 Q 是独立的 Gameplay 入口。

`GameInputReader.SetInputMode(GameLayerType layerType)` 会按当前游戏层启用对应 Action Map：

- `Gameplay`：启用 `Player` 和 `Game`。
- 菜单和 UI 层：启用 `UI`。
- `Dialogue`：启用 `Player`。
- `Cutscene`：关闭输入。

`InputRouter` 负责把输入派发到当前层：

- `Gameplay` 的移动输入进入 Player 持有的 `PlayerCommandInput.SetMoveInput()`，各移动状态持续读取同一个 MoveInput，不消费。
- `Gameplay` 的普通攻击与上挑攻击会被翻译为各自的 `ActionCommandKey` 并提交给战斗 Action Controller；Interact、Jump 和 Dash 提交给 Motion Controller，Attack 提交给 Combat Controller。
- 玩家地面状态中消费交互请求，并调用 `InteractionDetector.TryInteract()`。
- `Dialogue` 的交互输入调用 `DialogueManager.RequestAdvance()`。
- `DialogueChoice` 的导航与确认输入调用 `DialogueManager.HandleChoiceSelectedNavigate()` 和 `DialogueManager.HandleChoiceConfirmed()`。
- `Inventory` 的旋转输入调用 `InventoryView.RotateDragItem()`。Router 只判断「当前是不是背包层」，**不判断玩家有没有在拖东西**——`dragItem` 是 `InventoryView` 的私有状态，Router 不该知道；它是分发器，不是决策者。`inventoryView` 字段在 `Awake` 用 `FindAnyObjectByType<InventoryView>(FindObjectsInactive.Include)` 兜底，`Include` 不可省——背包 Canvas 默认是关闭的。

## 7. 全脚本与函数详解

本节按脚本路径记录所有当前游戏脚本。每个条目包含脚本职责、关键字段或属性、函数职责，以及与其他脚本的关联。

### Tool

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/TimeTool.cs`

- 普通 C# 实例，由 Player 持有，不再挂载组件或启动协程。
- `StartTimeCounter(float time, string timerId)` 保存 `Time.time + time` 到期时间，同名调用覆盖旧窗口；`IsRunning(string timerId)` 查询并清除到期项。使用缩放游戏时间，暂停时窗口不前进。
- `Player_RunState` 使用 RunBuffer / RunEndEarly 两个 ID 控制松手缓冲和过早结束窗口；状态不再读取协程字典。零时长立即视为结束，负数及非有限时长被拒绝。

#### `Assets/_Game/Scripts/Tool/GroundSensor.cs`

- 脚本职责：用角色脚底左、中、右三条 2D 射线检测角色与地面的接触程度，并记录最近一次接地时间，支持当前接地、稳定落地与 Coyote Time 查询。
- 关键字段：
  - `point`、`pointCenter`、`point2`：左、中、右三条射线的发射点。
  - `whatIsGround`：地面 LayerMask。
  - `distance`：检测距离。
  - `lastGroundedTime`：最近一次检测到接地时的 `Time.time`；初始为负无穷，避免游戏开始时误判宽限窗口。
  - `IsGrounded`：任意一条脚部射线命中时为 `true`，供地面状态判断是否仍接触支撑面，并用于刷新 Coyote Time。
  - `CanEnterGrounded`：至少两条脚部射线命中时为 `true`，供空中状态和 Grounded Action Condition 判断是否形成足够稳定的落地接触。
- 函数：
  - `UpdateGroundState()`：由玩家状态逻辑主动刷新 `IsGrounded` 与 `CanEnterGrounded`；任一射线命中地面层时同时刷新 `lastGroundedTime`。
  - `WasGroundedWithin(float duration)`：判断距离最后一次接地是否仍在指定时间窗口内。
  - `OnDrawGizmos()`：在 Scene 视图中绘制三条检测射线；绿色表示命中，红色表示未命中，并在命中点绘制圆圈。
- 关联：`PlayerState.LogicalUpdate()` 统一刷新检测结果；`PlayerGround`、`PlayerAir` 等状态读取当前接地，`Player_Fall` 结合最近接地时间判断 Coyote Time。

#### `Assets/_Game/Scripts/Tool/WallSensor.cs`

- 脚本职责：用身体与头顶两条水平 2D 射线检测贴墙及跨越墙边的瞬间，并在候选边缘通过向下射线确认墙顶，生成进入挂边状态所需的绝对世界 Y。
- 关键字段：
  - `point`、`bodyDistance`：身体贴墙射线的发射点与距离。
  - `ledgeCheckPoint`、`ledgeCheckDistance`：头顶边缘射线的发射点与距离。
  - `ledgeVerticalSearchDistance`：候选边缘处向下寻找墙顶的上下搜索范围。
  - `whatIsWall`：墙面 LayerMask。
  - `IsTouchingWall`、`WallDirection`：对外只读的贴墙状态与检测方向。
  - `ReachedLedgeThisFrame`：只有头顶射线状态发生变化、身体连续贴墙且成功找到墙顶时才在当前帧为 true。
  - `HangBodyTargetY`：按插值后的 Player Transform 与检测点误差计算出的 Rigidbody2D 绝对目标 Y，避免把 Transform 坐标产生的相对误差错误叠加到不同步的物理位置。
- 函数：
  - `UpdateWallState(bool facingRight)`：刷新两条水平射线；先形成边缘候选，再调用墙顶验证，成功计算目标位置后才发布 `ReachedLedgeThisFrame`；最后保存本帧状态供下一帧比较。
  - `TryFindLedgeTop(Vector2 ledgeRayEnd, out float ledgeY)`：从边缘检测射线末端上方向下发射 2D 射线，成功时通过 `out` 返回墙顶世界 Y。
  - `OnDrawGizmos()`：在 Scene 视图中绘制身体、头顶和墙顶搜索射线，方便校准检测范围。
- 关联：`PlayerState.LogicalUpdate()` 统一刷新检测结果；墙面检测供墙滑使用，确认边缘后由 `PlayerAir` 进入 `Player_HangIdle`。

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

#### `Assets/_Game/Scripts/Editor/HudFillInspectorLock.cs`

- Editor 专用：加载脚本和层级变化后，`private static void ApplyToLoadedScenes()` 查找已加载场景的 `Canvas_HUD/HUD/HP/MaskArea/Fill`、`Canvas_HUD/HUD/MP/MaskArea/Fill`，只给 RectTransform 添加 `HideFlags.NotEditable`，使其 Inspector 只读。
- Image 等其他组件仍可编辑，脚本仍可更新 RectTransform 的位置和尺寸；无需挂载组件，不主动标脏或保存场景，每次打开后自动应用。不参与游戏构建。
- 验证：HP、MP 两个目标均具有 NotEditable 标记，受控脚本调用仍能改变宽度并恢复原尺寸；Image 未被锁定，MP Fill 的 Inspector 已目视确认 RectTransform 灰显、Image 可编辑。临时验证脚本已移除。

#### `Assets/_Game/Scripts/Editor/UnityMcpSerializationGuard.cs`

- Editor 专用 Unity MCP 兼容补丁，不参与游戏构建。当前 `com.unity.ai.assistant` 2.6.0-pre.1 的组件读取工具通过反射展开 UI 组件属性，可能递归序列化 `TransformHandle` 和 `Matrix4x4` 的派生属性，触发 `StackOverflowException` 并使 Editor 崩溃；连接重试可使重开项目后再次触发。
- `[InitializeOnLoad]` 在 Editor 更新回调处理请求前，定位 `Unity.AI.MCP.Editor.Helpers.GameObjectSerializer` 的 `_outputSerializer`，注册有界输出转换器。矩阵仅输出 m00 至 m33 共16个标量；内部 TransformHandle 输出不透明标记，不继续展开引擎句柄。
- 未安装 MCP 时不执行；包结构变化导致找不到输出序列化器时给出警告。补丁使用包的私有字段，升级 AI Assistant 包后应重新验证并在上游修复后移除。
- 2026-10-20 验证：恢复启动完成并可操作 Editor；连续5次通过实际 MCP `GetComponentData` 序列化 RectTransform 和场景血条 Fill 的全部组件，结果正常返回，矩阵16个数值正确，无再次栈溢出。临时测试脚本已删除。故障与恢复日志保存在忽略目录 `Logs/Diagnostics/UnityCrash-20261020/`，未改血条实现。

#### `Assets/_Game/Scripts/Runtime/UI/UI_ResourceBarView.cs`

- 脚本职责：资源条父类，保留共享的序列化配置、校验、默认宽度更新与 Inspector 预览，不读取角色生命值/魔法值、不计算资源比例，也不订阅角色事件。HP、MP 使用各自子类，允许独立改变更新行为。
- 关键字段：
  - `fillClip`：血条填充裁剪区域的 `RectTransform`，由场景 UI 绑定。
  - `fullWidth`：序列化的满值宽度，可在 Inspector 配置；HP 为155，MP 为160。
  - `fillPercent`：Inspector 中0至1的预览比例，保留原 `fill` 序列化字段的迁移标记。
- 函数：
  - `public virtual void SetPercent(float percent)`：校验Fill引用和0至1的比例，通过后保存目标fillPercent并调用虚方法 `BarEffects(float percent)`。默认BarEffects立即设置fullWidth * percent；缓冲条重写该方法实现动画。HP通过PlayerHealth的Damaged事件读取角色血量。
  - `protected void OnValidate()`：按用户选择直接调用虚方法 `SetPercent(fillPercent)`，在修改 Inspector 值时立即更新预览；已移除 delayCall、额外重绘和取消排队逻辑。HP、MP 和 Energy 的 override 均由同一入口分派。
- 预览刷新：此前 delayCall 加主动重绘仍被用户确认拖动卡顿，现改为 OnValidate 直接更新。保留用户为 Energy 高度更新而调整的 protected 字段及 override。
- 直接预览验证：修改已编译，拖动HP比例至0.468时填充随之变化；Unity同时输出 `SendMessage cannot be called during Awake, CheckConsistency, or OnValidate (Fill: OnRectTransformDimensionsChange)`，栈为 OnValidate→HP.SetPercent→父类SetPercent。当前按用户选择保留直接OnValidate方案，不能声称警告已解决或连续拖动已无卡顿。
- 当前绑定状态：`Canvas_HUD/HUD/HP` 挂载 `UI_HPBarView`，`MP` 挂载 `UI_MPBarView`，均继承本类配置并分别绑定自身的 `MaskArea/Fill`，宽度、引用及预览比例已迁移，场景已保存。Fill 的 Inspector 只读规则保留，脚本更新不受影响。
- 迁移备份：通用组件迁移前的源码/场景位于忽略目录 `Logs/Diagnostics/ResourceBarBefore/`，继承调整前的父类源码/场景位于 `Logs/Diagnostics/ResourceBarInheritanceBefore/`。

#### `Assets/_Game/Scripts/Runtime/UI/UI_HPBarView.cs` / `UI_MPBarView.cs`

- MP编辑器调试入口 `private void DebugConsumeMP()`：Play Mode下通过组件右键菜单 `Debug/Consume 10% MP`，将目标fillPercent减少0.1并限制最低0，调用SetPercent触发主条速度曲线动画。只修改UI目标比例，不扣角色运行时MP；当前尚无当前MP数据组件。编辑态调用会警告并保持原值，入口不进入玩家构建。

- 分别继承 `UI_ResourceBarView`，SetPercent调用父类进行目标比例校验。HP使用默认BarEffects立即更新；MP重写BarEffects，通过BarFillTransition让主条宽度按速度曲线过渡，不增加Buffer。MP的OnEnable同步初始比例，OnDisable清除动画。
- 两个脚本保留最初 HP、MP 脚本的 GUID；场景已替换为对应子类，HP 满值宽度155，MP为160。通过父类引用调用仍进入实际子类 override；Inspector 预览也走同一虚方法。
- HP额外通过序列化的 playerHealth 引用场景Player的PlayerHealth：OnEnable订阅Damaged并刷新，OnDisable解除订阅；Start在角色Awake初始化完成后同步初始比例。`protected virtual void RefreshHealth()` 读取CurrentHealth / Stats.MaxHealth.Value，Clamp01后调用SetPercent更新宽度；角色未初始化或最大生命值非正时不更新。受伤事件发生前生命值已经扣除，因此回调读取的是受伤后的比例。
- HP受伤绑定本次仅做代码与场景引用检查，未运行Unity；可在Play Mode让Player实际受伤，确认生命值变化与血条比例一致，再禁用/启用HP对象确认显示同步。
- 验证：编辑态和 Play Mode 均确认两个 SetPercent 的 DeclaringType 为各自子类、GetBaseDefinition 指向父类；通过父类引用调用0%、50%、100%时宽度正确，高度和引用不变，测试后恢复原比例。临时迁移/验证脚本已删除，场景已保存。

#### `Assets/_Game/Scripts/Runtime/UI/BarFillTransition.cs`

- HPBuffer、MP、Energy共用的纯动画逻辑，不持有UI或角色引用。`void SetTarget(float percent, AnimationCurve speedCurve, bool immediate)` 从当前Value向目标过渡；相同目标不重启动画。`bool Tick(float deltaTime, float duration)` 推进时间并更新Value，返回是否需要刷新显示；`void Reset()` 在禁用时清除状态。
- 使用64段梯形积分，将速度曲线累计面积归一化为位移进度。负速度按0处理，无曲线或零总面积退回匀速。首次赋值立即同步，曲线在新目标开始时采样，使用Time.deltaTime，暂停时停止推进。
- MP与Energy的场景duration和完整speedCurve数据复制自用户当前保存的HPBuffer（1秒，中间关键点0.5055542 / 0.009593964），之后可独立调节。MP和Energy增加、减少均平滑变化；HPBuffer仅减少时动画。未使用computer use，未运行Unity测试，由用户在Play Mode调整MP/Energy的Fill Percent观察。

#### `Assets/_Game/Scripts/Runtime/UI/UI_HPBufferView.cs`

- 继承UI_HPBarView，复用PlayerHealth的Damaged订阅和RefreshHealth；场景HPBuffer的playerHealth已绑定Player组件，fillClip沿用其自身Fill。HP主条仍立即更新，缓冲条重写 `protected override void BarEffects(float percent)`，在扣血时从当前显示比例追向新目标，连续受伤时从当前可见位置重新开始。
- Inspector的duration默认1秒；speedCurve横轴为归一化时间，纵轴为非负相对速度。当前场景保存的三个关键点为(0,0)、(0.5055542,0.009593964)、(1,0)。动画计算交给BarFillTransition，每帧将当前显示比例应用到自身Fill宽度。
- 初始同步、回血、编辑态预览和禁用状态赋值立即显示；禁用时清除动画并解除受伤订阅，重启后同步当前生命值。使用Time.deltaTime，暂停时动画也暂停。fillPercent保存目标比例，BarFillTransition.Value保存当前显示比例，动画不会修改角色生命值。
- 本次按用户要求未进行Unity运行测试。PlayerHealth 的 Debug/Take 10 Damage 临时入口已移除，扣血与 UI 更新使用真实攻击命中链路。

#### `Assets/_Game/Scripts/Runtime/UI/UI_EnergyBarView.cs`

- Energy编辑器调试入口 `private void DebugConsumeEnergy()`：Play Mode下通过组件右键菜单 `Debug/Consume 10% Energy`，将目标fillPercent减少0.1并限制最低0，调用SetPercent触发Shader填充速度曲线动画。只修改UI目标比例，当前尚无独立Energy资源数据。编辑态调用会警告并保持原值，入口不进入玩家构建。两项调试入口未进行Unity运行测试。

- 继承UI_ResourceBarView，SetPercent保存目标比例并分派到重写的BarEffects；BarFillTransition驱动Energy主条的增减动画。`private void ApplyFill(float percent)` 通过fillClip找到Graphic，把当前显示比例写入引用材质及materialForRendering的_FillAmount，保持Fill矩形不变。
- `private void OnEnable()` 应用当前比例。继承的OnValidate通过SetPercent→BarEffects分派，编辑态即时预览；Play Mode按速度曲线过渡。OnDisable清除动画状态。
- 已删除 fullHeight 和按比例缩短高度的逻辑。场景 `Energy/MaskArea/Fill` 的 Image 已设置为Simple，并使用 M_UIEnergyFlow；Fill矩形保持100×42.5，比例仅控制Shader裁剪，纹理不再因比例改变而压缩。
- 当前直接更新所引用的材质；若增加多个独立Energy显示，应为它们配置独立材质，避免共享比例。
- 验证：编辑态和Play Mode下，0%、25%、50%、100%均同步到源材质、Stencil派生材质和CanvasRenderer实际材质；Fill尺寸始终为100×42.5。验证后恢复原比例，临时验证脚本已删除。

#### `Assets/_Game/Scripts/Runtime/UI/UI_SkillBeanView.cs`

- 继承UI_EnergyBarView，复用SetPercent和Inspector的Fill Percent，通过Shader裁剪显示比例；重写AnimateFill返回false，维持即时更新，四个独立材质不变。
- `GameScene/Canvas_HUD/HUD/SkillBeans` 下的 Bean1～Bean4，其 Fill Image 分别引用 `Assets/_Game/Materials/M_UISkillBean1Flow.mat`～`M_UISkillBean4Flow.mat`。四份材质复制自 M_UIEnergyFlow，使用同一Shader、纹理和初始参数，但各自保存独立的 `_FillAmount`；Energy保留原材质。
- 本次仅检查场景引用与材质资源的一致性；Unity导入、Inspector预览及Play Mode由用户测试。检查方式：在Hierarchy依次选择Bean1～Bean4，将各自组件的Fill Percent设为0.25、0.5、0.75、1，观察四个豆分别显示对应高度，Energy不受影响。

#### `Assets/_Game/Shaders/UIEnergyFlow.shader` / `Assets/_Game/Materials/M_UIEnergyFlow.mat`

- Shader 菜单名 `Game/UI/Energy Flow`，用于 UGUI Image/RawImage 的独立能量纹理，不依赖 Image 自动传入的 `_MainTex`。材质的 `_EnergyTex` 已绑定原始 `WindEnergyTexture.png`，默认 Tiling 为2×2，便于用户检查接缝；未修改原图或导入设置。
- 使用显式 Linear + Repeat 采样器，`_FlowVelocity.xy` 为每秒UV流动速度，正值表示纹理向对应正方向移动。`_EnergyTex` Tiling/Offset 只影响纹理采样，不影响填充高度。
- `_FillAmount` 控制由底部向上的可见区域，0全空、1全满；中间值的顶部由两条正弦波形成细微波动。`_WaveAmplitude`、`_WaveFrequency`、`_WaveSpeed` 分别控制幅度、频率和速度，`_EdgeSoftness` 控制边缘软化。Fill矩形保持固定，不通过压缩纹理表示比例。
- 包含UGUI标准Stencil比较/读写字段、透明混合、UI裁剪矩形和Alpha Clip变体，兼容现有多边形Mask。建议测试Image不使用Sprite（Source Image=None），Type=Simple，颜色为白色；避免SpriteAtlas UV影响归一化的填充高度。
- 当前已连接场景Energy的Fill Percent与Shader Fill Amount：从Energy组件调整比例即可；材质继续控制流动、平铺和波浪参数，Fill的Image使用Simple以保持固定UV范围。
- 验证：在独立Preview Scene同步编译Shader，实际渲染检查满/半/空值像素，并验证双区域UIPolygonMask的两块内部可见、中间空隙被遮挡；预览图位于 `Logs/Diagnostics/EnergyTexture-2x2-preview.png`、`EnergyTexture-half-fill-preview.png`。临时验证脚本已删除。原图是否无缝平铺由用户检查，尚未做连续时间动画回归。

#### `Assets/_Game/Scripts/Runtime/UI/UIPolygonMask.cs`

- 填充素材：`Assets/_Game/Art/UI/HUD/WindEnergyTexture.png`，淡青白风丝与雾气组成的矩形风元素纹理，按 Single Sprite / Full Rect 导入，供子节点 Image 的 Source Image 使用；尚未绑定场景对象。当前为静态纹理，未验证无缝平铺。
- 职责：从同物体的 `PolygonCollider2D` 读取多条闭合轮廓，逐条三角化后合并为同一个 `MaskableGraphic` 网格，再由同物体的 UGUI `Mask` 裁剪子 UI；各区域取并集，不负责血量计算或填充比例。
- 添加：在 Canvas 下选中父物体，右键 `UI/Polygon Mask` 创建 `PolygonMask`（初始 600 × 60）；也可在一个没有 Image/其他 Graphic 的 UI 物体上添加 `UI/Polygon Mask` 组件。自动依赖 `CanvasRenderer`、`PolygonCollider2D`、`Mask`。
- 编辑：在 `UI Polygon Mask` 组件点击 `Add Closed Region`，在 RectTransform 中心新增一条小矩形路径，支持 Undo；随后在 `Polygon Collider 2D` 组件点击 `Edit Collider`，在 Scene 拖动新路径顶点。也可在 Collider 的 Paths 中管理各条路径。Collider 保持禁用，仅存储轮廓，不参与物理碰撞；`Update()` 检测 `GetShapeHash()` 的变化后请求 UI 重建。
- `protected override void OnPopulateMesh(VertexHelper vh)`：Unity 重建 UI 时调用 `private bool BuildPolygons()`，遍历所有路径，交给 `private bool BuildPath(int pathIndex)` 按路径读取 offset、检查自交并以耳切法三角化凹/凸多边形。合并时为每条路径的索引加上顶点起始偏移，避免在独立区域间生成连接三角形；UV 使用同一个 RectTransform 坐标空间。
- `public void FitToRect()`：由 Inspector 的 `Fit Outline To Rect (Replaces All Regions)` 按钮或创建菜单调用，替换所有区域为当前 RectTransform 的单个矩形。按钮支持 Undo。
- `public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)`：由 UI 射线链调用，使用所有区域的合并三角形过滤指针命中，区域之间空隙不接受命中。
- 默认取消 `Mask.Show Mask Graphic`，仅裁剪，不绘制遮罩自身。子节点中的 Image 保持 `Maskable`；Frame 应放在遮罩外，作为同级装饰。
- 后续连接数值时，完整槽轮廓应固定，只改变子节点 Fill 的显示宽度或填充比例，使剩余量与槽轮廓相交；当前未接入该数值更新链。
- 限制：支持多个实心区域及区域重叠，不支持孔洞、单条路径自交和接触自身；任一路径非法时清空整个合并网格和射线区域，并警告具体 Path 索引。顶点使用本地 UI 单位，RectTransform 尺寸改变不会自动缩放轮廓；应先定尺寸，再编辑点。父级 Transform/Canvas 缩放会正常作用于整个轮廓。
- 验证：Unity 临时 Preview Scene 中实际渲染五顶点切角（5 顶点、3 三角形），采样确认内部红色、切角及外部黑色；凹轮廓三角化及重置 Undo 通过。
- 多路径变更验证边界：运行时代码已在 Unity 的 Assembly-CSharp 中编译，代码差异检查通过；本次新增的多区域实际渲染、射线、offset 和单路径回归脚本已准备于 `Logs/Diagnostics/UIPolygonMaskCheckpoint.cs`，尚未执行。手动导入测试脚本被自动审批以场景仍有未保存状态为由拦截，临时入口已移出 Assets，避免自动改动现场；不能把此前的单路径验证视为本次多路径运行验证。
- 当前已保存的场景配置：`Canvas_HUD/HUD` 下包含 `HP/MaskArea/Fill`、`MP/MaskArea/Fill` 及装饰 `Frame`。两处 MaskArea 均挂载 `UIPolygonMask`、禁用的 `PolygonCollider2D`、隐藏自身图形的 `Mask` 和 `CanvasRenderer`；HP 轮廓为 6 点，MP 为 9 点。Frame 使用 `Assets/_Game/Art/UI/HUD/HUD.png` 中的 Sprite，位于两处遮罩外。

#### `Assets/_Game/Scripts/Editor/UIPolygonMaskEditor.cs`

- 职责：提供区域数量、新增闭合区域按钮、替换全部区域的重置按钮、使用提示及 `GameObject/UI/Polygon Mask` 创建菜单；顶点编辑直接复用 Unity 原生 PolygonCollider2D 编辑工具。`private static void AddClosedRegion(UIPolygonMask graphic)` 由按钮调用，保留已有路径并在新索引写入矩形，支持 Undo 和脏标记。

#### `Assets/_Game/Scripts/Runtime/UI/InventoryView.cs`

- 背包快捷操作：`UseItem()` 与 `DropItem()` 均要求已选中物品、没有进行拖拽、且选中物品与 `ItemDetailPanel.CurrentItem` 一致。数量来自 `CurrentAmount`。F 调用 `TryUseItem`，Q 复用已有 `DropItem(InventoryItem item, int amount)` → `TryRemove`。数量改变由 `ItemAmountUpdated` 更新显示并将面板数量重置为 1；整件移除由 `ItemRemoved` 清空选中与详情。

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
  - `SetAmount(int amount)`：刷新右下角的数量文字；数量大于 1 时显示数字，数量为 1 或更少时清空文字。
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

#### `Assets/_Game/Scripts/Runtime/UI/UI_CharacterStatsPanel.cs`

- 脚本职责：读取玩家运行时属性与当前生命值，显示在背包角色面板；不修改属性、不参与伤害计算，也不负责开关背包。
- 场景配置：挂在 `GameScene/Canvas_Inventory` 下的 `CharacterStatsPanel`，引用 Player，以及 `HealthRow`、`AttackRow`、`DefenseRow`、`CriticalChanceRow`、`CriticalDamageRow`、`BaseManaRow` 各自的 `Value` TMP 文本。
- `Awake()`：兜底查找 Player；Player 或任一文本引用缺失时，输出含对象名与缺失条件的警告并禁用组件。
- `OnEnable()`：请求强制刷新并调用 `RefreshStats()`；若玩家尚未完成 Awake 初始化则等待后续帧。
- `LateUpdate()` → `RefreshStats()`：仅在面板激活期间检查运行时数值，缓存上次显示值，变化时才格式化并重写文本；可覆盖受伤、属性加成和关闭后重开。使用逐帧读取而非新增属性事件，避免扩展当前 `Stat` API。
- `RefreshValue(TMP_Text text, float value, ref float displayedValue, bool percentage = false)`：内部显示辅助函数，由 `RefreshStats()` 传入文本、当前属性值和上次显示值；百分比乘 100，数字最多保留两位小数，无返回值。

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

- `CurrentItem` / `CurrentAmount`：只读公开当前展示物品与玩家选择的操作数量，供背包快捷使用与丢弃读取。
- `useItemButton`：引用详情面板中的 `UseButton`。`Show(InventoryItem item)` 每次按 `item.Data.CanBeConsumed` 切换其显隐；当前仅 `Consumable` 分类显示。点击通过 `OnUseButton()` 发出无参数 `UseRequested`，由 `InventoryView` 在启用时订阅、禁用时退订，调用与 F 相同的 `UseItem()`；选中物品和数量由 View 从当前状态读取，最终限制由 `PlayerInventory.TryUseItem` 执行。

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

### Runtime/Systems/FallingSand

#### `Assets/_Game/Scripts/Runtime/Systems/FallingSand/SandGrid.cs`

- 脚本职责：Falling Sand 的纯 C# 二维 Cell 数据容器，不继承 `MonoBehaviour`，不负责模拟时间、输入或渲染。Grid 以左下角为 `(0, 0)`，`x` 向右、`y` 向上。
- 关键字段/属性：
  - `width` / `Width`、`height` / `Height`：Grid 尺寸；外部只能读取尺寸。
  - `cells`：以一维 `byte[]` 保存所有 Cell，索引规则为 `x + y * width`。材质 ID `0` 表示空，`1..255` 可表示不同颗粒材质或颜色类别。
- 函数：
  - `SandGrid(int width, int height)`：保存 Grid 尺寸并创建 `width * height` 个 Cell；新数组默认全部为 `0`，因此新 Grid 初始为空。当前尚未加入非正尺寸的构造参数校验。
  - `IsInside(int x, int y)`：判断单个 Cell 坐标是否位于 Grid 内，合法范围为 `x: 0..Width-1`、`y: 0..Height-1`。
  - `TryGetMaterial(int x, int y, out byte materialId)`：安全读取 Cell；越界时将输出设为 `0` 并返回 `false`，合法时输出对应材质 ID 并返回 `true`。
  - `TrySetMaterial(int x, int y, byte materialId)`：安全写入 Cell；越界返回 `false` 且不修改数组。写入 `0` 表示清空 Cell。
  - `ToIndex(int x, int y)`：私有坐标转换，将二维坐标映射为一维数组下标；调用前由公开读写函数完成边界检查。
- 关联：`SandSimulation` 通过安全读写接口移动颗粒，`SandGridRenderer` 读取材质 ID 并映射为动态纹理颜色，`SandWorld` 创建并持有运行时 Grid。

#### `Assets/_Game/Scripts/Runtime/Systems/FallingSand/SandSimulation.cs`

- 脚本职责：Falling Sand 的纯 C# 模拟规则，不继承 `MonoBehaviour`。非空 Cell 优先向正下方移动；正下方被阻挡时依次尝试左下和右下，三个方向都不可用时保持原位。
- 关键字段：
  - `grid`：构造时注入并以 `readonly` 保存的 `SandGrid` 引用；模拟直接读写同一份 Grid 数据。
  - `random`：由构造 Seed 创建的独立 `System.Random`，用于决定斜向移动的优先顺序；相同 Seed 可复现相同决策序列。
  - `BurstDirectionX` / `BurstDirectionY`：按相同下标配对的七个撞击破碎方向，包含上、左上、右上、左、右、左下和右下，不包含正下。
- 函数：
  - `SandSimulation(SandGrid grid, int randomSeed = 0)`：保存待模拟 Grid，并按 Seed 创建随机数生成器；传入 `null` Grid 时抛出 `ArgumentNullException`。默认 Seed 为 0，测试或运行时可传入明确 Seed 重现结果。
  - `ApplyImpactBurst(float edgeBurstChance, float interiorCrackChance, int maxExtraDistance)`：复制撞击瞬间的占用快照，先按内部概率处理内部颗粒，再按外圈概率处理暴露颗粒，返回真正成功移动的总数。概率使破碎数量随候选颗粒数量自动缩放。
  - `MoveBurstGroup(...)`：只扫描撞击快照中原本存在的颗粒，通过 `HasEmptyBurstNeighbor` 判断所属组，并用 `Random.NextDouble()` 与对应概率决定是否参与；内部使用随机方向，外圈使用原始暴露方向。
  - `TryFindBurstTarget(...)`：在真实 Grid 中沿指定方向穿过连续占用 Cell，找到第一个空 Cell 后最多额外逐格前进指定距离；遇到占用或边界立即停止，输出最终安全目标。
  - `GetExposedBurstDirection(...)`：从随机方向起点循环查找撞击快照中原本为空且位于 Grid 内的相邻方向，使外圈颗粒向原始轮廓外侧破碎。
  - `GetRandomBurstDirection(...)`：从七个撞击方向中随机选择一组配对的 X/Y 偏移，供内部裂隙使用。
  - `HasEmptyBurstNeighbor(...)`：根据撞击快照检查七个邻近方向，只要有一个 Grid 内邻格原本为空，就把来源颗粒归类为外圈。
  - `Step()`：执行一次模拟 Tick。按 `y = 1..Height-1` 从下向上、每行从左向右扫描；先跳过空 Cell，每颗粒子优先尝试正下，失败后才用 `System.Random.Next(0, 2)` 随机决定左下/右下的尝试顺序，首选失败仍会尝试另一边。返回本次是否至少移动过一颗粒子。底行 `y = 0` 不参与下落，因此 Grid 下边界充当地面。
  - `TryMove(int fromX, int fromY, int toX, int toY)`：检查来源是否存在颗粒、目标是否在界内且为空；成功时把来源材质 ID 写入目标，再将来源清为 `0`。越界或被占用时返回 `false` 且不修改 Grid。
- 关联：读取并修改 `SandGrid`；由 EditMode 测试直接验证，并由场景中的 `SandWorld` 按固定频率调用。

#### `Assets/_Game/Scripts/Runtime/Systems/FallingSand/FirstGame.FallingSand.asmdef`

- 职责：把 Falling Sand 纯逻辑代码编译成独立程序集 `FirstGame.FallingSand`，使 EditMode 测试可以直接引用；`autoReferenced: true` 允许 `Assembly-CSharp` 中后续运行时组件使用它。

#### `Assets/_Game/Scripts/Runtime/Systems/FallingSand/SandGridRenderer.cs`

- 脚本职责：把 `SandGrid` 的材质 ID 映射成 `Color32` 像素，并通过运行时 `Texture2D`、`Sprite` 与 `SpriteRenderer` 统一显示；不修改 Grid 或执行模拟。
- 关键字段：`spriteRenderer` 是场景绘制组件；`grid` 是只读数据来源；`texture` 是与 Grid 同宽高的 RGBA32 动态纹理；`pixels` 是 CPU 端颜色缓冲；`runtimeSprite` 使用左下 Pivot 把 Texture 对齐到 `SandWorld` 原点。
- 函数：
  - `Initialize(SandGrid grid, float cellSize)`：保存 Grid，创建无 Mipmap 的 RGBA32 Texture 与等长 `Color32[]`，设置 Point 过滤和 Clamp 包裹；用 `1 / cellSize` 作为 Pixels Per Unit 创建左下 Pivot Sprite，并赋给 `SpriteRenderer`。
  - `Render()`：逐 Cell 读取材质 ID，以相同的 `x + y * Width` 下标写入像素缓冲，随后调用一次 `SetPixels32` 和 `Apply(false)` 上传整张 Texture。
  - `GetColor(byte materialId)`：将 `0` 映射为透明，将当前沙粒颜色变体 ID 映射为不同沙色；未知 ID 返回紫色以暴露配置错误。
- 关联：由 `SandWorld` 初始化并在 Grid 状态改变后调用。运行时 Sprite/Texture 的销毁清理尚未加入。

#### `Assets/_Game/Scripts/Runtime/Systems/FallingSand/SandWorld.cs`

- 脚本职责：把纯逻辑 Grid、模拟器和世界渲染组合到 Unity 场景中。GameObject 的 Transform 表示 Grid 左下角世界原点。
- 关键字段：`width`、`height`、`cellSize` 定义逻辑分辨率与世界尺寸；`simulationStepsPerSecond` 和累计时间控制模拟频率；`simulationSeed` 让随机序列可复现；`edgeBurstChance` 与 `interiorCrackChance` 分别控制外圈和内部颗粒参与撞击破碎的概率，`impactBurstExtraDistance` 控制额外飞散距离；`impactDetected` 保证当前运行只处理一次首次触底；`gridRenderer` 是 Inspector 绑定的表现层。
- 函数：
  - `Awake()`：创建 `SandGrid` 与使用指定 Seed 的 `SandSimulation`，初始化渲染器，当前还会在 Grid 高处写入带随机沙色 ID 的临时测试块并首次渲染。
  - `Update()`：累计 `Time.deltaTime`，达到 `1 / simulationStepsPerSecond` 后执行一次 `Step()`；首次发现底行存在沙粒时调用 `ApplyImpactBurst`，并将其真实移动结果合并进 `gridChanged` 后统一 Render。当前使用单次 `if`，低于目标模拟频率的帧率下不会在一帧内追赶多个 Tick。
  - `DoesBottomRowContainSand()`：扫描 `y = 0` 判断是否至少有一颗沙触及容器底面；它是当前容器实验的临时撞击条件，尚不能识别任意已有沙堆的接触。
  - `OnDrawGizmosSelected()`：在 Scene 视图选中对象时，从左下原点向右上绘制 Grid 世界边界线框。
- 关联：当前挂在 `GameScene` 的 `SandWorld` 对象上，组合 `SandGrid`、`SandSimulation` 与 `SandGridRenderer`；高处测试块是临时验证入口，后续由完整沙块颗粒化替换。

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
  - `canBeCrafted`、`CanBeCrafted`：是否可以被合成，Inspector 中配置，默认 `false`；只读属性供合成逻辑查询，尚未接入合成流程。
  - `craftingMaterials`、`CraftingMaterials`：合成该物品所需材料的 `ItemData[]`，默认空数组，在 Inspector 中引用材料资源；当前仅保存材料类型，不包含独立数量字段，尚未接入材料检查或消耗逻辑。
- 关联：通过 `CreateAssetMenu` 暴露 `Game/Inventory/Item Data` 创建入口；被 `InventoryItem` 作为运行时物品实例的数据来源。

#### `Assets/_Game/Scripts/Runtime/Systems/InventorySystem/ItemCategory.cs`

- 脚本职责：定义背包物品分类枚举。
- 枚举值：`Material`、`Consumable`、`Weapon`、`Armor`、`Quest`、`Tool`、`Misc`、`Gloves`（护手）、`Necklace`（项链）、`Ring`（戒指）、`Boots`（鞋子）。新增分类追加在末尾，保留已有序列化数值；分类本身不提供穿戴或属性加成行为。
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

#### `Assets/_Game/Scripts/Runtime/Systems/InventorySystem/ItemEffect.cs`

- 脚本职责：物品即时效果的抽象 ScriptableObject 基类，不挂载到 GameObject，不负责库存扣除或 UI 刷新。
- `public abstract bool TryApply(ItemEffectContext context, int amount)`：由具体效果实现。`context` 携带使用者与物品配置，`amount` 表示本次使用数量；返回 true 表示效果成功，库存扣除由调用者负责。`ItemData.Effect` 引用单个效果资源，由 `PlayerInventory.TryUseItem` 调用。

#### `Assets/_Game/Scripts/Runtime/Systems/InventorySystem/HealItemEffect.cs`

- 即时回血效果 ScriptableObject；菜单 `Game/Inventory/Heal Item Effect`，`HealAmountPerItem` 为每份物品的恢复量。
- `bool TryApply(ItemEffectContext context, int amount)`：校验上下文、数量和恢复量，通过 `context.source.TryGetComponent<ItemEffectReceiver>` 找到使用者的接收组件，调用 `TryHeal(HealAmountPerItem * amount, context)`。无接收组件时返回 false，不扣库存。
- `Assets/_Game/Data/Items/Effects/GreenHerbHeal.asset`：每株 25 HP，被现有 `GreenHerb.asset` 的 `Effect` 引用；草药仍为 1×1、最大堆叠 3。

#### `Assets/_Game/Scripts/Runtime/Systems/InventorySystem/ItemEffectReceiver.cs`

- 挂在 `GameScene` 的 `Player`（与 `PlayerInventory` 同对象）上，作为物品效果进入角色运行时系统的入口。
- `bool TryHeal(float amount, ItemEffectContext context)`：确认使用者为自身且参数有效；当前输出包含角色、物品和恢复量的模拟回血日志并返回 true，尚无生命值数据变化。将来在此接生命组件，效果资源不保存角色生命值。

#### `Assets/_Game/Scripts/Runtime/Systems/InventorySystem/PlayerInventory.cs`

- `bool TryUseItem(InventoryItem item, int amount)`：先校验非空、正数量、不超库存、归属本网格、可消耗且配置了 `Effect`，再创建 `ItemEffectContext(gameObject, item.Data)` 并调用 `Effect.TryApply(context, amount)`；只有效果成功才 `TryRemove`。当前适用于不修改背包的同步效果；失败不扣库存。按钮与 F 共用此链路，Q 丢弃保持直接 `TryRemove`，不执行效果。

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
- `FallingSand/FirstGame.FallingSand.Tests.asmdef`：Falling Sand 的 EditMode 测试程序集，引用 `FirstGame.FallingSand` 与 Unity Test Runner，并仅在 Editor 中编译。
- `FallingSand/SandGridTests.cs`：覆盖尺寸属性、边界判断、新 Grid 全空、`byte` 材质 ID 的写入与读回、邻格隔离、写入 `0` 清空、`byte.MaxValue` 往返，以及越界读写返回 `false`。
- `FallingSand/SandSimulationTests.cs`：覆盖空 Grid、单粒下落、每 Tick 只移动一格、底边阻挡、堆叠颗粒同时各移动一格、连续 Step 最终静止，以及下方优先、随机选择一个开放斜向、另一侧回退、完全阻挡、左右边界和相同 Seed 决策可复现；撞击破碎测试验证零概率不移动、满概率搬运时颗粒总数守恒并留下缺口，以及相同 Seed 产生相同最终 Grid。
- 运行方式：`Window → General → Test Runner → EditMode → Run All`；也可只选中 `SandGridTests` 运行当前 Checkpoint。

### Runtime/GameFlow

#### `Assets/_Game/Scripts/Runtime/GameFlow/GameBootstrap.cs`

- 脚本职责：启动流程入口，持久化 BootScene 中的 GameRoot；正常启动进入主菜单，编辑器快捷启动请求则直接加载 GameScene 并将游戏层设为 Gameplay。
- 关键字段：
  - `sceneLoader`：启动后加载主菜单。
  - `layerStack`：启动时建立初始层。
- 函数：
  - `Awake()`：通过 `RequireComponent` 和组件查找确保 `SceneLoader`、`GameLayerStack` 可用。
  - `Start()`：重置当前层并加载主菜单场景。
- 关联：BootScene 中的启动对象使用它把项目从启动场景导入 `MainMenuScene`。

#### `Assets/_Game/Scripts/Editor/DirectGameScenePlay.cs`

- 编辑器开发开关：`Tools/Game/Direct Play From GameScene`，默认开启，偏好按项目路径保存在 EditorPrefs 中，不影响构建。
- `OnPlayModeStateChanged(PlayModeStateChange state)`：从 GameScene 开始 Play 时，将本次 Play 起始场景临时设为 BootScene，通过 SessionState 请求 GameBootstrap 跳过主菜单。复用原有输入、游戏层、事件、Flag、任务配置，避免直接运行时缺少常驻服务。
- 停止 Play 后恢复原先的 `playModeStartScene` 并清理临时请求；Unity 恢复编辑中的 GameScene。从 BootScene 正常启动仍进入主菜单。关闭菜单开关后恢复原行为。
- 已在 Play Mode 验证：GameScene 直启进入 Gameplay，GameBootstrap 与 GameInputReader 各一份，InputRouter 的关键引用有效，I 打开背包、Escape 返回 Gameplay，停止后恢复 GameScene 与原起始场景设置。

### Runtime/Input

#### `Assets/_Game/Scripts/Runtime/Input/GameInputReaders.cs`

- 背包 UI 输入：`UI.UseItem.performed` 发出 `UseItemInInventoryPressed`；`UI.DropItem.performed` 发出 `DropItemInInventoryPressed`。按键沿用 Input Actions 资源中的 F/Q 配置。

- 脚本职责：包装 Unity Input System 生成类，把输入回调转换为项目内部事件和输入缓存。
- 关键字段/事件：
  - `inputActions`：`InputSystem_Actions` 生成对象。
  - `MoveInput`：当前玩家移动输入。
  - `UINavigateInput`：当前 UI 导航输入。
  - `MoveChanged`、`JumpPressed`、`AttackPressed`、`LauncherAttackPressed`、`DashPressed`、`InteractPressed`、`UseItemPressed`：玩家输入事件。
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

- `OnUsingItemInInventory()` / `OnDropItemInInventory()`：在 `OnEnable` 订阅对应输入事件、`OnDisable` 退订；仅当前层为 `Inventory` 且 `inventoryView` 已赋值时调用其 `UseItem()` / `DropItem()`。选中物品、数量和拖拽判断由 View 承担。

- 脚本职责：输入路由层，把 `GameInputReader` 的原始事件分发到玩家、对话、UI 或系统。
- 关键字段：
  - `inputReader`：输入事件来源。
  - `gameLayerStack`：判断当前输入层。
  - `playerControlArbitration`：判断当前层是否允许玩家动作。
  - `player` / `playerMotionController`：直接引用玩家并取得 Motion Controller，接收 Jump / Dash / Interact Command。
  - `attackCommand`、`launcherAttackCommand`：把普通攻击与上挑攻击 Input Action 映射为战斗系统使用的抽象 Command Key。
  - `playerCombatActionController`：接收战斗 Action Request。
  - `dialogueManager`：接收对话推进和选项输入。
- 函数：
  - `Awake()`：补齐必要引用。
  - `OnEnable()`：订阅输入事件和层变化事件。
  - `OnDisable()`：取消订阅。
  - `OnCurrentLayerChanged(GameLayerType previousLayer, GameLayerType currentLayer)`：层变化时先根据 Gameplay/移动权限清空 CommandInput，再调用 inputReader.SetInputMode()；Update、OnDisable 和 OnApplicationFocus 处理额外清零入口。
  - `OnMoveChanged(Vector2 moveInput)`：Gameplay 且允许移动时写入 Player.CommandInput.MoveInput，否则清空；InputRouter 直接引用 Player，缺少引用时在 Awake 查找 Player。
  - `OnJumpPressed()`：Gameplay 且允许跳跃时向 MotionController 提交 jumpCommand，移动方向继续从 Player.CommandInput 读取。GameScene 复用既有 Command_DoubleJump 作为共享跳跃请求键，具体跳法由状态决定。
  - `OnAttackPressed()`：Gameplay 且允许攻击时，把 `attackCommand` 提交给 `PlayerCombatActionController.RequestAction()`；输入层不决定最终招式。
  - `OnLauncherAttackPressed()`：Gameplay 且允许攻击时，把 `launcherAttackCommand` 提交给同一个 Action Controller；上挑招式由配置匹配，而不是由 Router 直接播放。
  - `OnDashPressed()`：Gameplay 且允许冲刺时只向 ActionController 提交 Dash Command，不根据当前状态分流。
  - `OnInteractPressed()`：Gameplay 且允许 WorldInteract 时向 MotionController.RequestAction 提交 InteractCommand；Dialogue 时仍直接请求推进台词。
  - `OnUseItemPressed()`：Gameplay 且允许使用物品时处理使用物品入口，目前预留。
  - `OnPausePressed()`：Gameplay 或其他允许层中处理暂停入口，目前预留。
  - `OnOpenInventoryPressed()`：处理背包输入入口，目前预留。
  - `OnOpenMapPressed()`：处理地图输入入口，目前预留。
  - `OnUINavigateChanged(Vector2 navigateInput)`：DialogueChoice 层把导航交给对话选项。
  - `OnUISubmitPressed()`：DialogueChoice 层确认当前选项。
  - `OnUICancelPressed()`：处理 UI 取消输入入口，目前预留。
  - `IsCurrentLayer(GameLayerType layerType)`：封装当前层判断。
- 关联：位于 `GameInputReader` 和游戏系统之间；既依赖 `GameLayerStack`，也依赖 `PlayerControlArbitration` 的动作锁定结果。

### Runtime/GamePlay/Combat/Actions

- 创建资源菜单统一在 `Assets/Create/Game/Actions/`：Combat 子菜单包含 Action Definition、Action System Config、Conditions 和 Effects；Action Command 位于 `Game/Actions/Action Command`，Movement Action 位于 `Game/Actions/Movement Action`。仅调整 CreateAssetMenu 路径，资源目录与 GUID 不变。

#### `ActionCommandKey.cs`

- 脚本职责：用 ScriptableObject 资源身份表达抽象战斗输入意图；核心 Controller 不维护普通攻击、重攻击或 Parry 等硬编码枚举分支。
- 当前资源：`Command_Attack.asset`、`Command_LauncherAttack.asset`；Command 只表达输入意图，不编码具体招式类型。

#### `ActionRequest.cs`

- 脚本职责：不可变的玩家一次性输入快照，只保存 `Command` 与请求发生时的 `RequestedTime`。构造签名为 `ActionRequest(ActionCommandKey command, float requestedTime)`；请求表达输入意图，不直接指定最终 Action。当前代码没有 Value 字段。

#### `ActionContext.cs`（Runtime/GamePlay/Actions）

- 独立的 readonly struct，保存 IsGrounded、CanEnterGrounded、GroundProbePosition、CanDash、CanDoubleJump、MoveInput，通过构造函数的可选参数提供次数事实与当前角色的 PlayerSkills 查询来源；不引用 Player、不持有战斗运行时、不执行动作，由通用 TryStartAction、入口条件和 Stage 条件使用。末尾可选参数 Vector2 moveInput 默认零向量；PlayerState.CreateActionContext 从 CommandInput.MoveInput 传入当前输入快照。
- 与 CombatActionContext 没有继承关系，两个 Context 各自独立；通用 Context 的新增不改变现有 Combat 的类型、构造和条件接口。

#### `CombatActionContext.cs`

- 脚本职责：保持原有独立 readonly struct，保存 IsGrounded、CanEnterGrounded、GroundProbePosition、CombatActionChainRuntime / CombatActionUsageRuntime，并提供 HasConfirmedHit(CombatActionDefinition action) / HasActionBeenUsed(CombatActionDefinition action)。原五参数调用保持兼容，新增可选 PlayerSkills skills 参数；IsSkillUnlocked(SkillData skill) 查询指定技能，缺角色技能集合或 SkillData 时返回 false。两个 Context 没有继承关系；条件提供独立的 ActionContext / CombatActionContext 重载；Combat 的接地/位置事实按值转给通用条件，命中/使用记录等专属条件仍读取 CombatActionContext。

#### `ActionCondition.cs`（Runtime/GamePlay/Combat/Actions）

- 由原 CombatActionCondition 直接重命名为 ActionCondition，原 meta GUID 保留；只有这一套条件基类。IsMet(in ActionContext context) 默认拒绝，通用条件覆盖它；IsMet(in CombatActionContext context) 默认把接地/位置事实转给通用入口，战斗专属条件继续覆盖原 Combat 重载，不播放动画、不切换状态、不消费输入。
- 当前实现：`CombatActionGroundedCondition` 要求 `CanEnterGrounded`，即三条地面射线至少两条命中；`CombatActionAirborneCondition` 要求 `IsGrounded == false`，即三条射线全部未命中。单条命中的平台边缘区既不算稳定落地，也不算完全腾空。`CombatActionGroundClearanceCondition` 从 Context 的检测位置竖直向下 Raycast，要求配置距离内不存在 Ground / Wall；`CombatActionRequiredHitsCondition` 检查配置的每个 Action 是否至少确认命中过一个有效 Owner；`CombatActionNotUsedCondition` 限制同一落地周期内重复进入指定 Action。
- `CombatActionGroundClearanceGizmo` 挂在 GroundSensor 上并读取同一 Clearance Condition 资源；Scene Gizmos 用绿色射线表示空间足够、红色射线和命中圆表示距离内检测到地面，保证可视化与实际判断共用同一套参数和 Raycast。

#### `CombatActionChainRuntime.cs`

- 脚本职责：保存一次连续 Action 链的运行时命中事实，内部以 `CombatActionDefinition → HashSet<GameObject>` 记录每个 Action 命中过的唯一战斗 Owner。开始新的入口 Action、完成或取消整条链时清空；Action 派生时保留。

#### `ActionEntryVelocityRule.cs`

- 已迁移的 Motion 配置：Dash X=15 + Abs(currentX)×0.4，Y=0，贴墙翻转后备用 X=15；BackDodge 两阶段局部 (-15,5)；DoubleJump 垂直 (0,19)、前跳 (5×Abs(inputX)+Abs(currentX))×Sign(inputX)、Y=19；WallSlide (0,-3)。旧 PlayerBaseConfig 的 doubleJumpForce/defaultDashSpeed/dashFactor/wallDashSpeed/backDodgeVelocity/wallSlideSlowSpeed 字段与读取代码移除；普通 Jump/WallJump 尚无 Action，仍保留旧配置。
- 验证：11 个现有 Combat Stage 的左右朝向 22 项原数值核对通过；Play Mode 直接进入 Dash、BackDodge、二段跳两阶段、WallSlide、基础攻击，六项 Rigidbody 入场速度断言通过。未做完整碰撞/连招手感回归。

- 脚本职责：仅嵌入玩家 PlayerActionStage / PlayerCombatActionStage 的入场速度规则，位于 Runtime/GamePlay/Actions。水平/垂直轴可分别保留或覆盖；水平可按朝向或输入方向计算，并可叠加 Abs(当前水平速度) × inheritedHorizontalSpeedFactor。Resolve(Vector2 currentVelocity, bool facingRight, float horizontalInput = 0f, bool useAlternateHorizontal = false) 返回结果，不直接修改刚体；alternateLocalHorizontalVelocity 用于贴墙反向 Dash，不叠加原动量。PlayerState.ApplyEntryVelocity(ActionEntryVelocityRule rule) 在入场时读取当前刚体速度、朝向、输入，并将结果写回 Rigidbody；Motion 的 PlayerState.Enter 和 Combat 的 PlayCurrentActionStage 共用此函数。

#### `ActionDefinition.cs` / `ActionStage.cs`（Runtime/GamePlay/Actions）

- `ActionDefinition` 是抽象 ScriptableObject 父类，保存入口 Command、通用 `ActionCondition[] entryConditions`，通过 ActionEntryConditions 属性暴露条件，通过只读 `ActionStage[] Stages` 暴露候选阶段；不持有执行者与运行时状态。
- `bool AreEntryConditionsMet(in ActionContext context)` 与原 CombatActionContext 重载分别遍历同一份条件列表，依次判断条件，全部成立才返回 true；无条件时通过，空引用条件输出含资源名的警告并拒绝。只查询，不消费请求或切换状态。
- `ActionStage` 保存 Animator State、Cancel Window 和复用现有 `ActionCondition` 类型的 `conditions` 数组；`AreConditionsMet(in ActionContext context)` 及 CombatActionContext 重载检查全部条件，无条件通过，缺失条件引用警告并拒绝该候选。移动配置改由各角色阶段类型保存：玩家阶段有 entryVelocityRule，AI 阶段只有 aiDisplacement；共用基类不序列化两者。原玩家配置字段名和各阶段数值保留。
- `bool TrySelectStage(in ActionContext context, out ActionStage stage)` 及 CombatActionContext 重载只检查候选 Stage 条件，按数组顺序返回第一个满足的 Stage；跳过空 Stage，空数组或全部不满足时返回 false 且 stage 为 null。入口资格仍由 `AreEntryConditionsMet` 独立检查；选择不消费请求、不缓存运行时结果，也不验证动画配置。
- 当前已接通 Stage 选择（2026-10-31）：Motion 启动和 Combat 入口/手动派生/自动派生通过 TrySelectStage 取得候选，Controller 保存本次选择；动画、取消窗口、命中窗口和战斗速度读取 CurrentStage / CurrentCombatStage，不在执行中重新选择。15 份现有动作资源已从单个 stage 迁移至单元素 stages 数组，保留原配置并添加空 conditions。二段跳已合成 Action_DoubleJump.asset，包含垂直/向前两个候选 Stage，分别配置无/有水平输入条件；Player 只引用这一份 Definition，ActionContext.MoveInput 提供选择依据。
- ActionController 已从 Combat 提取，ActionContext 是独立 readonly struct；Dash 与 DoubleJump 通过 Request / TryStartAction 接入既有 FSM，Combat 保留自身生命周期。Move 已迁至 PlayerCommandInput，通用 Effect 尚未实现。

- 本轮验证：Unity 编译及 Console 无错误；隔离检查通过首项匹配、兜底、无匹配保留请求、成功消费、跨源 Exit 和 Context 改变保留选择、战斗入口/手动派生/命中窗口/迟到结束信号/结束清理。15 份动作资源加载成功。GameScene Play Mode 中调用真实 Dash / Combat 状态 Enter、Exit，确认选中动画 Hash、Stage 登记/清理和 Dash 重力恢复；未做完整按键连招与碰撞回归。验证临时对象已删除，未保存运行时场景。

#### `ActionDashAvailableCondition.cs`（Runtime/GamePlay/Actions/Conditions）

- 继承现有 ActionCondition，IsMet(in ActionContext context) 只读取 CanDash；菜单 Game/Actions/Conditions/Dash Available。
- 资源 Assets/_Game/Data/Actions/Motions/Conditions/Condition_DashAvailable.asset 已配置到 Action_Dash.asset.entryConditions；所有既有 Dash 入口共用它。没有次数时 TryStartAction 拒绝，不切换 FSM，不消耗请求；请求超时由统一校验处理。
- Player_Dash.Enter 成功进入后调用 Player.MarkDashUsed 记录使用，落地/墙滑仍调用 RestoreAirborneResources → ResetDash 恢复。检查与记录分开，没有在状态入口重复调用 TryConsumeDash。

#### `ActionHorizontalInputCondition.cs`（Runtime/GamePlay/Actions/Conditions）

- 复用 ActionCondition，保存 `[SerializeField] private bool requireHorizontalInput`，默认 true，仅供内部 IsMet 使用，不公开配置属性；创建菜单 `Game/Actions/Conditions/Horizontal Input`。
- `bool IsMet(in ActionContext context)`：使用 Mathf.Approximately(context.MoveInput.x, 0f) 判断水平输入是否为零；勾选要求有水平输入，不勾选要求没有水平输入，不影响速度或次数。
- `Assets/_Game/Data/Actions/Motions/Conditions/Condition_NoHorizontalInput.asset` 配置 false；`Condition_HasHorizontalInput.asset` 配置 true，分别引用到统一二段跳的两个 Stage。当前仅用于 Motion Context；CombatActionContext 尚不传递移动输入。
- Play Mode 验证：场景 Player 引用统一资源，无水平输入选择垂直 Stage，左右输入均选择向前 Stage，状态动画 Hash 一致；速度分别为 (0,19)/(5,19)/(-5,19)，次数均正确消耗，用尽后拒绝且保留请求。未解锁入口拒绝也已验证。测试通过工具调用启动/状态 Enter、Exit，未做完整按键与落地回归；技能只在运行时解锁，停止后恢复。

#### `ActionNegativeYInputCondition.cs`（Runtime/GamePlay/Actions/Conditions）

- `bool IsMet(in ActionContext context)` 仅判断 `context.MoveInput.y < 0f`，无公开配置属性；创建菜单为 `Game/Actions/Conditions/Negative Y Input`。资源 `Assets/_Game/Data/Actions/Motions/Conditions/Condition_NegativeYInput.asset` 用于 BackDodge Definition 的入口条件，不区分 Normal / Perfect，不消费请求。

#### `ActionDoubleJumpAvailableCondition.cs`（Runtime/GamePlay/Actions/Conditions）

- `bool IsMet(in ActionContext context)` 只读判断：不接地且 CanDoubleJump；不消费请求或次数。
- 统一 Action_DoubleJump Data 的 entryConditions 引用 Condition_DoubleJumpAvailable.asset 和 Condition_DoubleJumpSkillUnlocked.asset，分别检查空中次数与技能；状态入口不重复判断。
- Context 保留 CanDoubleJump，移除专用 IsDoubleJumpUnlocked；通过 IsSkillUnlocked(SkillData skill) 查询任意指定技能，不预先计算二段跳解锁结果。
- Play Mode 验证：未解锁拒绝；JumpUp / Apex / WallJump 的垂直与前向入口、次数用尽拒绝、Dash 取消窗口、落地刷新通过。实际普通跳 → 二段跳 → 落地两轮通过，垂直速度 (0, 19)、前向 (7, 19)。郊狼跳优先、Fall 超过郊狼时间后的二段跳、按 Controller 配置的缓冲时间判过期、墙跳 / 攀爬与 Dash / Combat 请求消费回归均通过。解锁仅用于运行时测试，不改变场景的初始解锁状态。

#### `ActionSkillUnlockedCondition.cs`（Runtime/GamePlay/Actions/Conditions）

- 通用入口条件，私有序列化 SkillData skill 由资源配置，仅供内部 IsMet 使用；不在 Context 中保存某个具体技能的解锁布尔值。
- `bool IsMet(in ActionContext context)` / `bool IsMet(in CombatActionContext context)` 分别调用 `context.IsSkillUnlocked(skill)`；两个 Context 仍独立，缺技能集合或指定 SkillData 时拒绝。
- Condition_DoubleJumpSkillUnlocked.asset 指向 Skill_DoubleJump，已加入统一二段跳 Data 的入口条件数组；检查只读查询，不解锁技能、不消费次数或请求。
- 创建菜单为 Game/Actions/Conditions/Skill Unlocked；其他动作可配置不同 SkillData 复用同一脚本。
- Play Mode 已验证：普通与 Combat Context 对未解锁 / 已解锁的查询一致，缺集合 / 缺 SkillData 拒绝；同一 Context 能读取随后解锁的结果，其他角色和其他技能保持独立；二段跳 Data 仍检查空中次数，解锁后实际进入二段跳、消费请求与次数并保持速度 (0, 19)。本轮 Console 无 Error，解锁仅发生在运行时。

#### `MotionActionDefinition.cs`（Runtime/GamePlay/Motion）

- Motion 动作的抽象 ScriptableObject 契约，继承 ActionDefinition，定义 NeedCommand。PlayerMotionActionDefinition 保存 needCommand（默认 true）和 PlayerActionStage[]；AIMotionActionDefinition 保存 AIActionStage[]、NeedCommand 固定 false。玩家创建菜单沿用 Game/Actions/Movement Action，AI 使用 Game/AI/Actions/Motion Action。
- `Assets/_Game/Data/Actions/Motions/Commands/Command_DoubleJump.asset`：统一二段跳动作使用的指令，已绑定 InputRouter.jumpCommand 与 Player.JumpCommand；作为同一跳跃按键的请求键，地面、墙滑、悬挂状态分别消费后执行普通跳、墙跳、攀爬。
- `Assets/_Game/Data/Actions/Motions/Actions/Action_Dash.asset`：引用既有 Command_Dash；动画 Dash，不保存速度配置；取消窗口 0 允许 Attack、LauncherAttack、DiveAttack、DoubleJump。
- `Action_DoubleJump.asset`：保留原 Vertical 资源 GUID，原 Forward 资源及其 .meta 已移除。Stages[0] 使用 DoubleVerticalJump + Condition_NoHorizontalInput；Stages[1] 使用 DoubleForwardJump + Condition_HasHorizontalInput。两个 Stage 各自保存 Entry Velocity Rule，取消窗口为空：垂直阶段 (0,19)，前跳阶段基础 X=5、Y=19、继承水平速度系数=1、Use Horizontal Input=true。水平输入绝对值缩放基础 X；入场先朝输入方向翻转，再由公共函数施加速度。GameScene Player.doubleJumpAction 指向统一资源；字段的 FormerlySerializedAs 保留旧 Vertical 引用的迁移能力。
- Dash 与统一 DoubleJump Data 已由 Player 引用，入口条件及 Stage 选择经 TryStartAction 校验；二段跳 Enter 通过 PlayerState 读取 Controller 已选 Stage 的 AnimatorStateHash。Dash 的动量加成、临墙反向、关闭/恢复重力，以及向前二段跳的水平加速仍由既有 FSM / PlayerMovement 执行，玩家阶段的入场速度保存在 EntryVelocityRule 中，AI 阶段使用独立位移配置。Animator 两个二段跳 State 与 Clip 命名存在交叉绑定，本次沿用原 State 名称。

#### `CombatActionStage.cs`

- 脚本职责：继承 ActionStage，仅保存共用 Hit Window；玩家子类 PlayerCombatActionStage 保存 entryVelocityRule / horizontalDeceleration，AICombatActionStage 保存 aiDisplacement；动画、取消窗口及候选条件由父类保存。不持有运行时窗口状态。Player_CombatActionState 和命中检测器从 Controller.CurrentCombatStage 读取本次选中的战斗配置。

#### `CombatHitWindowDefinition.cs`

- `damageMultiplier` / `DamageMultiplier`：每个命中窗口单独配置的伤害倍率，默认 1，Inspector 最小值为 0。CombatHitDetector 将其用于计算本次攻击伤害；同一窗口的 attackType / AttackType 配置 Light 或 Heavy，旧资源未填写时默认为 Light。
- `knockback` / `Knockback`：统一的 DisplacementRule（距离、时长、累计进度曲线）；X 由击退 Effect 根据双方位置确定方向，Y 为世界竖直位移。玩家/AI 共用同一份配置。只有 Effects 列表包含击退资源时才执行。

- 脚本职责：嵌入 Stage 的单个命中窗口配置，保存 Window ID、`Box` / `Circle` 查询几何和一组命中效果资源。首次确认目标时依次 `ApplyEffects()`；窗口关闭、Action 被取消或检测器禁用时，对本窗口实际命中过的上下文调用 `EndEffects()`。

#### `CombatActionDefinition.cs`

- 脚本职责：抽象契约继承 ActionDefinition，Command 与入口条件由父类保存，自身保存 Transitions 和 cooldownDuration。PlayerCombatActionDefinition 保存 PlayerCombatActionStage[]，AICombatActionDefinition 保存 AICombatActionStage[]；通过 Stages / CombatStages 暴露候选阶段，沿用父类条件与阶段选择入口。
- 原脚本 GUID 保留；原单个 `stage` 字段已替换成 `stages` 数组，既有攻击资源配置已迁移至单元素数组，原动画、取消窗口、命中窗口及速度配置保留。
- 当前资源：BaseAttack01 / 02 / 03、JumpAttack01 / 02、HeavyAttack、LauncherAttack 与 Dive Start / Loop / Land。LauncherAttack 和 Dive 使用独立 Command；有序 Transition 允许同一 Action 按条件选择不同目标。

#### `CombatActionTransition.cs`

- 脚本职责：描述当前 Action 接受哪个 `ActionCommandKey`、派生必须满足的 Condition 列表，以及成功时准备哪个目标 `CombatActionDefinition`。窗口是否开放、请求是否有效和何时真正切换由运行时 Controller 判断，不写入这份静态配置。
- 当前条件派生：BaseAttack03 的 Transition 引用 `Condition_Base123ConfirmedHits`；BaseAttack01、02、03 都确认命中后才能准备 HeavyAttack。
- Dive 分支：Start 优先在真正落地时进入 Land；仍在空中且 `Condition_DiveLoopGroundClearance` 确认下方至少有 5 个世界单位空间时才进入 Loop。空间不足时 Start 的自动转换窗口继续逐帧判断，直到落地后直接进入 Land。

### Runtime/GamePlay/Combat/Effects

#### `CombatHitEffect.cs` / `CombatHitEffectContext.cs`

- `CombatHitEffect` 是命中效果 ScriptableObject 的抽象入口：`Apply()` 在目标首次被本窗口确认时执行，`End()` 在该命中窗口结束时执行；默认 `End()` 为空，因此瞬时效果无需伪造收尾逻辑。
- `CombatHitEffectContext` 是只读上下文，携带本次效果的 `SourceOwner`、`TargetOwner` 和 `WindowDefinition`。CombatHitDetector 创建上下文时传入本次命中的窗口引用，效果从窗口读取参数；效果资源不保存某次命中的运行时对象。

#### `CombatHitKnockbackEffect.cs`

- 读取命中窗口唯一的 Knockback（DisplacementRule），根据攻击者与目标的 X 位置确定方向，通过公共 CombatEffectReceiver.TryApplyKnockback(DisplacementRule rule, float horizontalDirection=1f) 调用目标。窗口不再保存 Vector2 初速度或 AIKnockback 副本，双方读取相同距离、时长和累计进度曲线。
- 当前初始配置：普通三连击距离 0.8 / 1 / 1.4，时长 0.2 / 0.2 / 0.25；重击距离 2、时长 0.3；FlyEye Attack01 距离 1、时长 0.2；下落攻击 Distance=(0,-3)、时长 0.15，修正此前迁移为水平位移的错误。均沿用先快后慢曲线，是独立手感初始值，不由旧速度自动换算。
- 不覆盖 End：命中窗口关闭不会取消目标已经接受的击退。受击动画与击退执行各自管理生命周期。碰撞可能限制实际动态刚体位移；固定向下距离不是跟随地面或落地检测。

#### `CombatLaunchHitEffect.cs`

- 先通过目标的 CombatEffectReceiver.TryLaunch 判断目标能否被挑起；成功后可同时作用于来源角色。玩家使用原 targetVerticalVelocity/sourceVerticalVelocity，带 AI Motor 的来源/目标分别使用 aiSourceDisplacement/aiTargetDisplacement，不转换速度。现有两个 Launch 资源的 AI 初始配置为向上 2、用时 0.3 秒。LauncherAttack 使用“目标与来源一起上升”，JumpAttack02 使用“只挑起目标”。

#### `CombatMotionPauseHitEffect.cs`

- 可分别暂停来源和目标的 Rigidbody2D 运动；`Apply()` 请求 Receiver 保存并冻结运动，`End()` 在同一命中窗口关闭、动作退出或阶段切换时同步恢复；来源和目标独立判空，任意一方销毁不妨碍另一方恢复。JumpAttack01 当前使用该效果，让双方在剩余命中窗口内停顿，但动画继续播放。

### Runtime/GamePlay/Combat/Receivers

#### `CombatEffectReceiver.cs` / `PhysicsCombatEffectReceiver.cs` / `AICombatEffectReceiver.cs`

- CombatEffectReceiver 是抽象效果接收契约：提供只读 CanBeKnockedBack / CanBeLaunched 与对应 SetCanBeKnockedBack(bool value) / SetCanBeLaunched(bool value)，由 Physics/AI 实现修改各自许可字段；霸体效果同时关闭两个许可，伤害许可独立。TryApplyKnockback(DisplacementRule rule, float horizontalDirection=1f) 对双方使用同一配置；BlocksLocomotion、TryBeginMotionPause、EndMotionPause 保留。击飞 TryLaunch(float, DisplacementRule) 仍沿用玩家原上抛速度和 AI 位移配置，本轮仅统一 Knockback。
- PhysicsCombatEffectReceiver 用于动态刚体，DefaultExecutionOrder(50) 在普通 Player 状态物理更新后执行。调用共享 DisplacementProgress 推进曲线，将该步增量除以 Time.fixedDeltaTime 写入速度，不使用 MovePosition。击退接管水平轴（包括下砸时 X=0）；竖直位移非零时临时关闭重力，结束或取消恢复原重力；水平击退保留自由竖直物理。旧 deceleration、MoveTowards 匀减速状态已移除，场景对应旧字段清理。
- 暂停不推进曲线；保存速度/重力并冻结动态刚体，恢复后继续剩余进度，不重新开始。关闭接收器先释放暂停再结束击退，避免把旧击退快照恢复成残留速度。动态物理仍受碰撞/摩擦影响。
- AICombatEffectReceiver 仅将同一个 Knockback rule 传给 AIDisplacementMotor；AI Motor 与动态接收器都复用 DisplacementProgress，保留各自执行状态。AI 继续使用 MovePosition；优先级和按 source 取消的规则不变。
- Knockback/Launch/MotionPause 效果通过公共基类分派。GameScene Player、动态胶囊及禁用旧 FlyEye 使用 Physics 实现，活跃 FlyEye 使用 AI 实现；组件引用与 GUID 保留。
- Play Mode：同一水平 0.8/0.2 配置，隔离动态刚体与 AI 均实测 (0.8,0)；同一下落配置动态刚体实测 (0,-3.02)（完成后恢复重力继续下落）、AI (0,-3)，暂停保持位置且恢复继续剩余进度，重力恢复为 4。真实玩家命中 FlyEye HP 100→90、位移约 0.8，Hurt 正常。另验证动态接收器在向下击退暂停中禁用后重力恢复为 4、清除控制状态且不恢复旧下降速度。Console 无 Error；测试后停止 Play Mode 并恢复 runInBackground=false。

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
  - `AnimatorStateHash`：只读暴露当前状态实际使用的 Animator State Hash，供动画事件来源校验。
- 函数：
  - `EntityState(StateMachine stateMachine, int stateName, Animator anim)`：保存状态机、动画器和动画参数。
  - `Enter()`：进入状态时通过 `Animator.CrossFade` 播放 `stateName` 对应状态。
  - `LogicalUpdate()`：每帧逻辑更新入口，默认空实现。
  - `PhysicalUpdate()`：物理帧更新入口，默认空实现。
  - `Exit()`：状态退出入口，默认空实现。
  - `CanTransitionTo(EntityState targetState, StateTransitionKind transitionKind)`：由当前源状态判断是否允许指定种类的转换；基类默认拒绝 Cancel。
- 关联：所有玩家状态继承它的动画身份和生命周期。

#### `Assets/_Game/Scripts/Runtime/Core/FSM/StateMachine.cs`

- 脚本职责：简单状态机，统一驱动当前状态生命周期。
- 属性：
  - `currentState`：当前状态。
- 函数：
  - `InitializeState(EntityState currentState)`：设置初始状态并调用 `Enter()`。
  - `ChangeState(EntityState stateChangeTo)`：Natural 转换的兼容入口。
  - `CanChangeState(EntityState targetState, StateTransitionKind transitionKind)`：无副作用地查询目标、同状态保护、Forced 规则和当前源状态权限。
  - `TryChangeState(EntityState targetState, StateTransitionKind transitionKind)`：权限成立后退出旧状态并进入目标状态，返回是否提交成功。
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
- 关联：`PlayerGround` / `Player_RunTransition` 消费 Interact Command 后调用它；实现方为 `NPCDialogueInteractable` 与 `WorldItem`。

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
  - `ClearPlayerInput(GameObject player)`：查找 Player 并调用其 `CommandInput.ClearMoveInput()`；不清除 Jump / Dash 等一次性请求。
  - `StopPlayerVelocity(GameObject player)`：查找 `PlayerMovement` 并调用 `SetRigibodyVelocity(Vector2.zero)`。
- 关联：常用于切入剧情或镜头演出前固定玩家位置；依赖玩家对象上同时存在 Player 和 PlayerMovement。

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

#### `Assets/_Game/Scripts/Runtime/GamePlay/Combat/Damage/GameplayCalculator.cs`

- 通用纯计算静态类，不继承 MonoBehaviour，不查找场景对象，不修改角色或 UI；后续其他 Gameplay 数值计算可按实际需求添加。
- `public static DamageResult CalculateOutgoingDamage(CharacterStats attackerStats, float damageMultiplier)`：正常调用入口，在计算器内部通过 UnityEngine.Random.value 生成暴击抽签值，再委托确定性重载。暴击率本身始终从攻击者 Stats 读取。
- `public static DamageResult CalculateOutgoingDamage(CharacterStats attackerStats, float damageMultiplier, float criticalRoll)`：保留用于固定抽签值的验证入口，接受 [0,1]；计算基础攻击 × 命中倍率，再按暴击结果乘暴伤总倍率。暴击率限制到 [0,1]，0% 必不暴击、100% 必暴击，攻击不低于 0，暴伤倍率不低于 1；不修改属性。
- `public static float CalculateIncomingDamage(CharacterStats targetStats, float incomingDamage)`：只读取受击方当前防御，返回 `incomingDamage * 100 / (100 + defense)`；防御小于 0 时按 0 计算，防御 100 时减半，不修改属性或生命值。
- 非法输入返回零伤害并输出警告。打出伤害由 CombatHitDetector 调用，承受伤害由 Health.TryTakeDamage 调用；旧 CalculateBaseDamage 已移除。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Combat/Damage/IDamageable.cs`

- 统一受伤接口，声明 `bool TryTakeDamage(DamageContext context)`；由攻击入口调用，当前 Health 实现只有实际减少生命值才返回 true。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Combat/Damage/Health.cs`

- 序列化 canTakeDamage 默认为 true，只读 CanTakeDamage 暴露状态，SetCanTakeDamage(bool value) 修改。TryTakeDamage 首先检查该许可，false 时直接返回 false，不扣血、不触发 Damaged。Motion 无敌效果临时关闭并在结束时恢复。


- 通用生命组件，实现 `IDamageable`。`Stats` 引用角色已有的 CharacterStats，`CurrentHealth` 为自身持有的当前生命值，初始化后生命值为 0 时 `IsDead` 为 true。
- 不包含初始化函数；Stats 和 CurrentHealth 提供 protected setter，由具体角色的 Health 子类完成初始化，对外保持只读。
- `public event Action<DamageContext> Damaged`：TryTakeDamage 实际扣血后触发一次，订阅者此时可读取更新后的 CurrentHealth。包括致命伤害；零伤害、已死亡或被拒绝的攻击不触发。事件携带本次 DamageContext，仅由 Health 内部调用；UI_HPBarView.OnDamaged 接收上下文后调用 RefreshHealth。
- `bool CanBeAttacked(GameObject attacker)`：检查攻击者非空、不是自身且其根对象 Layer 属于 acceptedAttackerLayers。字段默认允许全部 Layer，GameScene 的 PlayerHealth 配置为只接受 Enemy Layer。该函数仅判断攻击来源；生命初始化、目标归属和死亡检查仍在 TryTakeDamage 中执行。
- `bool TryTakeDamage(DamageContext context)`：检查初始化状态、目标归属、死亡与 CanBeAttacked，随后调用 CalculateIncomingDamage 读取自身当前防御，扣除生命值并限制最低为 0。实际生命值减少时输出受击 Debug（双方名字、原始伤害、实际扣血、暴击与生命变化）并返回 true，包括致命伤害；被拒绝或零伤害返回 false，不输出成功受击日志。当前不直接处理死亡表现、动画、UI 或草药治疗；成功扣血的逐次 Debug 输出已移除，事件仍保留。
- GameScene 的 Player 挂载其子类 PlayerHealth，继承受伤与死亡判定。PlayerCombatHitDetector 通过 IDamageable 调用该接口；其他目标也必须实现接口并完成初始化。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Enemy/Enemy.cs`

- 当前敌人实体入口，直接继承 MonoBehaviour，不依赖 Entity / FSM。RequireComponent 声明 EnemyHealth、CombatHurtbox 和 CombatEffectReceiver 依赖；`Awake()` 从 characterStatsData 创建独立 CharacterStats，再把同一实例传给 EnemyHealth.Initialize。通过 Stats 和 Health 只读属性访问运行时数据。
- GameScene 当前测试敌人为根对象 `Enemy_TestCapsule`，位于玩家出生点右侧（3, -4.74, 0），使用 `Assets/_Game/Data/Character/EnemyStats.asset`，初始生命 100。外观使用 `Assets/_Game/Art/Enemy_TestCapsule.png` 的红色胶囊 Sprite，尺寸 1.6 × 2.5；配置 Enemy Layer、CapsuleCollider2D、动态 Rigidbody2D（冻结旋转）和 CombatEffectReceiver，支持受击、挑起与运动暂停，不包含 AI。Enemy 在 Awake 创建独立运行时属性。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Enemy/EnemyHealth.cs`

- 继承 Health，通过 `void Initialize(CharacterStats stats)` 设置运行时属性和初始满血；拒绝空配置、非正最大生命值和重复初始化。复用基类受伤、攻击者检查、Debug 和死亡判定。
- GameScene 中 `Enemy_TestCapsule` 的 EnemyHealth 仅接受 Player Layer 攻击，CombatHurtbox.Owner 指向胶囊根对象。Play Mode 已验证满血初始化和落地；通过调试调用打开普攻第一段命中窗口并运行真实 PlayerCombatHitDetector，生命 100 -> 90，生成一个 VFX，同窗口重复检测不重复扣血。Animator 引用修正后，用户已在游戏内测试并确认攻击特效效果良好。死亡仅更新 IsDead，重新进入 Play Mode 可恢复满血。

#### 攻击命中 VFX

- `Assets/_Game/Scripts/Runtime/GamePlay/Combat/Effects/CombatHitVFXEffect.cs`：成功受伤后由命中窗口调用 `Apply(CombatHitEffectContext context)`，在 TargetOwner 附近按配置的 X/Y 随机偏移生成特效 Prefab；拒绝伤害的目标不触发特效。
- `Assets/_Game/Scripts/Runtime/GamePlay/VFX/VFXData.cs` 保存 Animator 状态名称；`VFXPlayer.cs` 在 Awake 补取缺失的 Animator 引用，Start 根据 VFXData 播放对应状态，`Destory()` 销毁自身 GameObject。两个 Prefab 显式引用 Visual 上的 Animator，根物体不再保留空 Animator。
- `VFXAnimationEvent.cs` 接收动画末尾的 `DestoryVFX()` 事件，调用所属 VFXPlayer 的销毁入口。`Animation/AnimationStates/VFX/HitFX_01.anim` 与 `HitFX_02.anim` 均关闭循环并配置结束事件；共用 `Animation/AnimationController/VFX.controller`。
- 资源：`Data/VFX/VFX_Hit_01.asset`、`VFX_Hit_02.asset`，`Data/Combat/Effects/Effect_VFX_Hit01.asset`、`Effect_VFX_Hit02.asset`，以及 `Prefabs/VFX/Hit FX_01.prefab`、`Hit FX_02.prefab`（以上均位于 `Assets/_Game/`）。普攻第一、三段及跳攻第一段、下劈循环使用 Hit01；普攻第二段、重攻、挑起与跳攻第二段使用 Hit02。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerHealth.cs`


- 玩家生命组件，继承 Health。`void Initialize(CharacterStats stats)` 从 Player 创建的同一份运行时属性设置 Stats 和初始满血；拒绝空属性、非正最大生命值和重复初始化。
- 由 Player.Awake 在创建 CharacterStats 后显式调用，不使用独立 Awake 读取 Player.Stats，避免跨组件生命周期顺序问题。GameScene 中原 Health 组件已原位迁移为 PlayerHealth，保持组件引用与序列化生命字段。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Combat/Damage/DamageContext.cs`

- 不可变伤害上下文 `readonly struct`，构造函数 `DamageContext(GameObject attacker, GameObject target, DamageResult damage, AttackType attackType)` 保存攻击者根对象、受击者根对象和攻击方计算出的减伤前结果。
- 只读属性 Attacker、Target、Damage、AttackType；保留伤害数值、暴击标记和轻/重攻击类型，不计算、不扣血。CombatHitDetector 从当前命中窗口读取类型，创建后传入目标的 TryTakeDamage。AttackType 枚举位于同目录，Light=0、Heavy=1。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Combat/Damage/DamageResult.cs`

- 不可变伤害结果，`Damage` 为攻击方打出的减伤前伤害，`IsCritical` 标记是否暴击。由打出伤害计算返回；之后调用方将 Damage 交给目标的承伤计算，并可保留暴击标记用于表现。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Stats/Stat.cs`

- 单项运行时属性的纯逻辑类；`Stat(float baseValue)` 保存不可变的有限基础值，`FlatBonus` 为累计固定加成，`Value` 为两者之和。
- `void AddFlatBonus(float amount)` / `void RemoveFlatBonus(float amount)`：增加或撤销相同的有符号数值，拒绝非有限结果并保留原值。当前由调用方记住自己的贡献，不跟踪来源，不支持百分比加成，也不统一限制各属性的取值范围。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Stats/CharacterStats.cs`

- 角色独立持有的运行时属性纯逻辑集合。构造函数 `CharacterStats(CharacterStatsData data)` 校验配置非空，并为六项配置分别创建新的 `Stat`；不同角色不会共享可变属性，配置资源不会被修改。
- 暴露 `MaxHealth`、`BaseAttack`、`CriticalChance`、`CriticalDamageMultiplier`、`Defense`、`BaseMana` 六个只读 Stat 引用。由 `Player.Awake()` 创建、通过 `Player.Stats` 读取，不包含当前生命值或当前法力值。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Stats/CharacterStatsData.cs`

- 角色基础数值 ScriptableObject，仅配置六个 float 字段并提供只读属性：`MaxHealth`、`BaseAttack`、`CriticalChance`、`CriticalDamageMultiplier`、`Defense`、`BaseMana`。
- 暴击率以 0–1 表示，默认 0.05；暴击伤害为总伤害倍率，默认 1.5（150% 伤害）。默认最大生命值 100、基础攻击 10、防御 0、基础法力值 100。
- 创建菜单 `Game/Character/Character Stats Data`；默认玩家资源为 `Assets/_Game/Data/Character/PlayerStats.asset`，由 GameScene 中 Player 的 `characterStatsData` 引用。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerBaseConfig.cs`

- 脚本职责：玩家基础参数配置资源。
- 属性：
  - `RunVelocity`、`WalkVelocity`：跑步和步行速度。
  - `JumpForce`、`WallJumpForce`：普通跳跃与墙跳施加速度。
  - `CoyoteTimeDuration`：角色离开地面后仍允许普通跳跃的时间窗口。
  - `GravityScale`、`FallGravity`：默认重力缩放与下落阶段重力缩放。
  - `FallEnterVelocityThreshold`：Ground 与 Run 地面系状态离开平台后，进入 Fall 所需的最小向下速度绝对值；配置保存正数，判断时取负值。
  - `WallSlideFastSpeed`：按下方向时的快速滑落速度；普通滑落速度迁到 Action_WallSlide 的 Stage，当前为 (0,-3)，入场和持续慢滑都读取该规则。
  - `CoastingDuration`：跑步结束滑行时间。
  - `CanEndRunEarlyDuration`：进入跑步后允许进入 RunEnd 的延迟窗口。
  - `RunBufferDuration`：跑步松手缓冲时间。
  - `ApexThreshold`：接近最高点时进入 Apex 的速度阈值。
  - Dash 基础速度、水平速度继承比例与贴墙反向速度均迁到 Action_Dash.Stage.EntryVelocityRule，PlayerBaseConfig 不再保存重复参数。
  - BackDodge 初速度已迁到 Action_BackDodge 的 Normal / Perfect Stage，当前均为相对朝向 (-15,5)。PlayerBaseConfig 只保留 BackDodgeGravityScale=4，不再提供该动作初速度。
  - `backDodgeGravityScale` / `BackDodgeGravityScale`：Back Dodge 分组中的独立重力倍率，Inspector 最小值为 0，默认及当前资源值为 4；由 BackDodge.Enter 应用，Exit 恢复进入前的倍率，不进行自动计算。
- 关联：`PlayerMovement` 读取移动/跳跃参数；跑步和跳跃状态读取缓冲、滑行和 Apex 参数。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerMotionAnimationTrigger.cs`

- 只负责 Motion 动画事件，挂在 Player/Visual 的 Animator 所在对象；Awake 从父级取得 Player，再通过 Player.MotionController 获取普通逻辑实例。
- IsAnimationFinished / CanPerformAction 供普通状态读取；ResetAnimation() 在 PlayerState.Enter 重置标记。
- StartAnimation(AnimationEvent)、EndAnimation(AnimationEvent)、EnableAction(AnimationEvent)、DisableAction(AnimationEvent) 检查 Controller 登记的当前状态 Hash 后更新标记；取消后的迟到动画事件被忽略。
- OpenMotionCancelWindow(AnimationEvent) / CloseMotionCancelWindow(AnimationEvent) 把 Window ID 和来源 State Hash 直接交给 Motion Controller；不再通知 PlayerState。
- Player_Dash.anim 的窗口事件已改为上述 Motion 方法，保留原时间点和参数。普通动画原 StartAnimation / EndAnimation / EnableAction / DisableAction 事件名保留，接收完整 AnimationEvent。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerCombatAnimationTrigger.cs`

- 只负责 Combat：Hit Window、Cancel Window、Transition Window、固定派生提交、自动派生和 FinishCombatStage；Awake 从父级取得 Player，通过 Player.CombatActionController 转发给普通逻辑实例。
- 不再持有 Player、不再保存普通动画结束/起跳标记，也不再转发普通状态窗口。场景两个 Trigger 都保留在 Visual 的 Animator 对象上。

#### `Assets/_Game/Scripts/Runtime/Input/PlayerCommandInput.cs`

- 普通 C# 类，不继承 MonoBehaviour，不挂载组件。Player 在实例字段初始化时创建一份，通过只读 CommandInput 属性暴露，各状态缓存同一实例。
- MoveInput 保存最新 Vector2；SetMoveInput(Vector2 moveInput) 由 InputRouter.OnMoveChanged 写入；ClearMoveInput() 写入 Vector2.zero。读取不消费，状态切换不清空，不参与动作条件或 FSM 切换。
- 输入链：GameInputReader.MoveChanged → InputRouter → Player.CommandInput → PlayerState.commandInput → 跑步/走路/空中/墙滑等移动逻辑。
- 离开 Gameplay、移动权限关闭、InputRouter/Player 禁用、窗口失去焦点，以及 StopPlayerStoryStepAction 都会清空方向。InputRouter.Update 只在存在非零方向时检查权限变化；权限恢复不自动恢复旧方向，等待新输入。
- 本阶段没有迁移 Jump / Dash / Attack / Interact，没有改动 ActionController 或原动作入口，不需要场景 Inspector 新引用。
- Play Mode 检查通过：单次写入方向持续跑步、松键停止、持方向打开背包并松键后返回无残留、无输入事件时移动权限变化清零、失焦与组件禁用清零。既有 StopPlayerStoryStep 实际执行后方向及速度归零，Jump / Dash 请求仍可正常消费；Console 无错误。


#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerControlArbitration.cs`

- 脚本职责：根据当前游戏层和层规则判断玩家动作是否可执行。
- 属性：
  - `CanMove`、`CanJump`、`CanAttack`、`CanDash`、`CanWorldInteract`、`CanUseItem`。
- 函数：
  - `PlayerControlArbitration(GameLayerStack layerStack, GameLayerRuleDatabase gameLayerRuleDatabase)`：构造时注入依赖；普通 C# 类，由 Player.ControlArbitration 延迟创建，InputRouter 通过 Player 访问。Player 负责查找缺失的层栈，缺配置时拒绝输入。
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
  - `HandleJump()`：设置普通跳跃速度。
  - `HandleWallJump()`：翻转到远离墙面的朝向，并设置墙跳速度。
  - `Teleport(Vector2 targetPosition)`：用于离散位置提交，同时设置 Rigidbody2D 物理位置与 Player Transform 显示位置，避免 Interpolate 下出现一帧视觉回闪。
  - `ClearPlayerVelocity()`：把刚体线速度清零，供挂边和攀爬等冻结状态使用。
  - `ClearYVelocity()`：保留水平速度并清除竖直速度，供地面状态进入时稳定落地速度。
  - `HandleMoveAndFlip(Vector2 inputMove)`：移动并根据输入翻转。
  - `HandleMove(Vector2 inputMove)`：按当前速度设置水平速度，保留当前 Y 速度。
  - `SetPlayerMoveMode(PlayerMoveType playerMoveType)`：切换步行/跑步模式。
  - `InitializePlayerMovement(PlayerBaseConfig config)`：从配置读取速度、跳跃力等参数。
- 关联：所有玩家地面/空中状态都通过它移动角色。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/Player.cs`

- 脚本职责：玩家实体入口，创建和驱动所有玩家状态。
- `characterStatsData` 为基础属性配置；CharacterStats Stats 暴露该角色独立的运行时属性。Awake 在 base.Awake 后创建实例，并将同一实例传给 PlayerHealth.Initialize(Stats)；PlayerHealth Health 属性暴露同对象的玩家生命组件，RequireComponent 声明 PlayerHealth 依赖。缺配置时输出警告，保持 Stats 为空且 PlayerHealth 未初始化，继续原有状态初始化。
- 关键字段/属性：
  - `playerBaseConfig`：玩家参数。
  - `CommandInput`：Player 独占的 PlayerCommandInput 实例，持续方向在状态切换间保留；Player 禁用时清零。
  - `Skills`：Player 创建的普通 PlayerSkills 实例，通过只读属性暴露给 Context。二段跳需要的 SkillData 改由 ActionSkillUnlockedCondition 配置，Player 不再持有专用 doubleJumpSkill。
  - `playerMovement`、`MotionAnimationTrigger`、`interaction`、`timeTool`、`groundSensor`、`wallSensor`、`MotionController`、`combatActionController`：玩家子系统引用。
  - `idleState`、`walkState`、`runState`、`runTurnState`、`runEndState`、`jumpStartState`、`jumpUpState`、`apexState`、`fallState`、`doubleJumpState`、`rollingLandState`、`wallSlideState`、`wallJumpState`、`dashState`、`combatActionState`：所有玩家状态实例。
  - `CanDoubleJump`：当前这一轮滞空是否还保留一次二段跳机会；初始可用并由 `Player` 持有，使额度可以跨越 JumpUp、Apex 与 Fall 状态。
  - `CanDash`：当前是否还保留一次冲刺机会；额度由 `Player` 跨状态持有。
- 函数：
  - `ResetDash()`：恢复一次冲刺机会，仍在成功落地和进入墙滑时调用。
  - `MarkDashUsed()`：不做判断，只标记 Dash 已使用；仅由 Player_Dash.Enter 在 FSM 真正进入后调用。是否允许进入由 Dash Data 的 ActionDashAvailableCondition 读取 ActionContext.CanDash 判断。
  - `ResetDoubleJump()`：恢复一次二段跳机会；`PlayerAir` 确认成功处理落地后统一调用，因此落到 Idle、Walk、Run 或 RollingLand 都不会遗漏。
  - `MarkDoubleJumpUsed()`：仅在 Player_DoubleJump.Enter 真正进入时标记已使用，不重复检查条件。次数由 Data 上的 ActionDoubleJumpAvailableCondition 检查；技能由独立的 ActionSkillUnlockedCondition 查询。
  - `Awake()`：调用 `Entity.Awake()` 创建状态机，并实例化所有状态。
  - `Start()`：初始化默认状态。
  - `Update()`：驱动当前 FSM 状态逻辑更新，再尝试把缓冲的战斗 Request 准备为 Action 并进入通用 Combat State。
  - `CreateCombatActionContext()`：从 GroundSensor 取得接地事实与射线检测位置，并与 Controller 的 Chain / Usage Runtime 组装本帧只读上下文，供入口和派生条件读取。
  - `TryEnterCombatAction()`：连接 Action Controller 与 Player FSM；用 Context 选择入口候选，先确认 FSM 可切换与 MotionController.CanInterrupt(action.Command)，消费请求后调用 TryInterrupt(action.Command) 清理源动作，再 BeginAction 并自然切换到 Player_CombatActionState；受击状态中不处理攻击入口。
  - `FixedUpdate()`：驱动状态机物理更新。
- 关联：继承 `Entity`；把配置、输入、移动、交互、地面检测、动画触发器整合给各个 `PlayerState`。

### Runtime/Systems/SkillSystem

- `SkillData.cs`：技能静态配置，继承 `ScriptableObject`，只读属性 `DisplayName` 暴露显示名；创建菜单为 `Game/Skills/Skill`。
- `PlayerSkills.cs`：普通 C# 类，由 Player.Skills 持有独立 HashSet；IsUnlocked 查询，TryUnlock 对有效资源首次解锁返回 true，空资源或重复解锁返回 false。Player 上的 Debug Skill 字段与 Debug/Unlock Skill 菜单提供 Play Mode 调试入口，尚未接入存档。
- `Assets/_Game/Data/Skills/Skill_DoubleJump.asset`：二段跳技能资源，显示名“二段跳”；GameScene Player 已配置 Debug Skill，运行时持有 PlayerSkills 实例；技能资源由 Condition_DoubleJumpSkillUnlocked.asset 引用。解锁使用 Player 的 Debug/Unlock Skill 调试入口，调用 Skills.TryUnlock。
- 当前调用链：InputRouter 提交 Jump Command → JumpUp / Apex / Fall / WallJump 或 Dash 取消入口读取统一 Player.DoubleJumpAction → TryStartAction 校验 Data 条件并按 Context.MoveInput 选择 Stage → FSM 进入 Player_DoubleJump → MarkDoubleJumpUsed。落地 / 墙滑保留既有 ResetDoubleJump 刷新。

### Runtime/GamePlay/Enemy/AI

- 提交前检查：Unity 编译完成，已保存编辑态资源及场景，Missing Scripts 为 0。使用隔离 PhysicsScene2D 重跑原斜向追击案例，加入 Approximately 后第 82 步成功，最终距离 1.00000012；未进行全项目自动测试或完整战斗伤害回归。

- 播放动画任务验证：Unity 编译及 13 项隔离预览场景检查通过，使用现有 FlyEye Animator Controller 实际从 Attack01 切到 FlyIdle；覆盖即时成功、重复 Tick 不重播、开始前取消、构造参数、工厂多实例、Sequence 不等待循环片段及 Owner 销毁。临时场景与配置已清理，现有场景和资源未改动。
- `AIPlayAnimationTask`（AI/BehaviorTree/Tasks）通过 AIAgentContext.Owner 查找子级 Animator，在首次 OnTick 校验可用性及第 0 层的目标状态后 CrossFade，立即返回 Succeeded；成功表示已提交播放请求，不等待循环动画结束。终态保护由 AITask 提供，重复 Tick 不重复播放；不覆盖 OnEnd，也不负责停止动画。构造函数拒绝空 Owner 与空白状态名。`AIPlayAnimationTaskDefinition`（AI/BehaviorTree/Definitions）保存 stateName，通过 StateName 只读暴露；菜单 Game/AI/Tasks/Play Animation，配置无效时警告并返回 null，每次创建独立实例。资源可配置 FlyIdle 等状态名，脚本不写死具体敌人动画；现有资源 Task_PlayFlyIdle 已用于攻击后的动画切换。
- 敌人采用行为树任务与具体执行器协作，不要求并行维护角色 FSM。EnemyState 及未引用的空 FlyIdleState 已移除；Enemy 不再继承带 StateMachine 的 Entity。Core/FSM 的 Entity / EntityState / StateMachine 保留给正在使用它们的 Player。Animator 动画状态与 CombatExecutionStatus 执行结果仍然保留，不属于此次移除的敌人 FSM。
- 已实现等待 / 消息 / 动画播放任务、顺序 / 选择组合及独立运行实例，并接入飞行追击和真实攻击。攻击分支已配置攻击日志 → AICombatActionTask → 播放 FlyIdle → 等待；Attack01 关闭循环并以动画事件报告结束。敌人命中与伤害检测尚未接入，动作资源 hitWindows 为空。
- `AITaskStatus` 定义 Running / Succeeded / Failed；本次等待只返回前两种结果。
- `AITask` 为纯逻辑抽象类，统一保存 hasStarted / hasEnded 及 public 只读 Status。非虚 Tick(float deltaTime) 首次调用 OnStart，仍 Running 时同次调用 OnTick(deltaTime)，成功 / 失败时调用一次 OnEnd(false)；结束后不再执行钩子。Abort() 将未结束任务置 Failed，只有开始过才调用 OnEnd(true)。结束标记先于回调设置，清理中的再次 Abort 幂等；OnStart / OnTick 内部取消后不再被其返回值覆盖。子类通过 protected virtual OnStart()、protected abstract OnTick(float deltaTime)、protected virtual OnEnd(bool interrupted) 实现行为，不各自维护终态。
- `AITaskDefinition` 为抽象 ScriptableObject，声明 AITask CreateTask(in AIAgentContext context)。工厂契约及资源配置不变；Sequence / Selector 向子项继续传递 Context，Wait / Log 不使用该参数。每轮仍创建新运行实例，没有 Reset / 对象池复用。
- `AIAgentContext` 为 readonly struct，保存 Owner / Target、Motor、CombatExecutor 及 ActiveArea（Area）引用，以及 SelectedAction / IdlePosition / Perception，由 EnemyAIBrain 创建根任务时传入；任务执行时读取当前位置，Definition 不保存实例上下文。通用组合、等待、日志与动画任务及其 Definition 位于 AI/BehaviorTree/Tasks 和 Definitions；敌人通用 AICombatActionTask / Definition 位于 Enemy/AI/Shared/BehaviorTree/Tasks 和 Definitions；当前飞眼专用的 AIFlyToTargetTask / Definition 位于 Enemy/AI/FlyEye/BehaviorTree/Tasks 和 Definitions。EnemyAIBrain 位于 Enemy/AI/Shared；敌人公共部分不混入具体飞眼任务。战斗任务菜单为 Game/AI/Enemies/Tasks/Combat，飞眼追击菜单为 Game/AI/Enemies/FlyEye/Tasks/Fly To Target。AIAgentContext 位于 AI 根目录，当前仍携带敌人执行组件引用。AIAttackUtilityOptionDefinition 因直接查询敌人执行器冷却而归入 Enemy/AI/Shared/Utility，菜单 Game/AI/Enemies/Utility/Attack Option；通用的 Chase / Idle Option 与候选协议保留 AI/Utility。
- 飞眼 Idle 树为日志 → Task_Wait_StartInterval；Return 树 TaskTree_FlyEye_Return 为返回日志 → FlyIdle 动画 → Task_Wait_ReturnInterval → AIFlyToIdlePointTask。Attack 树不再带末尾动作间隔等待，旧 Task_Wait_ActionInterval 已由新的 Start / Return 等待资源替代。Brain.Awake 保存自身初始世界 XY 为 IdlePosition，不再拖入 IdlePoint。返回任务从 Context 接收 Motor / IdlePosition，Definition 直接读取 Context.Perception 并显式传给运行任务，不通过 Owner.GetComponent 自行查找，创建时不改变感知；OnStart 调用感知组件的 ClearTarget，再开始返回，达到 stoppingDistance 后成功，途中允许重评。返回资源 speed=5、stoppingDistance=0.1；无寻路绕障能力。
- `AIWaitTask` 继承 AITask，每个实例独立保存 duration 与 elapsed。构造函数拒绝非正时长；重写 OnTick(float deltaTime) 累计调用者传入的秒数，达到时长返回 Succeeded。终态及取消后的计时停止由基类保证。任务自身不读取 WorldTime、不访问角色组件、不保存共享配置。
- `AIWaitTaskDefinition` 继承 AITaskDefinition，序列化 duration 并通过 Duration 暴露；菜单 Game/AI/Tasks/Wait。每次创建新的 AIWaitTask，不缓存执行实例。当前资源为 Assets/_Game/Data/AI/Enemies/FlyEye/Task/Timing/Task_Wait_ActionInterval.asset，时长 0.5 秒，攻击 / 待机分支共用其定义。非正时长警告并返回 null；当前警告只写“非法配置”，后续应补充资源名、失败条件及未创建任务的结果。
- `AILogTask` 构造时校验并保存消息，OnTick 输出后返回 Succeeded；重复输出保护由基类提供，不再保存 hasLogged。创建任务没有日志副作用。`AILogTaskDefinition` 保存 logMessage，创建独立 AILogTask，空消息警告并返回 null；菜单 Game/AI/Tasks/Log，当前资源为 Task_LogAttack / Task_LogChase / Task_LogIdle。
- `AISequenceTask` 持有子任务数组、currentIndex 和序列状态，每次 Tick 只推进当前子任务：Running 保持位置，Succeeded 推进索引，最后一个成功才结束，Failed 立即终止且不执行后续任务。终止状态保持，重复调用不重跑子任务。构造函数分别拒绝 null 数组、空数组和空子任务，并复制数组容器到 readonly 字段，避免调用者替换数组元素影响执行。`AISequenceTaskDefinition` 保存 AITaskDefinition[] children，每个位置分别创建运行实例。当前 TaskTree_FlyEye_Attack 由消息 / 攻击任务 / 播放待机 / 等待组成，TaskTree_FlyEye_Chase 由消息 / 飞行追击组成；分支不再包含顶层距离选择条件，攻击启动时复核目标，追击任务自行检查运行目标与完成 / 放弃距离，TaskTree_FlyEye_Idle 由消息 / 等待组成。
- `AISequenceTaskDefinition.CreateTask(in AIAgentContext context)` 用非序列化 isCreatingTask 标记当前同步创建过程；重复进入时警告循环引用并返回 null。标记设为 true 后的全部创建路径由 try/finally 包围，成功、提前返回和子工厂抛异常都会恢复为 false；重复进入的调用不清除外层标记。诊断包含资源名、失败条件及未创建序列的结果，子项错误包含索引。
- 顺序组合验证：36 项临时 Play Mode 检查覆盖构造参数、数组隔离、顺序 / 终止状态、时间不重复累计、相同资源重复位置与多实例独立、共享嵌套定义、直接 / 间接循环及修复后重建、空配置与子工厂 null / 异常后的标记复位、两个临时 EnemyAIBrain 的 OnEnable / Update、重新启用及成功 / 失败日志各输出一次。测试使用受控 Tick / Update 调用，未修改或保存现有场景资源；9 条警告为故意构造无效配置的预期诊断，并非真实资源新增错误。此轮未做实际按帧等待六秒的动画或移动测试。
- `AISelectorTask` 保存独立子任务数组、索引与状态；仍有候选分支时返回 Running，全部失败才返回 Failed，Tick 开头已有终态保护。Definition 已具备非序列化创建标记和 try/finally，但诊断仍误写 Sequence。飞眼的顶层分支选择已由 Utility 接管，旧 Tree_EnemyDecision 已移除；通用 Selector 组合能力保留。
- `AI/AIMotor.cs` 为抽象的二维 AI Motor 基类，持有原 EnemyFlightMotor 的 body / visual / effectReceiver 序列化引用、位置、左右朝向、翻转、带来源归属的朝向锁及旋转恢复。protected virtual Awake / OnDisable 初始化依赖与释放锁；public abstract void MoveTowards(Vector2 target, float speed, float stoppingDistance, float deltaTime) 定义移动入口。Enemy/FlyEye/EnemyFlightMotor.cs 继承 AIMotor，仅重写自由二维飞行步进，通过 AIDisplacementMotor.RequestLocomotion 提交位置，不直接写刚体。旧脚本 GUID 及序列化字段名保留。Context.Motor、EnemyAIBrain.motor、EnemyCombatExecutor 与追击/返回任务改依赖 AIMotor；Brain.motor 使用 FormerlySerializedAs("flightMotor") 迁移原引用。基类仍限定 Rigidbody2D + AIDisplacementMotor 和 Sprite 左右翻转，不提供寻路或自动巡逻。Unity 编译通过，12 项检查通过：现有场景 Body / Visual 引用保留、基类组件查找、翻转和朝向判断、基类多态调用飞行步进、朝向锁归属与旋转恢复、禁用释放以及 Context 接收基类。临时 Preview Scene 和检查入口已清理，未运行完整场景 Play Mode。
- `AI/AITargetPerception.cs` 在 FixedUpdate 未检测到目标时调用 public bool TryDetectTarget()。不再依赖 defaultTarget：序列化 targetLayers（默认 Player，位掩码 64）/ AIMotor motor / Health health / viewDistance。使用对象所属 PhysicsScene2D.OverlapCircle 获取目标层候选，按候选 Collider bounds.center 检查距离与左右朝向，再沿候选方向 Raycast；视线查询不按目标层过滤，所有层的非 Trigger Collider 均参与遮挡；非目标层 Trigger（如 Player/InteractionDetector）跳过，目标层 Trigger 仍可被识别。忽略自身层级和同 Rigidbody2D 的 Collider，取射线上距离最近的外部命中，若非 targetLayers 则该方向不可见；候选中选择最近的有效首命中。记录命中的 attachedRigidbody.gameObject（无刚体则 Collider 对象）为 GameObject DetectedTarget，再触发 event Action<GameObject> TargetDetected。使用复用 List 接收查询结果，不使用固定容量截断。发现结果保持到 ClearTarget / OnDisable；保留 Health.Damaged 直接记住 context.Attacker 的现有行为，Health 可空且安全订阅/取消订阅。没有对已发现目标持续执行遮挡检查。OnDrawGizmosSelected 绘制距离圆；只有有 Collider2D 且 Collider 层包含在 Target Layers 中的对象可被视觉发现。Unity Preview Scene 中 11 项检查通过：此前普通/Trigger 遮挡、自身排除、无遮挡发现、事件单次通知、锁定和重置、背向/超距离拒绝、第一目标优先、非目标首命中不穿透、子 Collider 归属和空 LayerMask。临时场景与入口已清理，未进行完整场景 Play Mode 验证。随后实际场景射线确认 InteractionDetector（Default 层 Trigger，距离约 7.18）先于 Player 身体（距离约 8.63）命中，旧规则因此挡住玩家；已改为忽略非目标层 Trigger。使用实际 Player / InteractionDetector bounds 的 4 项隔离检查通过：交互 Trigger 不遮挡、同几何非 Trigger 遮挡、目标 Trigger 可检测、朝向条件保留；不修改真实场景。Brain 持有感知组件，创建观察/执行/调试 Context 时只传 perception，不读取或传递独立 Target。Context 构造函数签名为 AIAgentContext(Transform owner, AITargetPerception perception, AIMotor motor, EnemyCombatExecutor combatExecutor, ActiveArea area, ActionDefinition selectedAction = null, Vector2? idlePosition = null)，保存 Perception，并从 perception.DetectedTarget.transform 生成只读 Target 快照；无组件/无目标时快照为空。Brain 的 Target 字段/属性均移除。Utility 继续读取 Context.Target 快照；返回 Definition 直接使用 Context.Perception，OnStart.ClearTarget 保留。之前“不传 Perception、由 Brain 读取 Target”方案已撤回，其 8 项检查不代表当前构造方式的验证。
- `EnemyAIBrain` 是角色 AI 运行入口，配置 flightMotor / combatExecutor / intentTaskMap / utilityOptionSet / perception / repeat。OnEnable 与上一轮任务终止后的 FixedUpdate 调用 TryStartNextTask：构造观察 AIAgentContext，由 AIUtilityDecisionMaker 选择 AIUtilityEvaluation，通过 intentTaskMap.TryGetTask(intent.Key) 查任务配置，再将选中 Target / Action 传入执行上下文创建根任务。FixedUpdate 在推进任务前调用 TryReplanCurrentTask，当前根 Running 且 CanBeReplanned 时重新选择；Suspend / OnDisable 清空引用并 Abort 旧根。当前重评在创建候选任务之前以 Key / Target / Action 相同识别重复选择，避免重复分配。TryStartNextTask / TryReplanCurrentTask 均显式把 intent.Action 传给执行上下文的 selectedAction，并先成功创建候选任务再 Abort 旧根；创建返回 null 时返回 false，保留原任务与观察信息，重选路径输出说明保留旧任务的警告。FixedUpdate 在重评后再次检查 IsSuspended / currentTask，确认有效后继续检查状态及 Tick，避免空引用。仍有待完善边界：Sequence / Selector 按任意孩子可重评来放行，尚未限定活动孩子。
- `EnemyAIBrain` 以只读 CurrentEvaluation / CurrentTaskDefinition / CurrentTaskStatus / CanReplanCurrentTask 暴露实际任务信息。两条创建路径在任务创建成功后保存选中时的评估，AbortCurrentTask 同步清空；CurrentEvaluation 的分数与原因是选中时快照。`bool TryEvaluateUtilityOption(AIUtilityOptionDefinition option, out AIUtilityEvaluation evaluation)` 使用该敌人的实时上下文调用候选评估，只返回观察结果，不选择、不创建、不取消任务。
- `Assets/_Game/Scripts/Editor/AIUtilityDebugWindow.cs` 为可停靠 EditorWindow，通过 `Window > Game > AI Utility Debugger` 打开。默认跟随 Hierarchy 选择的敌人或其子对象，也可关闭跟随并拖入 EnemyAIBrain 固定观察。Play Mode 每 0.1 秒按候选配置顺序采样：显示实际意图、具体动作、目标、任务树、任务状态、途中重评权限及选中时分数；逐项显示实时分数、可行性与原因，评估失败单独标记，不当作可行零分。最高分只在有效且可行的结果中比较，同分保留靠前项，支持零分 / 负分；实时最高分并不代表正在执行。窗口在 AI 挂起或当前任务不可重评时仍可读取评分，敌人物体未激活时不采样，退出 Play 清空采样。此窗口只用于 Editor，不加入场景或构建，也不改写共享 SO。
- `Runtime/UI/AIUtilityTooltip.cs` 挂在 GameScene 的 `Enemy_FlyEye/AIUtilityTooltip` 子物体，引用同角色 Brain。每 0.1 秒读取当前选择与实时评分，OnGUI 经 Camera.main.WorldToScreenPoint 锚定头顶。中文精简显示：当前行为及每个候选的两位小数评分，不可行项显示“不可用”，配置 / 评估错误单独提示；不显示资源长名、详细原因、历史分数、重评权限或重复的最高分行。显示别名近战 / 突进 / 远程 / 追击 / 待机不参与决策。默认 Head Offset=(0,1.5,0)、Panel Width=400、Font Size=30，使用项目 Hexin Pixel Font 中文字体。仅观察，不改写任务或评分；禁用子物体即可隐藏，完整诊断仍由 Editor 调试窗口提供。 已在实际 Play 画面确认中文字体正常、五行内容不折行；验证后退出 Play。
- 调试窗口验证：Unity 编译通过，GameScene 实际 Play 中读取 FlyEye 的 Attack01 / Attack02 / Chase / Idle 四个候选，确认距离、朝向、冷却导致的不可行原因及实时评分；实际攻击仍执行时，窗口可显示不同的实时最高分。已目视检查窗口排版，并受控调用 Suspend / Resume 验证实际选择与任务配置引用清空 / 重建；验证后退出 Play，恢复 runInBackground 原值 false，保留调试窗口。后续已修复 SelectedAction 漏传及重选失败清空任务的问题：Unity 隔离 Preview Scene 中 9 项回归检查通过，覆盖具体攻击动作传递、创建失败保留旧任务、FixedUpdate 失败后安全推进、成功切换取消旧任务、相同意图跳过工厂，以及 TryStartNextTask 的成功/失败路径；临时测试入口与对象已清理。该回归未覆盖完整场景动画与战斗表现。
- 意图数据位于 `Assets/_Game/Scripts/Runtime/GamePlay/AI/Intent/`：`AIIntentKey` 为无运行态的 SO 资源标识，菜单 `Game/AI/Intent Key`，资源身份用于区分意图，不依赖字符串或中央枚举。`AIIntent` 为 readonly struct，提供只读 Key / Target / Action 属性，构造函数 `AIIntent(AIIntentKey key, Transform target, ActionDefinition action = null)` 拒绝空 Key，允许空 Target / Action；目标是否合法由具体策略及任务检查。Action 携带本次选择的具体动作，同一个 Attack 意图可执行不同攻击。值类型的 default 仍会产生空 Key，不能视作有效决策。`Assets/_Game/Data/AI/Shared/Intent/` 已创建 Intent_Idle / Intent_Chase / Intent_Attack 三个资源。意图已接入正式运行链：候选产生 AIIntent，选择器比较后由运行入口通过行为映射创建任务。
- AI 命名约定：UtilityOptionDefinition 表示可配置的候选选项，UtilityEvaluation 表示一次评估结果，UtilityDecisionMaker 比较评分选出意图；IntentTaskMapDefinition 只把意图标识映射到任务配置；EnemyAIBrain 组织选择与任务运行。通用 AITask / TaskDefinition / TaskStatus 位于 AI/BehaviorTree，意图位于 AI/Intent，映射位于 AI/Execution，AIAgentContext 位于 AI 根目录且仍包含当前敌人执行组件引用。EnemyAIBrain 的 intentTaskMap 使用 FormerlySerializedAs 保留旧字段数据，目标不再使用 Brain 上的序列化字段；原 utilityOptions 数组已迁移为 utilityOptionSet 资源引用，GameScene 的飞眼绑定同步更新。
- `AI/Utility/AIUtilityOptionSetDefinition.cs` 为共享候选列表 SO，菜单 `Game/AI/Utility/Option Set`，序列化 options 数组，以 IReadOnlyList<AIUtilityOptionDefinition> Options 提供读取入口，不保存角色运行态。`Assets/_Game/Data/AI/Enemies/FlyEye/Utility/UtilityOptionSet_FlyEye.asset` 按攻击 / 追击 / 待机顺序引用三个选项，GameScene 的 Enemy_FlyEye 通过 EnemyAIBrain.utilityOptionSet 引用此资源。每轮 TryStartNextTask 读取 optionSet.Options 交给 AIUtilityDecisionMaker，缺少配置时警告并早退；评分、同分顺序与任务执行规则不变。
- AIReturnUtilityOptionDefinition 保存 IntentKey / returnTask（引用 Task_FlyEye_ReturnToIdlePoint 配置，复用 StoppingDistance），独立重写 IsFeasible，不调用要求区域内的基类。Owner 越出原始 ActiveArea 或 Target 越出带 TargetBuffer 的扩展区域（边界算内）、距离超过 Area.DiagonalLength（沿用 Chase 最大范围，近似相等算内）、Context.Target 为空，任一成立则可用且 score=1；其余不可用且 score=0。Target 缺失也作为返回条件；Owner / Area / Key 缺失为配置错误，TryEvaluate 警告返回 false。Intent_Return.asset 已创建，UtilityOption_Return 已绑定；IntentTaskMap 将 Return 映射 TaskTree_FlyEye_Return。Return 在 OptionSet 首位，同分优先于 Attack/Chase；IsFeasible 优先比较 Owner 与 Context.IdlePosition 的距离，小于等于 returnTask.StoppingDistance 或 Mathf.Approximately 相等时不可用、评分 0，即使未发现目标或目标越界也不返回；到达之外才判断其他返回条件。IdlePosition / 返回任务配置缺失、位置非有限或停止距离非法时 TryEvaluate 警告并返回 false。头顶调试显示“返回”。此前 Unity Preview Scene 中 12 项检查覆盖返回条件、评分与映射、OnStart 清除时机；其中“已到返回点但未发现目标仍可用”规则已替换。当前额外 7 项 Preview Scene 检查通过：到达点、容差内/边界/外、共享停止距离调整、正式选择器到达后选 Idle；测试入口和对象已清理，尚未验证完整场景 Play Mode。
- Utility 候选协议位于 `Assets/_Game/Scripts/Runtime/GamePlay/AI/Utility/`：`AIUtilityEvaluation` 是 readonly struct，只读保存 Intent / IsFeasible / UtilityScore / Reason。构造函数拒绝空意图 Key（ArgumentException）和非有限评分（ArgumentOutOfRangeException），不限制评分区间，空 Reason 转为空字符串；default 结构体仍无有效意图，不可直接参与选择。`AIUtilityOptionDefinition` 为抽象 SO，声明 `bool TryEvaluate(in AIAgentContext context, out AIUtilityEvaluation candidate)`；true 表示评估完成，可通过 IsFeasible=false 表达当前不可执行，false 表示配置或输入错误导致评估无法完成。具体实现应在评估错误时警告并早退，不把角色上下文或评分缓存回共享 SO。当前已有攻击 / 追击 / 待机候选与纯逻辑 AIUtilityDecisionMaker，EnemyAIBrain 在每轮开始时通过选择器选出意图并驱动正式任务。
- `AIUtilityOptionDefinition.IsFeasible(in AIAgentContext context)` 为公共可用性入口：基类要求 Owner / Target / Area 存在，Target 由 Context 构造时从传入的 Perception 感知结果生成，非空即表示已发现，Owner 的世界 XY 位于 ActiveArea 原始边界内，Target 位于向四边各扩展 TargetBuffer 的边界内（含边界），任一缺失或越界均不可用。Chase / Attack 重写并先调用 base.IsFeasible，再执行各自运行条件；Idle / Return 各自重写且不调用基类，跳过公共区域和已发现目标要求；Return 自行检查返回条件。正式选择和调试均通过 TryEvaluate 调用该入口，不在共享 SO 中缓存状态。TryEvaluate 保留配置错误校验及评分，配置错误与正常不可行仍分别返回 false 与 true；不可行 Reason 为条件摘要。EnemyAIBrain 创建上下文时传入 activeArea，Awake 缺引用时仅尝试 transform.root.GetComponent<ActiveArea>()；未自动搜索其他对象。
- 攻击候选 `Enemy/AI/Shared/Utility/AIAttackUtilityOptionDefinition.cs` 保存 IntentKey、CombatActionDefinition Action、minRange / maxRange 与 distanceUtilityCurve。TryEvaluate 校验 Owner / Key / 执行组件、有限的 0 <= minRange < maxRange、非空曲线及非负有限冷却配置；错误时警告并返回 false。IsFeasible 先调用基类活动区域检查，再检查动作冷却、目标存在且激活且非自身、有限距离位于含边界的 [minRange,maxRange]（容许 Mathf.Approximately）以及面朝目标；不通过则 TryEvaluate 返回成功但不可行、评分 0。通过后用 Clamp01((distance-minRange)/(maxRange-minRange)) 采样曲线，检查有限输出并 Clamp01 得到评分；Reason 保留成功时距离和评分明细，不可行时列出可能未满足的条件。尚不检查全部动作许可。
- `AI/Utility/AIIdleUtilityOptionDefinition.cs` 为待机兜底候选，配置 IntentKey / UtilityScore，Owner 或 Key 缺失、分数非有限时警告返回 false；IsFeasible 重写后仅要求 Owner / Key 有效，不调用基类活动区域检查，以便区域外仍可选择待机返回；TryEvaluate 使用该结果，评分取配置值。UtilityOption_Idle.asset 引用 Intent_Idle，基础评分为 0。`AI/Utility/AIUtilityDecisionMaker.cs` 为无状态纯逻辑类，`bool TrySelect(in AIAgentContext context, IReadOnlyList<AIUtilityOptionDefinition> options, out AIUtilityEvaluation selectedEvaluation)` 顺序评估，跳过空项、评估失败、无效结果及不可行项，从可行项中选最高评分；同分保留数组中靠前项，首个可行结果即使零分或负分也有效。没有可行项时返回 false，输出 default。选择器已接入正常任务创建，尚未实现重评惯性或团队协调。
- 追击候选 `AI/Utility/AIChaseUtilityOptionDefinition.cs` 保存 IntentKey、minRange 与 distanceUtilityCurve，不再序列化 maxRange 或暴露固定 MaxRange 属性。最大距离每次读取 context.Area.DiagonalLength（sqrt(width²+height²)），同时用于范围判断和 Clamp01((distance-minRange)/(maxRange-minRange)) 曲线归一化。要求有限的 0 <= minRange < 对角线长度和有效 Owner / Key / 非空曲线；配置错误或距离/曲线输出非有限时警告返回 false。IsFeasible 先检查 Owner / Target 均在活动区域内，再要求 Motor 存在、目标激活且非自身、距离严格大于 minRange（近似相等也视为到达）且不超过对角线（含近似边界）。不检查当前朝向，背向时只要区域与距离条件满足也可追击；距离小于等于 minRange 时仍不可用，不恢复之前的近距离转向例外。无可用攻击/追击时由 Idle 兜底。有效评分仍由距离曲线生成；不检查路径可达性。UtilityOption_FlyEye_Chase.asset 保留 Intent_Chase、minRange=2 和线性上升曲线，旧 maxRange=20 已删除。移除朝向限制前，Unity 隔离 Preview Scene 中 7 项检查通过：6×8 区域对角线为 10、对角线端点评分为 1、中间距离评分、当时的背向拒绝、动态区域尺寸、最小距离与目标越界；其中背向拒绝已不适用于当前规则。移除朝向限制后尚未重新进行行为验证。
- 行为映射位于 `Assets/_Game/Scripts/Runtime/GamePlay/AI/Execution/AIIntentTaskMapDefinition.cs`，为 SO 配置，菜单 `Game/AI/Intent Task Map`。同文件的可序列化 AIIntentTaskBinding 保存 IntentKey / TaskDefinition；bindings 数组按资源引用匹配，不按名字比较。`bool TryGetTask(AIIntentKey key, out AITaskDefinition taskDefinition)` 查找对应任务；空输入、空数组、找不到、匹配项重复或任务缺失时警告并返回 false，失败输出保持 null。不创建或执行任务，不读取或修改意图目标。`Assets/_Game/Data/AI/Enemies/FlyEye/IntentTaskMap/IntentTaskMap_FlyEye.asset` 已配置 Intent_Attack -> TaskTree_FlyEye_Attack、Intent_Chase -> TaskTree_FlyEye_Chase、Intent_Idle -> TaskTree_FlyEye_Idle。映射已配置给 GameScene 的 Enemy_FlyEye 并用于正式任务创建。
- GameScene 启用的 Enemy_FlyEye 使用 IntentTaskMap_FlyEye 与 Return / Attack01 / Attack02 / Chase / Idle 候选，目标通过 AITargetPerception 获取（Target Layers 为 Player），repeat 开启；Return 映射 TaskTree_FlyEye_Return。旧 AIDecisionDefinition、EnemyDistanceDecisionDefinition 及 Decision_FlyEye 资源已在核实无其他引用后移除；旧顶层决策树 Tree_EnemyDecision、Condition_AttackRange / Condition_DetectRange 资源，以及无其他使用者的 AITargetInRangeTask / AITargetInRangeTaskDefinition 脚本已连同 meta 移除。Utility 候选保留评分与范围配置，动作系统保留入口约束。
- Utility 接入的此前验证（早于冷却改动）：Unity 编辑器动态命令编译执行成功；使用真实候选与映射资源，在临时对象上验证距离 1 / 2 / 5 / 20 / 30 分别选择 Attack / Attack / Chase / Chase / Idle，并解析到对应分支，全部通过。临时对象已销毁。Play Console 已确认 Enemy_FlyEye 在距离 7.4164 时经 Utility 选择 Chase（评分 0.3009）并创建 TaskTree_FlyEye_Chase。暂停 Play 的受控任务推进命令遭工具空引用错误，未完成攻击 / 待机启动及完整行为回合验证；Console 查询未见游戏侧 Error，已退出 Play。加载中的场景只同步飞眼 AI 引用，未整体保存或重载其余未保存编辑。
- Sequence / Selector 已迁移到 OnTick，保留每步最多推进一个孩子的组合规则，不再自行保存终态。OnEnd(true) 仅向有效索引处的当前孩子传播 Abort，已完成孩子保持结果，未来孩子不被启动；嵌套组合沿当前分支传递取消。飞行追击通过 OnTick 推进；目标不存在、自身或禁用时失败，仍按原 stoppingDistance / giveUpDistance 完成或放弃，不添加顶层战术选择。
- 生命周期验证：Unity 编译通过，22 项受控检查覆盖首次启动、成功 / 失败终态、未开始即取消、运行中取消、重入取消、等待停止计时、嵌套组合传播、孩子之间取消、全失败索引安全、单步组合推进，以及 EnemyAIBrain 禁用 / 重新启用 / 循环 / 活动根替换。测试使用临时预览场景和显式生命周期回调，未修改场景 / 资源；不等同于真实 Play Mode 动画与战斗整合测试。
- `EnemyFlightMotor` 位于 Runtime/GamePlay/Enemy，Awake 获取 Rigidbody2D / CombatEffectReceiver，通过 public Position 暴露刚体位置。MoveTowards 按 min(speed × deltaTime, distance − stoppingDistance) 限制位移，再调用 MovePosition；受 BlocksLocomotion、组件启用及时间步长限制。visual 与 FacingRight / HandleFlip 管理朝向。Motor 不保存持续指令，不需要额外清零速度来停止 MovePosition。
- `CombatEffectReceiver.BlocksLocomotion` 为只读属性，返回 IsOnKnockback 或 isMotionPaused，Motor 据此让出击退 / 命中停顿期间的主动移动。
- `AIFlyToTargetTask` 保存 Context、速度和停止 / 放弃距离，终态由基类管理。空目标 / 自身目标 / 空 Motor 或距离超限时提前失败，到达时成功，其余情况调用 Motor。到达判断已采用 distance <= stoppingDistance || Mathf.Approximately(distance, stoppingDistance)，修复斜向移动停在 1.00000012 却一直 Running 的浮点边界；距离目前仍从 Owner.position 读取。构造数值异常类型、Owner / 目标激活及 Motor 激活校验仍可完善。Task_FlyEye_ChaseTarget 当前速度 3、停止距离以资源实际配置为准、放弃距离 10。
- 飞行追击检查：使用临时预览场景的独立 PhysicsScene2D 受控 Tick / Simulate，确认正常追击一步位移 0.06、已到达时成功且无位移、大步长限制到停止边界。复现放弃距离外仍移动 0.06、空目标 NullReferenceException、自身目标错误成功、禁用目标仍移动及禁用 Motor 时任务仍 Running。检查结束关闭预览场景，未修改现有场景 / 资源；尚未进行实际场景连续追击和受击整合验证。
- GameScene 的 Enemy_FlyEye 已连接飞行 Motor、Enemy 所持普通 CombatActionController 与 Executor、带 Animator 物体上的 EnemyCombatAnimationTrigger；EnemyAIBrain 指向 Player、IntentTaskMap_FlyEye 行为映射、三个 Utility 候选和同物体 Executor，repeat 开启。AI 资源按使用者归档：Shared/Intent 保存共用标识，Enemies/FlyEye 内按 Utility / IntentTaskMap / TaskTree / Task 整理飞眼配置；Task 内按 Combat / Movement / Animation / Timing / Debug 分类；新增敌人在 Enemies 下建独立角色目录，NPC 在 NPCs 下按角色建目录（当前未创建空 NPC 目录）。仅在实际跨角色共用时才将其他配置提到 Shared；玩家战斗配置已移动到 Data/Actions/Combat/Player，飞眼动作位于 Data/Actions/Combat/Enemies/FlyEye/Action_FlyEye_Attack01.asset，移动保留 GUID。
- AI 等待与飞行步长均使用自定义 WorldTime.fixedDeltaTime；Time.time 日志仍记录 Unity 游戏时间。Motor 不再次乘 WorldTime 倍率；CombatEffectReceiver 的击退减速仍使用 Time.fixedDeltaTime，尚未统一其时间语义。

### Runtime/GamePlay/Player/Combat


GamePlay 下的通用脚本按职责分目录：`Actions/` 保存通用动作数据、上下文和控制器，`Actions/Conditions/` 保存通用动作条件；`Motion/` 保存移动动作；`Stats/` 保存角色属性；`Combat/Damage/` 保存生命、受伤接口与伤害计算；`Environment/` 保存背景视差和循环。角色专属实现仍在 `Player/`、`Enemy/`，脚本保持全局命名空间。文件移动保留原 `.meta` GUID，配置资源位置不变。

#### `ActionController.cs`（Runtime/GamePlay/Actions）

- 普通 C# 抽象类，不继承 MonoBehaviour。构造时传入 owner，仅用于日志归属；Motion / Combat 继承并保存每个角色独立的动作与窗口状态。

- 抽象父类只共享动作运行状态、Data 取消窗口及动画来源校验；不持有 ActionRequest、输入缓冲 Config、时间戳或请求消费 API。
- 具体 Controller 通过 protected ActiveAction 提供通用 ActionDefinition，通过 CurrentStage 提供已选阶段；IsActionRunning、ActiveCancelWindowId 等只读属性由父类统一暴露。
- AllowsCancel(ActionCommandKey command) 是双方共用的只读许可查询：Command 为空拒绝；没有当前 Action 时允许尝试；有当前 Action 时必须有活动窗口且窗口接受该 Command。不消费请求。
- NotifyCancelWindowOpened/Closed(int windowId, int sourceStateHash) 只接受当前 Stage 的来源 Hash 和有效 Window ID；错误来源及错误关闭 ID 忽略，无窗口时 ActiveCancelWindowId 为 -1。共同校验函数不设 virtual，避免子类跳过窗口约束。

- `CanInterrupt(ActionCommandKey command = null)`：共同的只读打断许可。无活动 Action 允许；有活动 Action，传命令时复用 AllowsCancel 的白名单判断，不传命令（受击）时只检查取消窗口是否打开。
- `TryInterrupt(ActionCommandKey command = null)`：先检查许可，通过后调用各控制器的 InterruptCurrentAction；Combat 清理当前动作/命中窗口/派生与请求，Motion 结束当前动作登记但保留已经准备好的下一阶段，随后由 FSM Exit 完成重力等状态清理。实现放在父类，两个控制器继承同一个入口。失败不修改动作。

#### `MotionController.cs`（Runtime/GamePlay/Motion）

- Motion 阶段效果生命周期：Player/AI 两个 BeginState 重载选定阶段后统一调用 CommitState，先结束旧效果，再登记阶段并 Begin 新效果。TryStartAction 只准备阶段，不提前施加效果；EndState 通过来源动画校验后清理效果，TryInterrupt 沿用 EndState。Player.OnDisable 主动结束当前 Motion 登记，EnemyInterruptExecutor.OnDisable 已有同类清理。
- Runtime/GamePlay/Motion/MotionStageEffect 是资源契约，CreateInstance(GameObject owner) 为每次执行生成独立 MotionStageEffectInstance；实例提供 Begin/End。Controller 独立保存实例并逆序结束，防止共享资源存储角色状态，且重复 EndState 不重复恢复。
- MotionInvulnerabilityEffect 在 Begin 保存 Health.CanTakeDamage 原值并设 false，End 恢复原值。MotionKnockbackImmunityEffect 在 Begin 分别保存 CombatEffectReceiver.CanBeKnockedBack / CanBeLaunched 原值并均设 false，End 分别恢复；霸体同时屏蔽击退和击飞，不改变可受伤许可或取消窗口。
- 霸体扩展验证：Play Mode 中玩家和 FlyEye 的两个二段跳条件阶段均拒绝 Knockback 与 Launch，可受伤许可保持原值；阶段结束分别恢复原击退/击飞许可，原先 CanBeLaunched=false 时不会错误恢复为 true。Console 无 Error，测试后已停止 Play Mode。
- 资源位于 Data/Actions/Motions/Effects/Effect_Motion_Invulnerability.asset 与 Effect_Motion_KnockbackImmunity.asset；Game/Actions/Motion/Effects 菜单可创建。Action_Dash 的 Dash 阶段绑定无敌，Action_DoubleJump 两个阶段均绑定霸体（防击退与击飞）。整个阶段生效，结束/取消时恢复，未另加效果时间窗口。
- Play Mode：真实 Dash 请求成功后 CanTakeDamage=false，敌人命中窗口开放期间 HP 不变、Damaged 回调为 0、未应用 Knockback；Dash 退出恢复。两个二段跳阶段允许实际扣血、拒绝 Knockback，结束恢复 CanBeKnockedBack；重复结束不影响恢复，进入前已 false 的值结束后仍 false，Player 禁用清理成功。Console 无 Error，测试后停止 Play Mode 并恢复后台运行设置。


- 普通 C# 移动动作基类，构造时传入 owner 用于日志；共享 currentAction / currentStage、pendingAction / pendingStage、动画身份与取消规则；不移动 Rigidbody、不读取输入、不决定 AI 行为。
- `public virtual bool TryStartAction(MotionActionDefinition action, in ActionContext context)` 检查非空、重复动作、源动作取消许可、入口条件及 Stage 动画，成功时暂存目标 Action / Stage。返回 true 仅代表准备成功；实际进入仍由 FSM 完成。
- `int BeginState(int animatorStateHash, MotionActionDefinition action, in ActionContext context)` 由状态 Enter 调用，优先使用已准备的 Stage；未经准备直接进入时选择一次。返回动画 Hash；无有效动画时警告并保留状态原动画。
- `EndState(int animatorStateHash)` 只清理当前动作、Stage、动画身份和窗口，保留给下一状态的 pending Stage；`IsCurrentStateAnimation` 防止旧事件或旧 Exit 清理新动作。这些共同生命周期函数不设 virtual。

#### `PlayerMotionController.cs` / `PlayerActionRequestBuffer.cs`（Runtime/GamePlay/Player）

- PlayerMotionController 继承普通 C# MotionController；构造签名 PlayerMotionController(Player owner, ActionSystemConfig config)。Player.MotionController 延迟创建并持有同一个实例，配置迁到 Player.motionActionConfig，独立持有 PlayerActionRequestBuffer，原场景组件已移除。
- 重写 TryStartAction：仅在 NeedCommand=true 时校验有效且匹配的输入，再调用 base 的共同准备逻辑；成功后消费请求，拒绝时保留有效请求。NeedCommand=false 的自动动作不检查、不消费缓存。
- 保留玩家调用 API：RequestAction(ActionCommandKey command)、TryGetValidBufferedRequest(out ActionRequest request)、TryConsumeBufferedRequest(ActionCommandKey expectedCommand)，及 HasBufferedRequest / BufferedRequest / RequestBufferDuration。Player / InputRouter 显式使用玩家子类，Player 状态通过 player.MotionController 读取和消费缓存。
- 本次重构验证：GameScene Player 自定义组件从 12 个减到 7 个，Missing Scripts=0，编译与 Console 错误检查通过。Play Mode 验证权限、技能隔离与重复解锁、跑步水平速度、计时到期、Motion/Combat 缓存隔离、入口准备/消费、Dash 登记/退出、错误来源 Hash 忽略、完成清理及敌人转接开始/完成；使用真实 Animator 推进 BaseAttack_01_Normal，命中窗口、输入派生窗口与结束事件均到达普通控制器。未进行全招式与伤害效果回归。
- PlayerActionRequestBuffer 用 Time.time 保存请求时间，按调用方传入的 ActionSystemConfig.InputBufferDuration 检查有效期，过期清空；缺 Config 警告并拒绝验证。Motion / Combat 各持有自己的实例，缓冲时间仍为 0.1 / 0.3 秒。
- MotionSystemConfig 继续继承 ActionSystemConfig；NeedCommand 表示玩家入口是否需要输入，不限制 AI 直接选择同一动作。普通运动速度、重力与次数消费仍由 PlayerBaseConfig / PlayerMovement / 玩家状态负责。

#### `EnemyMotionController.cs`（Runtime/GamePlay/Enemy）

- 保留 MonoBehaviour 作为场景入口；Runtime 属性延迟创建普通 MotionController(this)，后续执行器通过 Runtime 使用动作逻辑。不持有玩家输入缓冲。
- AI / 运动执行器传入 MotionActionDefinition 和当前 ActionContext；使用该动作管线时，执行器须调用 BeginState / EndState 登记生命周期，无须依赖 FSM。不要求输入请求，但仍检查动作入口条件、Stage 条件及取消窗口。
- 当前只提供动作 Controller 分支；飞行位移已由独立 EnemyFlightMotor 和追击任务实现，尚未接入此 Motion 动作管线。

#### 世界交互 Command

- Assets/_Game/Data/Actions/Motions/Commands/Command_Interact.asset 使用已有 ActionCommandKey；GameScene InputRouter.interactCommand 与 Player.InteractCommand 引用同一资源，InputRouter.player 直接引用场景 Player。
- Gameplay 输入权限通过后提交到 MotionController；原地面和跑步消费入口调用 TryConsumeBufferedRequest，再执行已有 InteractionDetector.TryInteract()。附近目标、拾取背包和 NPC 对话系统保持原职责，不新增动作 Data 或动画状态。
- 交互和 Jump / Dash 共用 Motion 的单槽缓存（后一次覆盖前一次）及当前 0.1 秒有效期；过期请求不再延迟到落地后执行。对话层按键仍调用 DialogueManager.RequestAdvance()，不提交世界交互 Command。
- PlayerInputReceiver 脚本、meta、场景组件，以及 Player / PlayerState / InputRouter 引用已移除；输入路由保留 Inspector 指定的 Player，只有缺失时才查找。
- Play Mode 已验证 Idle / Run 输入只进入 Motion 缓存、消费后实际拾取 WorldItem 到背包、一次按键不重复拾取；NPC 对话能由世界交互 Command 启动，对话层按键推进实际台词且不创建世界请求。场景 Receiver 已移除，Missing MonoBehaviours 为 0。

#### `CombatActionController.cs`（Runtime/GamePlay/Combat）

- 普通 C# 战斗基类，构造时传入 UnityEngine.Object owner 作为日志上下文；共享当前 Action / Stage、pendingAction / pendingStage、命中 / 自动转换窗口、提交信号、完成状态、ChainRuntime / UsageRuntime；不持有玩家输入缓存、输入派生窗口、手动提交事件、缓冲 Config 或玩家入口动作列表。
- `protected bool TryPrepareEntryAction(CombatActionDefinition action, in CombatActionContext context)` 仅在没有当前动作时检查目标与入口条件并准备阶段；`protected bool TryPrepareStage(...)` 复用 Stage 选择与战斗动画校验，不设 virtual。子类可以使用共同能力，但无需复制规则。
- `TryQueueAutomaticTransition(in CombatActionContext context)` 只处理无 Command 的自动派生，跳过输入分支；自动窗口开放且 Transition / 入口条件与目标 Stage 通过后，准备目标并立即请求提交。`TryCommitTransition(out CombatActionDefinition targetAction)` 消费该信号并返回 pending 目标，保留准备好的 Stage 供 BeginAction 使用；自动派生不等待玩家输入或手动 Commit 动画事件。
- `BeginAction(CombatActionDefinition action)` 要求已有配对 pendingAction / pendingStage；先结束旧命中效果，再激活目标并重置窗口 / 完成状态。新链清空命中记录，派生保留记录。窗口关闭时 ActiveHitWindowId 为 -1。
- NotifyHitConfirmed、命中 / 取消 / 自动转换窗口及完成事件继续共用；输入派生窗口和手动 Commit 事件仅在玩家子类。信号必须匹配当前 Stage 动画 Hash。EndHitWindow 同步发出 HitWindowEnded，确保旧效果在新状态应用速度前恢复。
- CancelCurrentAction / TryCompleteAction 使用私有 ClearCurrentActionRuntime 统一清理；清理完成后调用 `protected virtual void OnActionRuntimeCleared()`，玩家子类扩展输入派生窗口与缓存清理。BeginAction 完成共同初始化后调用 protected virtual OnActionStarted()，玩家用它重置输入派生窗口。生命周期与窗口检查本身不开放重写。
- UsageRuntime 仍由角色流程决定何时清空；玩家落地 / 墙滑刷新保持原行为，未给敌人添加额外冷却或次数规则。
- 输入派生归属修正验证：Unity 编译通过，Play Mode 50 项临时对象检查通过；覆盖玩家输入派生等待提交、敌人自动准备后无需手动事件即可提交、混合列表相互跳过、玩家自动派生、切阶段重置输入窗口，以及取消 / 完成清理。输入派生 API 已从父类移除。

- 技能冷却：CombatActionDefinition 增加 cooldownDuration / CooldownDuration（默认 0 表示无冷却）。普通 CombatActionController 每实例持有 double 冷却时钟及动作到结束时间的字典；TickCooldowns(float deltaTime) 累计外部传入世界时间，GetCooldownRemaining(CombatActionDefinition action) 返回非负剩余秒数，StartCooldown(CombatActionDefinition action) 仅为当前正在执行的动作记录恢复时间。准备 Stage 时拒绝未冷却动作或非法时长。EnemyCombatExecutor 仅在动作启动且 TryPlayCurrentStage 成功后记录冷却；启动失败不消耗，正常结束或取消不清空，ClearActionUsage 不影响冷却。当前仅敌人接入启动记录与世界时间推进，玩家冷却尚未接入。飞眼 Attack01 已显式保存 cooldownDuration=0，待用户在 Inspector 设置实际秒数；不猜测调参值。此轮未运行测试。

#### `PlayerCombatActionController.cs`（Runtime/GamePlay/Player/Combat）

- 普通 C# 类，继承 CombatActionController。构造签名 PlayerCombatActionController(Player owner, ActionSystemConfig config, CombatActionDefinition[] entryActions)，独立持有 PlayerActionRequestBuffer；配置迁到 Player.combatActionConfig / entryCombatActions，保留四个入口攻击和 0.3 秒缓冲，原组件已移除。
- `TryGetEntryActionCandidate(in CombatActionContext context, out CombatActionDefinition action)` 先验证请求，再按 entryActions 顺序匹配 Command，调用父类 TryPrepareEntryAction；只准备、不消费，等待 Player 检查 Motion 许可及 FSM 后调用 TryConsumeBufferedRequest / BeginAction。
- `TryQueueTransition(in CombatActionContext context)` 仅存在于玩家子类：要求当前动作、无 pending、输入派生窗口开放、请求有效且 Command 匹配，跳过无 Command 的自动分支，检查 Transition / 入口条件并调用父类 TryPrepareStage。准备成功后消费输入，但仍等待手动提交信号。
- IsTransitionWindowOpen、NotifyTransitionWindowOpened/Closed(int sourceStateHash)、NotifyTransitionCommitOpened(int sourceStateHash) 均归玩家子类。动画触发器 PlayerCombatAnimationTrigger 通过 Player.CombatActionController 获取实例，手动提交事件关闭输入窗口并调用父类 protected RequestTransitionCommit()。父类和敌人不再暴露这些玩家专属 API。
- 重写 OnActionStarted 重置输入派生窗口；重写 OnActionRuntimeCleared，完成和取消时清空输入派生窗口与本实例的剩余请求；共同窗口和命中效果清理由父类保证。普通输入缓存 API 与 PlayerMotionController 一致，但实例互相独立。

#### `ActiveArea.cs`（Runtime/GamePlay/Enemy）

- 保存序列化的 width / height；OnDrawGizmos 使用 Gizmos.DrawWireCube，以 transform.position 为中心在世界 XY 平面绘制矩形线框（Z 厚度为 0）。宽高为完整尺寸，默认均为 10，不随 Transform 的旋转或缩放变化；LeftXPosition / RightXPosition / BottomYPosition / TopYPosition 按中心与半尺寸提供世界坐标边界，供 Utility 基类检查 Owner / Target 是否在区域内；只读 DiagonalLength 使用宽高计算区域对角线长度，供 Chase 作为最大距离及评分归一化上限。

- targetBuffer（Inspector: Target Buffer）默认为 1，非负，表示 Target 专用的单侧外扩距离。Contains(Vector2 position) 使用原始边界，ContainsTarget(Vector2 position) 使用扩展边界；公共 Utility / Return 统一调用这两个入口区分 Owner 与 Target。Gizmos 保留原矩形，额外黄色矩形标出目标缓冲区。原始宽高、边界属性与 DiagonalLength 不变；Chase / Return 的对角线距离规则独立保留，缓冲并不扩展追击距离。

#### `Enemy.cs`（Runtime/GamePlay/Enemy）

- 作为敌人实例的系统持有入口，保留 Stats / Health 创建和初始化，并通过延迟创建的 CombatActionController 属性持有一份普通 C# CombatActionController；Awake 在属性配置检查之前取得该实例，其他组件提前访问也返回同一实例。共享动作数据仍为 SO，动作运行状态属于每只 Enemy。
- 不接管 Utility 选择、任务推进、动画播放或 Rigidbody 运动。旧 EnemyCombatActionController 转发组件及脚本已移除；GameScene 两个飞眼对象上的适配组件已删除，执行器与动画事件引用改为 Enemy owner。Enemy.FixedUpdate 以 WorldTime.fixedDeltaTime 推进本实例控制器的冷却时钟，待机、追击及攻击期间均推进；禁用 Enemy 时暂停推进，已有记录不重置。尚未增加突进执行。

#### `EnemyCombatExecutor.cs`（Runtime/GamePlay/Enemy）

- 通过序列化 Enemy owner（缺失时 Awake 查找同物体 Enemy）读取 Enemy.CombatActionController；公开只读 CombatActionController 属性供攻击任务读取同一实例。负责单次攻击 Stage 播放及完成 / 取消结果，Status 为 CombatExecutionStatus（NotStarted / Running / Succeeded / Cancelled），NotStarted 明确表示尚未开始执行，不是角色待机动画。
- public bool TryStartAction(CombatActionDefinition action, in CombatActionContext context, Transform target = null) 校验执行器、Enemy owner、Animator 及动作引用，委托控制器开始后由 TryPlayCurrentStage 验证 Animator 第 0 层状态并播放；public void Tick() 仅检查完成，不再接收未使用的 Context，也不推进尚未接入的自动派生，不自带 Update / FixedUpdate。
- 开始时的 Context 由攻击任务提供：Enemy.CombatActionController 的 ChainRuntime / UsageRuntime、可选 GroundSensor 的接地数据（无组件时为 false）及角色位置；Executor 不内置飞行假设，也不选择攻击后的动画。public CancelCurrentExecution 由执行器失效 / OnDisable 及任务 OnEnd 使用，仅清理战斗记录和窗口，不停止 Animator。
- AIAgentContext / EnemyAIBrain 已接入 Executor。AICombatActionTask 从 executor.CombatActionController 取得 Enemy 所持有的普通 C# 实例，不再 GetComponent 查找控制器。OnStart 先检查目标存在、激活且非自身，失败时警告并返回 Failed；随后构造 CombatActionContext 并请求开始。Owner 缺失的诊断不会再解引用空 Owner；GroundSensor 仍可选。OnTick 调用 Executor.Tick，将 Running / Succeeded / Cancelled 映射为任务 Running / Succeeded / Failed；OnEnd 在 Executor 仍 Running 时取消。EnemyCombatAnimationTrigger 引用 Enemy owner，Awake 缺引用时从父级查找；FinishCombatStage 将动画来源 shortNameHash 转交同一个 Enemy.CombatActionController，通知阶段结束。地面检测目前读取现有值，任务未主动调用 GroundSensor.UpdateGroundState；角色感知更新与取消所有权还可进一步完善。敌人通过 EnemyCombatHitDetector 复用公共 CombatHitDetector；动画接收器新增 OpenCombatHitWindow / CloseCombatHitWindow(AnimationEvent)，转发窗口 ID 与来源 shortNameHash。

#### `Player_CombatActionState.cs`

- 脚本职责：所有战斗 Action 共用的 Player FSM 承载状态，不按具体招式创建 State。进入每个 Action 时播放 Stage 动画并按 `ActionEntryVelocityRule` 决定是否覆盖 Rigidbody2D 各轴速度；物理更新按 Stage 的水平减速度形成地面攻击顿挫感。
- `Exit()` 统一调用 CancelCurrentAction，再执行普通源状态退出清理；TryHandleCancel 只在许可和目标条件通过后切换 FSM，退出时同步释放命中效果。自然结束已提前清理时该操作幂等，任意外部 FSM 退出也不会遗留命中暂停。
- 命中暂停取消回归：真实命中把敌人生命 100→90 后进入 Dash，速度保持 15，后续物理帧不被旧效果覆盖；Dash 退出恢复重力 4，再次地面跳跃速度从 14.2912 降到 8.0128 并正常落地。正常命中窗口关闭、空中连段进入第二段（Y=20）、外部退出和目标销毁后的源对象清理也已验证，Console 无 Error。
- 逻辑顺序：先通过共用 TryCancelToDash 读取 Motion 请求并检查 Combat/Motion 窗口，条件通过后准备 Motion、清理攻击并以 Cancel 切换 FSM；再创建 Context 尝试 Queue 派生，然后独立消费 Commit 信号。Action 完成后，已落地则回到 Idle / Walk / Run；仍在空中则按 Y 速度回到 JumpUp / Apex / Fall。

#### 玩家与 AI 的 Action 阶段配置边界

- ActionStage 仅序列化共用的动画名、条件与取消窗口。EntryVelocityRule / AIDisplacement 为运行时只读虚属性，默认返回 null，不在共用基类保存移动配置；CombatActionStage 只追加共用命中窗口，HorizontalDeceleration 为默认 0 的运行时属性。
- PlayerActionStage 保存 entryVelocityRule 与 MotionStageEffect[] effects；PlayerCombatActionStage 保存 entryVelocityRule 与 horizontalDeceleration。AIActionStage 保存 aiDisplacement 与 MotionStageEffect[] effects，AICombatActionStage 保存 aiDisplacement；Effects 仅由 Motion 生命周期执行。因此 AI Inspector 不显示 Entry V 或玩家水平减速，玩家阶段也不显示 AI 位移字段。
- MotionActionDefinition / CombatActionDefinition 为运行时抽象契约。PlayerMotionActionDefinition / PlayerCombatActionDefinition 保存对应玩家阶段数组；AIMotionActionDefinition / AICombatActionDefinition 保存对应 AI 阶段数组。MotionController、CombatActionController 与条件选择入口仍依赖共同契约，无需按角色分支。
- 原有玩家 SO 脚本 GUID 迁到玩家具体定义，资源自身 GUID 和阶段字段值不变，仅同步具体类标识；现有 15 个玩家动作迁移时逐个核对，未修改参数。FlyEye Attack01/Hurt 两个动作改用 AI 定义，并移除 AI 资源的 entryVelocityRule、horizontalDeceleration 与 needCommand 遗留配置。AI Motion 不读取玩家输入，NeedCommand 固定 false。
- 创建玩家资源仍使用 Game/Actions/Movement Action 与 Game/Actions/Combat/Action Definition；创建 AI 资源使用 Game/AI/Actions/Motion Action 或 Combat Action。
- 编辑器验证了玩家/AI Combat 与 Motion 四类资源的实际加载类型、阶段类型及 SerializedProperty：玩家有 Entry V 且无 AI 位移，AI 无 Entry V 且有 AI 位移。Dash 配置保持基础 15、动量继承 0.4，输入水平速度 10 时结果 19。此轮为数据配置拆分，未重新进行 Play Mode 行为测试。

#### `DisplacementRule.cs` / `DisplacementProgress.cs` / `AIDisplacementMotor.cs`

- 主动动作的 AI 与玩家使用不同移动参数；Knockback 使用公共 DisplacementRule。AIActionStage / AICombatActionStage 的 AIDisplacement 保存 distance（X 为沿启动方向的距离，Y 为额外世界竖直位移）、duration、progressCurve（归一化时间→累计位移比例，端点必须为 0/1）。玩家仍读取 EntryVelocityRule/HorizontalDeceleration；AI 不把玩家速度自动换算为距离。未配置的动作距离为 0，不额外移动。
- AIDisplacementMotor 是 AI 刚体移动的唯一写入端，FixedUpdate 在默认执行顺序后运行，以 WorldTime.fixedDeltaTime 推进曲线，把每步进度差乘配置距离，经 Rigidbody2D.MovePosition 执行；每帧处理追击请求或位移，空闲/暂停时保持位置。GameScene 活跃 FlyEye 挂载此组件，FacingRoot 为 Visual；EnemyFlightMotor 通过 RequestLocomotion 提交位置，受击或动作位移期间追击不会抢占。
- TryStartDisplacement(DisplacementRule rule, float horizontalDirection, Object source, bool isHitReaction=false) 保存启动方向与来源。击退/击飞优先于普通动作位移；StopDisplacement(Object source) 只停止匹配来源，因此旧攻击取消不会清掉 Receiver 发起的新击退。暂停不推进 elapsed；组件禁用时清理并停止位置请求。移动总距离是无阻挡时的目标，碰撞处理仍由当前刚体设置决定。
- EnemyCombatExecutor 在阶段播放时提交 AIDisplacement，完成/取消时停止自己的位移。EnemyInterruptExecutor 同样执行 Motion 阶段位移，带 DamageContext 时以远离攻击者的方向作为 X 方向，否则使用当前朝向；动画结束/取消停止本执行器拥有的位移。
- CombatHitWindowDefinition 的 Knockback 已统一为 DisplacementRule；原 AIKnockback 和速度配置移除。AICombatEffectReceiver 提交公共配置到 Motor；PhysicsCombatEffectReceiver 用同一进度计算驱动动态刚体速度。双方主动动作的配置分离仍保留。
- Play Mode 验证：无碰撞临时 AI 配置 (2,1) 实测 (2,1)；暂停期间位置不变，恢复完成剩余距离；取消旧来源保留新击退、动作不能抢占击退。真实玩家命中 FlyEye HP 100→90，AI 实际击退 0.8000001，玩家无 AI Motor。真实 Combat 执行器阶段位移 1、提前取消停止移动，Motion 外部执行器阶段位移 (0,1)，均通过。Console 无 Error。测试后已停止 Play Mode、清理临时资源和对象、恢复 runInBackground=false。

- DisplacementRule 已从 AI 目录移至 Runtime/GamePlay/Actions（保留 .meta GUID），因为配置被双方击退共用。新增纯 C# DisplacementProgress，负责校验并开始一次位移、TryAdvance(float deltaTime, out Vector2 delta) 计算曲线进度差、Clear 结束；它不持有刚体、暂停、来源或优先级，各接收端独立持有一个实例。AI Motor 不再重复保存 elapsed/lastProgress/offset 计算逻辑。

#### `AIActionContext.cs`（Runtime/GamePlay/AI）

- readonly struct，只持有可空的完整 DamageContext，不包含玩家输入、技能、地面探测或跳跃资源字段。Enemy 通过 new AIActionContext(context) 包装原始命中事件；default 上下文表示没有伤害事件，避免缺省数据误判为轻攻击。
- ActionDefinition.TrySelectStage / AreEntryConditionsMet、ActionStage.AreConditionsMet、MotionController.TryStartAction / BeginState 添加 AIActionContext 重载，直接选择/准备/提交阶段，不转换为玩家 ActionContext。ActionCondition 默认 AI 重载返回 false，玩家专用条件不会因补假值而通过。
- ActionIncomingAttackTypeCondition 的 AI 重载从 context.DamageContext.Value.AttackType 读取类型，缺少 DamageContext 时返回 false；不另存或单独传攻击类型。玩家上下文与其既有条件入口保留。
- 验证：完整伤害的攻击者、目标、伤害值、暴击与类型不丢失；轻/重条件选择正确、无伤害不满足类型条件、AI Motion 准备阶段能正确提交。Play Mode 真实玩家命中使敌人 HP 100→90、执行器运行且 AI 暂停，动画结束后 IsSuspended=false；Console 无 Error，测试后已停止 Play Mode 并恢复后台运行设置。

#### FlyEye Attack02：锁定目标方向的突进

- AICombatActionStage 新增 DisplacementDirection（AICombatDisplacementDirection.Facing / TowardsTarget）。Facing 沿用当前朝向；TowardsTarget 在执行开始时计算并保存 target.position-owner.position 的二维单位方向，重合时使用当前左右朝向。Attack02 使用 TowardsTarget，不持续跟踪目标，玩家中途换位不会改变方向。
- AICombatActionTask 从本次上下文 SelectedAction 构造通用战斗任务，OnStart 转向后将 context.Target 作为第三参交给 EnemyCombatExecutor.TryStartAction。Executor 在阶段启动时锁方向、请求位移；LockedDirection 在完成/取消时清理。普通攻击动画 CrossFade 显式从 normalizedTimeOffset=0 开始，修复连续播放同一状态时无法重新收到结束事件的问题。
- AIMotor.TryLockFacing(Vector2 direction, Object source) 保存 Visual 原局部旋转，先处理左右镜像，再按 atan2(direction.y,direction.x) 设置世界 Z 角；左向镜像扣除 180°，保持 Sprite 的可见前方与 dir 一致。锁定期间 HandleFlip 不修改朝向；ReleaseFacing 仅接受锁的拥有者，恢复原旋转并释放锁。完成、取消、禁用均清理；曲线位移先完成时，朝向锁仍保持到攻击结束。
- AIDisplacementMotor / DisplacementProgress 新增 Vector2 direction 重载，统一计算 direction.normalized * Distance.x + Vector2.up * Distance.y；原 float 左右入口转换后复用。现有玩家/AI Knockback 的 X/Y 语义保持不变。
- CombatHitDetector 的 Box 查询使用 hitboxRoot.eulerAngles.z；Gizmos Box 使用相同旋转且保持世界尺寸。偏移继续经 Visual.TransformPoint，圆形半径不变，因而倾斜时攻击盒子与模型一起转向。
- 创建 Data/Actions/Combat/Enemies/FlyEye/Action_FlyEye_Attack02.asset：Attack02 动画，二维方向锁，当前位移距离 5、用时 0.4 秒；曲线归一化 0–0.25 保持 0，0.25–0.85 前进至 1，之后保持 1（蓄力约 0.1 秒，移动约 0.1–0.34 秒）。当前冷却 10 秒；Attack01 冷却 1 秒。Heavy 类型、伤害倍率 1.5，前方圆形命中窗口，Knockback 距离 1.4/0.2 秒。均为初始手感配置。
- Attack02.anim 在 0.13333334 / 0.4 秒开关命中窗口，0.4 / 0.5 秒开关取消窗口，0.53333336 秒 FinishCombatStage。创建 UtilityOption_FlyEye_Attack02，复用 Intent_Attack，攻击范围 [2,6]、距离曲线在中间峰值 1；接入现有 OptionSet 的 Attack01 后、Chase 前。攻击候选新增 minRange，默认 0 保持 Attack01 原行为，评分归一化改为 (distance-minRange)/(maxRange-minRange)。不新增 Intent 或攻击树。
- Play Mode：通过当前 Task_FlyEye_Attack 通用任务连续执行，右上 (0.6,0.8) 实测 (2.4,3.2)、左上 (-0.6,0.8) 实测 (-2.4,3.2)、竖直向下实测 (0,-4)；每次均验证可见 Sprite 前方与 dir 对齐、途中目标换边和 HandleFlip 被忽略、完成恢复旋转。验证近距选原攻击、中距选 Attack02、冷却期间拒绝 Attack02、中途取消停止位移并恢复旋转。真实动画窗口命中玩家造成 Heavy 伤害 15（HP 85→70）并进入 HeavyHurt，攻击正常完成。
- 验证期间修正了测试脚本旧任务资源名，以及连续动画未重启的问题；另一次定点命中采样未命中后，在可见场景中重新观察了真实重击成功。最终回归无新增游戏错误；测试后停止 Play Mode、恢复 runInBackground=false，运行时位置/开关未保存。Attack03 留待远程攻击切片开发。

#### 攻击朝向资格与任务执行前转向

- AIMotor.IsFacingTarget(Vector2 targetPosition) 是共用只读判断：比较目标 X 与角色 X，并结合 FacingRight；Mathf.Approximately 同 X 时视作已面朝，不强制翻转。
- AIAttackUtilityOptionDefinition 要求 Context.Motor 存在；目标有效、距离可攻击且面朝目标时才进行距离曲线评分。背向时返回正常完成的不可行评价，原因说明 facing away；Utility 不调用 HandleFlip，不改变角色表现。
- AICombatActionTask.OnStart 在校验目标和控制器后，再检查朝向；目标在评分与任务执行之间换边时调用 Motor.HandleFlip(target.position-owner.position)，然后构造 CombatActionContext 并启动攻击，不在攻击进行中持续追踪转向。
- Chase 当前不要求面朝目标，Utility 仅检查区域、目标、执行组件及距离条件；之前近距离背身 Chase 的特殊放行仍已移除。AIFlyToTargetTask 在执行时通过 HandleFlip 面向目标，再判断停止距离及移动。Attack 的朝向要求不变。
- 此前朝向规则的 Play Mode 验证（近距离 Chase 转向例外已移除）：背身攻击不可行，近距离背身选 Chase 并原地转向，下一次可选 Attack；目标在选定后换边，真实 Combat Task 在启动前完成转向；超范围仍拒绝，目标同 X 不强制转向。Console 无 Error，测试后停止 Play Mode；未改动资源范围、评分曲线或候选顺序。

#### `EnemyInterruptExecutor.cs` 与敌人外部打断链路

- Enemy.Awake 获取可选 EnemyInterruptExecutor，OnEnable/OnDisable 订阅/退订 Health.Damaged；Enemy 持有 hurtAction 配置，TryEnterHurtState(DamageContext context) 校验目标归属并将完整 DamageContext 放入独立 AIActionContext，调用 InterruptExecutor.TryStartAction(hurtAction, in facts)。没有执行器的敌人仍可扣血。
- EnemyInterruptExecutor 执行外部动作打断，内部使用普通 C# MotionController 保存阶段。public bool TryStartAction(MotionActionDefinition motionDef, in AIActionContext context) 接收调用方指定的动作与条件事实，不持有 hurtAction，不依赖 DamageContext。先检查存活、当前没有外部动作在运行、资源/Animator 与 CombatActionController.CanInterrupt，再按传入 context 准备阶段；标记 IsRunning 后先 SuspendExecution 中止旧任务，清理攻击并播放选定阶段。伤害/Knockback 仍在原检测器/效果链路结算；执行器仅提交本动作的 AI 位移，不写刚体速度。
- EnemyAIBrain.SuspendExecution 先设置 IsSuspended 再 AbortCurrentTask；暂停时不 Tick、不选任务。ResumeExecution 在角色存活且 Brain 开启时重新选择任务；旧任务不恢复。重新启用 Brain 时，仅在受击仍执行中保留暂停，避免禁用/重启后永久挂起。动画与冷却更新不依赖暂停中的任务。
- EnemyCombatAnimationTrigger 新增 OpenCombatCancelWindow / CloseCombatCancelWindow，传递窗口 ID 与来源哈希；FinishInterruptAction 将来源哈希交给 NotifyAnimationFinished，只有当前外部动作的动画来源哈希匹配时才结束执行并恢复决策。执行器禁用时清理登记，角色仍有效且存活时释放任务暂停。
- 本次泛化保留脚本 .meta GUID，场景组件不重新创建；将原执行器 hurtAction 引用迁移到 Enemy，并同步动画事件名。此前以普通上下文验证外部传入动作的执行；当前已改用专用 AIActionContext，并验证真实玩家命中后的启动、暂停、动画结束与恢复；按帧等待观察到 IsRunning=false、IsSuspended=false。先前固定延迟采样观察过早，未据此修改动画时长。当前仍不接受运行中的外部动作被另一个动作替换。
- FlyEye 资源 Data/Actions/Motions/Enemies/FlyEye/Action_FlyEye_Hurt.asset 是 NeedCommand=false 的单阶段 Motion Action，轻/重攻击共用现有 Hurt 动画，不覆盖速度、无取消窗口。GameScene 活跃 Enemy_FlyEye 挂载并绑定 EnemyInterruptExecutor；Hurt.anim 在 0.4 秒发送 FinishInterruptAction。Attack01 的窗口 0 在 0.40 秒打开、0.50 秒关闭，allowedCommands 为空（允许无命令的受击打断，不开放主动命令取消）。
- Play Mode：正常 Player Combat 请求经真实动画与 CombatHitDetector 命中，敌人 HP 100→90，命中回调观察 Hurt=true、IsSuspended=true；0.42 秒后 Hurt=false、IsSuspended=false。另验证关闭取消窗口时 TryEnterHurtState 返回 false 并保留原攻击，打开后进入 Hurt、清空攻击与命中窗口。Console 无 Error。测试后停止 Play Mode 并恢复 runInBackground=false；未加入敌人死亡动画或轻/重受击差异。

#### `CombatHitDetector.cs` / `PlayerCombatHitDetector.cs` / `EnemyCombatHitDetector.cs`

- 当前场景接收链路：Player 身体 Collider 同物体配置 CombatHurtbox；FlyEye 配置受击 Collider 与 CombatHurtbox，EnemyStats 提供属性。FlyEye Attack01 的实际窗口已挂 Effect_Hit_Knockback，Knockback=(5,0)。Player 的 CombatEffectReceiver 绑定动态刚体、CanBeKnockedBack=true、Deceleration=50。Play Mode 定点真实敌人动画命中已验证 HP 100→90 且 Hurt Action 激活；逐物理帧测得 vx 约 3.686→2.372→1.058→0，累计 X 位移约 0.1423，说明击退执行但迅速衰减。测试后已停止 Play Mode、恢复 AI 开启和 runInBackground=false；未修改用户调参。


- 公共抽象 MonoBehaviour 位于 Runtime/GamePlay/Combat/CombatHitDetector.cs，负责 Physics2D 范围查询、CombatHurtbox.Owner 归属检查、同一 Action/Window 内按 Owner 去重、计算伤害、构造带 AttackType 的 DamageContext 并调用 IDamageable.TryTakeDamage。只有实际扣血成功才 NotifyHitConfirmed 并 ApplyEffects；拒绝的目标在同一窗口内也不会重复尝试。
- 玩家与敌人子类分别留在 Player/Combat 和 Enemy 目录，只通过 ResolveOwner() 获取自身 Player / Enemy，并提供 ActionController 和 AttackerStats。PlayerCombatHitDetector 保留原脚本 GUID；hitboxRoot 和 LayerMask 引用继续使用。旧的独立 shape/localOffset/boxSize/circleRadius 预览字段已移除；实际检测始终读取命中窗口。
- 公共 OnDrawGizmos 与 DrawHitWindowGizmo(CombatHitWindowDefinition hitWindow, Color color) 同时服务玩家/敌人。Inspector 的 Preview Action / Preview Stage Index（从 0 开始）/ Preview Hit Window Id 选择实际攻击资源窗口，绿色线框预览；Play Mode 打开的真实命中窗口优先绘制红色线框。偏移经过 hitboxRoot.TransformPoint，形状尺寸和半径保持世界单位，与现有 Physics2D 查询一致。编辑预览不创建角色控制器；缺引用、阶段越界或窗口不存在时不绘制。GameScene FlyEye 默认预览 Attack01 的阶段 0、窗口 0；玩家可按需指定攻击资源。
- 每个检测器独立持有命中集合与效果上下文；OnEnable 订阅实际控制器的 HitWindowEnded，OnDisable 退订并清理。窗口结束、动作结束或取消时逐一 EndEffects；FixedUpdate 保留兜底清理。窗口外的受击效果过滤尚未实现。
- GameScene 活跃 Enemy_FlyEye 已挂 EnemyCombatHitDetector，hitboxRoot 为子物体 Visual Transform（与 EnemyFlightMotor 的转向对象一致，TransformPoint 随 X 缩放镜像命中偏移），targetLayers 仅 Player（64），characterStatsData 绑定已有 EnemyStats。Attack01 的窗口 0 配置 Light、倍率 1、Knockback 效果，当前圆形范围半径 1.04、偏移 (0.96,0)；动画在 0.2 秒打开、0.4 秒关闭，原 0.53333336 秒 FinishCombatStage 保留。这些范围和时间为初始配置，尚未经手感验证。
- 当前链路：敌人动画事件 → CombatActionController 命中窗口 → EnemyCombatHitDetector 公共检测流程 → PlayerHealth.TryTakeDamage → Health.Damaged(context) → 血条更新。Unity 已编译新类型且 Console Error 为 0；未进入 Play Mode 验证实际扣血、去重或动画时机。事件包括致命伤；Player 已订阅受击事件并调用 TryEnterHurtState(context)，窗口关闭时仍扣血但不切换受击动画；效果屏蔽尚未实现。

### Runtime/GamePlay/Player/PlayerState

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/PlayerAnimationHash.cs`

- 脚本职责：集中保存 Animator 参数 hash。
- 字段：
  - `Idle`、`Run`、`RunTurn`、`RunEnd`、`Walk`、`JumpStart`、`JumpUp`、`Apex`、`Fall`、`BaseLand`、`RollingLand`、`wallSlide`、`HangIdle`、`ClimbUp`、`Dash`。
- 关联：`Player` 创建状态时传入对应 hash，`EntityState.Enter()` 使用 hash 直接 `CrossFade` 到目标 Animator 状态。

#### `Player_HurtState.cs` 与玩家受击 Action

- Player 持有序列化 HurtAction（MotionActionDefinition）与纯 C# Player_HurtState，Awake 创建状态，OnEnable/OnDisable 订阅/退订 Health.Damaged；OnDamaged(DamageContext context) 将完整命中信息交给 `public bool TryEnterHurtState(DamageContext damageContext)`。入口检查存活、FSM、资源/动画、两个控制器的打断许可，构造带 IncomingAttackType 的 ActionContext，准备条件阶段，再 TryInterrupt 两个控制器并以 Forced 切换。Forced 只用于通过窗口检查后的 FSM 提交，不绕过受击窗口。死亡表现仍未实现，致命伤不进入普通受击。
- ActionContext 新增可空 IncomingAttackType（构造最后一个可选参数，默认 null），仅携带本次命中类型；普通动作上下文未提供类型时不会满足受击条件。`ActionIncomingAttackTypeCondition.IsMet(in ActionContext context)` 比较非空类型；条件资源只存期望类型，不保存某个角色的受击状态。
- Data/Actions/Motions/Actions/Action_Player_Hurt.asset 是一个 NeedCommand=false 的 Action，两个阶段分别配置真实 Animator 状态 LightHurt、HeavyHurt；条件资源为 Motions/Conditions/Condition_IncomingLightAttack.asset 与 Condition_IncomingHeavyAttack.asset。两个阶段均关闭入场水平速度覆盖；击退由攻击窗口的 Knockback 效果负责。HurtState.SetHitSourcePosition 记录攻击来源，Enter 让玩家面向来源，不在受击状态另行施加速度。两阶段均配置取消窗口 0，仅接受 Dash Command。选中的阶段由 MotionController 的 pendingStage 保存，HurtState.Enter 经 PlayerState.Enter 提交，不需要把受击类型长期保存在 Player 上。
- 两个动画关闭循环，在片尾分别 0.88 秒与 1.1333333 秒调用现有 PlayerMotionAnimationTrigger.EndAnimation；来源状态哈希验证后标记结束。HurtState 等待结束后，落地回 Idle 并恢复空中资源，空中按 Y 速度返回 JumpUp/Apex/Fall；期间不响应普通攻击入口；TryCancelToDash() 在 LogicalUpdate 中检查有效 Dash 缓冲与 FSM Cancel 许可，准备 Dash 后 TryInterrupt(DashCommand)，再以 Cancel 切到 Dash。CanTransitionTo 对 Cancel 仅放行当前窗口允许的 Dash，自然退出仍等待动画结束。轻受击窗口为 0.32–0.84 秒，重受击为 0.60–1.10 秒（初始调参值，尚未经手感确认），动画事件 OpenMotionCancelWindow/CloseMotionCancelWindow 均携带 ID 0。再次受击仍扣血；依照共用窗口规则，窗口内允许被再次受击打断，但当前 FSM 不支持同一个 HurtState 自身重入，因此正在受击时不会重播受击。
- GameScene Player.hurtAction 已绑定上述资源。效果 condition、击退屏蔽仍未加入；敌人受击执行链路见 EnemyInterruptExecutor。
- 本轮验证：Unity 编译通过、Console Error 为 0；编辑器内使用真实资源验证轻/重阶段选择、无类型拒绝、Motion/Combat 关闭窗口不改变动作、打开窗口后成功清理，以及已准备的 HeavyHurt 阶段在打断后仍由 BeginState 正确提交。检查了场景引用、Animator 两个状态、非循环和片尾事件。已另行验证两个阶段的朝向速度、保留 Y、Dash 窗口开关、命令白名单，以及准备 Dash 后中断/提交的流程；Console Error 为 0。尚未进行 Play Mode 的敌人命中→受击→Dash/恢复表现验证。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/PlayerState.cs`

- `protected bool TryCancelToDash()` 是受击、战斗动作、BackDodge 共用的取消入口，由各状态 LogicalUpdate 显式调用。检查有效 Dash 缓冲与 FSM Cancel 许可，准备目标阶段（沿用 Dash 条件），再依次 TryInterrupt Combat/Motion，最后以 Cancel 切换。父类 CanTransitionTo 对 Cancel 仅放行两个控制器均允许 Dash 的目标；自然切换保持原规则，Hurt 的自然退出仍受动画结束限制。旧 Combat.TryHandleCancel 与 Hurt 私有副本已移除。普通地面/空中移动发起 Dash 的现有入口保持不变。


- 缓存 Player、Movement、持续方向、Motion Controller、Ground/Wall Sensor 与 PlayerMotionAnimationTrigger。
- Enter()：创建 ActionContext，调用 BeginState(stateName, MotionAction, in context) 并保存返回的动画 Hash，登记当前状态，重置普通动画标记，再播放动画；Exit()：EndState(stateName) 清理源动作及窗口。
- protected virtual MotionActionDefinition MotionAction 默认 null；Dash / BackDodge / WallSlide / DoubleJump 返回对应 Data，由 Controller 保存本次选中的 Stage。
- LogicalUpdate() 刷新传感器。CreateActionContext() 从传感器、Player.CanDash / CanDoubleJump、Player.Skills 和 CommandInput.MoveInput 构造条件查询上下文。
- ChangeStateToMoveState() / isSameDirctionForWallandFacingDir() 保留原移动选择与贴墙判断。
- `protected bool TryStartBackDodge()` 由 PlayerGround / Player_RunTransition.LogicalUpdate 调用：Command 匹配后创建 Context，经 MotionController.TryStartAction 检查地面、向下输入与候选 Stage，再切入 player.backDodgeState；调用者在 true 时立即返回。Player.BackDodgeCommand 从 BackDodgeAction.Command 读取，避免重复配置 Command。
- 已移除 State 窗口字段、AddCancelTransition、CanTransitionTo 重写和 Open/CloseCancelWindow；StateCancelTransitionRule 及其 meta 已移除。取消资格由 Controller 检查，FSM 只提交普通生命周期切换。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/PlayerGround.cs`

- 脚本职责：地面状态公共逻辑。
- 函数：
  - `PlayerGround(...)`：调用玩家状态基类构造。
  - `Enter()`：进入地面状态时清除竖直速度，但保留可能仍有效的跳跃缓冲请求；Dash 由共享有效期判断管理；二段跳额度由实际落地事件统一恢复，不依赖具体地面状态继承关系。
  - `LogicalUpdate()`：先检查 BackDodge 请求并在成功后返回，再消费可用冲刺请求并进入 `Player_Dash`；否则在离地后的向下速度达到 `FallEnterVelocityThreshold` 时进入 Fall，再处理跳跃、世界交互和地面移动转换。
- 关联：`Player_IdleState`、`Player_WalkState`、跑步相关状态继承或间接使用地面逻辑。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/PlayerAir.cs`

- 脚本职责：空中状态公共逻辑。
- 函数：
  - `PlayerAir(...)`：调用玩家状态基类构造。
  - 状态进入沿用 EntityState.Enter() 播放动画，已移除只有 base.Enter() 的重复重写；Jump / Dash 请求均由 ActionController 消费与过期校验管理。
  - `LogicalUpdate()`：刷新传感器，识别 Dash Command 并调用 `TryStartAction(player.DashAction, in context)`，成功后切换 Dash；否则通过 `TryHandleLanding()` 处理落地。成功落地后统一恢复二段跳与冲刺额度，未落地且 `WallSensor` 已确认当前帧到达有效边缘时进入 `Player_HangIdle`。
  - `TryHandleLanding()`：默认仅在 `CanEnterGrounded` 成立时切回移动状态并返回是否已经处理落地；`Player_Fall` 重写它以选择普通落地或翻滚落地。
  - 空中 Dash：直接识别 Dash Command 并调用 TryStartAction；删除原 TryHandleDash 包装。JumpUp、Apex、Fall、DoubleJump、WallSlide、WallJump 继承同一入口。
- 关联：`Player_JumpUp`、`Player_Apex`、`Player_Fall`、`Player_WallSlide` 继承它，`Player_WallJump` 通过继承 `Player_JumpUp` 间接使用它。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/Player_HangIdle.cs`

- 脚本职责：让玩家稳定悬挂在已确认的墙边，冻结物理下落，并处理离墙后的坠落转换。
- 函数：
  - `Enter()`：清除速度，把 Rigidbody2D 的 Y 直接设置为 `WallSensor.HangBodyTargetY`，随后暂时把重力缩放设为 0。
  - `LogicalUpdate()`：持续刷新传感器；角色不再面向并贴住墙面时进入下落状态并转身；挂稳后消费有效跳跃请求并进入 `Player_ClimbUp`。
  - `Exit()`：恢复进入挂边前的重力缩放。
- 关联：由 `PlayerAir` 在边缘确认帧进入；使用绝对目标而非相对修正，以兼容 Rigidbody2D Interpolate 下 Transform 与物理位置的时间差。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/Player_ClimbUp.cs`

- 脚本职责：播放一次爬上墙沿动画，冻结攀爬期间的物理运动，并在动画结束时把 Animator 的视觉位移提交给真实 Player。
- 函数：
  - `Enter()`：清空速度、重置动画完成标记、保存并关闭重力。
  - `LogicalUpdate()`：收到动画末尾 `EndAnimation` 事件后提交位移，并根据输入切回地面移动状态。
  - `CommitAnimationPosition()`：先读取 Animator 最后一帧的世界位置，通过 `PlayerMovement.Teleport` 同步 Rigidbody2D 与 Player Transform，再把 Animator 局部位置清零，避免双重位移和插值回闪。
  - `Exit()`：恢复进入攀爬前的重力缩放。
- 关联：由 `Player_HangIdle` 消费跳跃请求后进入；依赖 `Player_ClimbUp.anim` 的 `StartAnimation` / `EndAnimation` Animation Event。

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

- BackDodge 入口与 PlayerGround 共用 TryStartBackDodge()，在基类刷新传感器后、原跳跃/交互处理前检查，因此 Run / RunTurn / RunEnd 也使用同一地面条件。

- 脚本职责：跑步相关过渡状态基类。
- 函数：
  - `Player_RunTransition(...)`：调用玩家状态构造。
  - `Enter()`：清除竖直速度，但保留可能仍有效的跳跃缓冲请求。
  - `LogicalUpdate()`：通过 ActionController.TryConsumeBufferedRequest 消费共享 Jump Command；离地后的向下速度达到 `FallEnterVelocityThreshold` 才进入下落，用短暂速度窗口过滤平台边缘检测抖动，并处理跑步公共转换。
  - `IsSameDirection()`：判断输入方向和当前朝向是否一致。
- 关联：`Player_RunState`、`Player_RunTurnState`、`Player_RunEndState` 继承它。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/Player_Run/Player_RunState.cs`

- Run → Dash：base.LogicalUpdate 后确认 FSM 仍为 this，识别 Dash Command、创建 Context，直接调用 TryStartAction；成功后 ChangeState(dashState) 并 return，不在状态里重复判断 CanDash/有效期或消费输入/次数。

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
  - `LogicalUpdate()`：执行空中公共逻辑；有效跳跃请求与二段跳额度都满足时进入 `Player_DoubleJump`，否则当竖直速度低于 Apex 阈值时进入 `Player_Apex`。
- 关联：从 `Player_JumpStart` 进入，之后连接 Apex 或 Fall。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/Player_Apex.cs`

- 脚本职责：跳跃最高点状态。
- 函数：
  - `Player_Apex(...)`：调用空中状态构造。
  - `LogicalUpdate()`：执行空中公共逻辑；有效跳跃请求与二段跳额度都满足时进入 `Player_DoubleJump`，否则当角色开始下落时进入 `Player_Fall`。
- 关联：让跳跃最高点有独立动画状态。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/Player_Fall.cs`

- 脚本职责：下落状态。
- 函数：
  - `Player_Fall(...)`：调用空中状态构造。
  - `Enter()`：切换为配置中的下落重力，使下降手感可与上升阶段独立调整。
  - `LogicalUpdate()`：记录本轮下落速度并执行空中公共逻辑；只有 `CanEnterGrounded` 成立时才落地，下落速度达到阈值时进入 RollingLand；Coyote Time 普通跳跃优先于二段跳，只有有效跳跃请求与二段跳额度都满足时才进入 `Player_DoubleJump`，否则可进入墙滑。
  - `PhysicalUpdate()`：处理空中水平移动。
  - `Exit()`：恢复默认重力缩放。
- 关联：从离地、跳跃上升结束或 Apex 进入；落地后通过 `PlayerAir.LogicalUpdate()` 回到地面移动状态。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/Player_Jump/Player_DoubleJump.cs`

- 脚本职责：执行本轮滞空唯一一次二段跳，并根据进入状态时的水平输入选择向前或垂直动画。
- 函数：
  - `Enter()`：调用 MarkDoubleJumpUsed；MotionAction 固定读取统一 Player.DoubleJumpAction，由 PlayerState.Enter 采用 Motion Controller 启动时已选中的 Stage 动画，通过共用 ApplyEntryVelocity 施加所选 Stage 的初速度。
  - `LogicalUpdate()`：执行空中公共逻辑，二段跳动画结束后进入 `Player_Fall`。
- 关联：JumpUp、Apex、Fall 消费有效输入和共享二段跳额度后进入；额度在 `PlayerAir` 确认成功落地后恢复，不依赖落地目标状态的继承关系。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/Player_RollingLand.cs`

- 脚本职责：播放高下落速度对应的翻滚落地表现，并在动画期间维持翻滚方向的水平速度。
- 函数：
  - `Enter()`：清除竖直速度、重置动画结束标记，并根据当前朝向与配置计算翻滚速度。
  - `LogicalUpdate()`：动画结束且仍接地时回到待机、步行或跑步状态。
  - `PhysicalUpdate()`：持续写入翻滚水平速度。
- 关联：`Player_Fall.TryHandleLanding()` 根据下落速度阈值选择进入。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/Player_BackDodge.cs`

- 脚本职责：执行地面起始的后撤小跳。继承 PlayerState，不继承 PlayerAir，避免空中移动覆盖后撤速度或落地时提前跳过动作结束判断。Player.Awake 创建 backDodgeState，GameScene Player.backDodgeAction 指向 Action_BackDodge.asset。
- `public override void Enter()`：保存原重力，经基类登记 Action/Stage、播放动画并施加 Stage.EntryVelocityRule；朝右 (-15,5)、朝左 (15,5)，不翻转角色。随后应用 PlayerBaseConfig.BackDodgeGravityScale，退出时恢复。
- `public override void Exit()`：恢复进入前保存的原重力，再执行基类清理；自然结束与 Dash 打断均走同一恢复路径。
- 可配置重力 Play Mode 验证：用临时克隆将 BackDodgeGravityScale 设为 7，入场正确使用 7，动画结束且接地后回 Idle 并恢复 4；Dash 打断时重力按 Dash 规则变为 0，Dash.Exit 后恢复原倍率 4，未继承后撤倍率 7。原配置资源未改为测试值，克隆已销毁，Console 无 Error/Warning。
- `public override void LogicalUpdate()`：刷新传感器后，优先检查 Dash 请求；由 Motion Controller 依取消窗口与 Dash 自身条件决定能否打断。正常结束要求 IsAnimationFinished、Rigidbody2D.IsTouchingLayers(GroundSensor.GroundLayers) 与 CanEnterGrounded 同时成立，再 RestoreAirborneResources 并调用已有 ChangeStateToMoveState 返回 Idle / Walk / Run。先落地则等待动画结束，动画先结束则等待落地；动画时长不参与速度或重力计算。
- 正常结束已改为动画结束与实际落地共同决定，不再仅凭接地提前返回，也不再要求 hasLeftGround 标记；未真正离地的配置同样可以在动画结束且接地后退出。此前自动重力版本的固定落地时刻不适用于当前配置驱动版本。
- Play Mode 使用临时克隆配置验证两种完成顺序：Y=0 且接地时等待动画结束再返回 Idle；Y=13.85 的垂直跳中先触发动画结束，仍等待实际落地后才返回 Idle。两组均通过，Console 无 Error/Warning；原资源配置未修改，克隆配置已销毁并退出 Play。
- 地面资格复用现有 CombatActionGroundedCondition（检查 CanEnterGrounded），与负 Y 输入一起放在 Definition.entryConditions，不新建重复地面条件。空中状态没有后撤入口；不足两个探针支持时入口也会拒绝。
- 两个 Stage 的取消窗口 0 只接受 Dash，Clip 起点打开、终点关闭；State 退出沿用 PlayerState.Exit 清理窗口和当前 Stage，迟到动画事件被来源校验忽略。Perfect 目前仅配置动画和窗口，未加入特殊选择规则。
- 验证范围：原有地面/负 Y 输入入口、非循环 Clip、Dash 取消窗口与实际地面接触检查保留。自动重力、动画固定更新和按动画时长同步的计算已撤销；不同配置的滞空时间应随真实物理变化，不保证与动画结束同帧。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/Player_Dash.cs`

- 转向冲刺回归：朝左/右 × 入场水平速度 +13/-13 四组均得到绝对速度 20.2，0.22 秒位移约 4.444；转向残留的反向速度不再把冲刺速率抵消为 9.8。

- 脚本职责：执行地面或空中的水平冲刺，并在冲刺期间临时关闭重力。
- 关键字段：
  - `originalGravity`：进入冲刺前的重力缩放，用于退出时恢复。
- 函数：
  - `Enter()`：调用 MarkDashUsed 记录本次使用，重置动画完成标记并关闭重力；进入前判断是否面向墙壁，需要时先翻转；基类从 Stage.EntryVelocityRule 计算并施加速度：基础 15 + Abs(当前水平速度) × 0.4，贴墙使用备用局部速度 15，Y=0。
  - `LogicalUpdate()`：空中冲刺期间允许消费二段跳；动画结束后按落地、贴墙或仍在空中的结果分别进入地面移动、墙滑或下落状态；冲刺中落地会恢复二段跳和冲刺额度。
  - `Exit()`：恢复进入冲刺前的重力缩放。
- 关联：地面、跑步和空中公共状态消费冲刺输入与共享额度后进入；`PlayerAir` 成功落地时恢复额度，`Player_WallSlide.Enter()` 当前也会补充一次冲刺机会。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/Player_WallSlide.cs`

- 脚本职责：控制玩家贴墙滑落，并接收离墙与墙跳输入。
- 关键字段：
  - `originalGravity`：进入贴墙状态前的重力缩放，用于退出时恢复。
- 函数：
  - `Enter()`：保留统一缓冲请求，恢复空中资源，保存并临时关闭刚体重力，把速度设为配置中的普通滑落速度。
  - `LogicalUpdate()`：向下输入时使用快速滑落速度；不再朝墙输入时返回 `Player_Fall`；先由 MotionController.AllowsCancel(JumpCommand) 检查窗口，再通过 TryConsumeBufferedRequest 消费共享 Jump Command 并进入 `Player_WallJump`；落地转换由 `PlayerAir` 处理。
  - `Exit()`：恢复进入状态前的重力缩放。
- 关联：`Player_Fall` 检测到玩家持续朝墙输入后进入；墙跳时连接 `Player_WallJump`。

#### `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/Player_WallJump.cs`

- 脚本职责：墙滑期间触发远离墙面的定向跳跃，并在离墙初段锁定普通空中水平控制。
- 函数：
  - `Enter()`：复用 `JumpUp` 动画进入墙跳表现，再调用 `PlayerMovement.HandleWallJump()` 翻转朝向并施加墙跳速度。
  - `PhysicalUpdate()`：保持为空，避免 `Player_JumpUp` 的普通水平移动立刻覆盖墙跳水平速度。
- 关联：由 `Player_WallSlide` 消费新的跳跃请求后进入；复用 `Player_JumpUp.LogicalUpdate()`，到达 Apex 阈值后继续进入 `Player_Apex`。

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
5. Move 写入 Player.CommandInput；Interact、Jump、Dash 提交给 MotionController；Attack、Launcher、Dive 提交给 CombatController，各自使用父类的请求入口。
6. 普通玩家状态按当前场景消费 MotionController 的 Interact / Jump / Dash 请求；战斗 Controller 把 Action Request 匹配为 `CombatActionDefinition`，由 `Player` 协调 FSM 进入通用 Combat State。

### 玩家状态机流程

1. `Player` 继承 `Entity`，在 `Awake()` 中创建状态实例。
2. `StateMachine.InitializeState()` 进入默认状态。
3. `Player.Update()` 调用 `StateMachine.LogicalUpdate()`，随后通过 `TryEnterCombatAction()` 协调 Action System 与 FSM 的控制权交接。
4. `Player.FixedUpdate()` 调用 `StateMachine.PhysicalUpdate()`，处理刚体移动。
5. `EntityState.Enter()` 和 `Exit()` 用 `PlayerAnimationHash` 控制 Animator bool。
6. `PlayerMovement` 负责速度和翻转，`GroundSensor` 负责落地检测，`PlayerMotionAnimationTrigger` / `PlayerCombatAnimationTrigger` 分别负责对应动画事件回传；Combat Stage 结束信号经 Controller 处理后由通用 Combat State 把控制权交回移动状态。

### 世界交互到对话流程

1. 玩家按交互键，`InputRouter.OnInteractPressed()` 向 MotionController 提交 Command_Interact。
2. `PlayerGround` / `Player_RunTransition.LogicalUpdate()` 通过 TryConsumeBufferedRequest(player.InteractCommand) 消费世界交互请求。
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
2. `InputRouter` 将 Interact Command 提交到 MotionController 的单槽 ActionRequest。
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

## 10. 玩家跑步、跳跃与冲刺状态设计

当前已经移除旧的 `Player_JumpState.cs`，跳跃拆分为 `JumpStart`、`JumpUp`、`Apex`、`Fall` 与 `DoubleJump`。二段跳额度由 `Player` 跨状态持有，在成功落地和进入墙滑时恢复。跑步逻辑集中在 `Player_Run` 子目录，步行状态由 `Player_WalkState` 和 `Walk` 动画承接。

冲刺由 `Player_Dash` 统一承接地面和空中入口。冲刺机会由 `Player` 跨状态持有，成功冲刺时消费，落地时恢复；进入墙滑当前也会恢复一次，因此墙面可作为冲刺补充点。冲刺期间重力暂时关闭，退出状态时恢复原重力。

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
- `DoubleForwardJump`、`DoubleVerticalJump`：根据二段跳触发时是否存在水平输入选择。
- `RollingLand`：下落速度达到配置阈值时进入，动画期间维持翻滚水平速度。
- `BaseLand`：动画 hash 已准备，普通落地状态尚未接入。
- `Dash`：播放水平冲刺表现，结束后按接地和贴墙结果回到地面移动、墙滑或下落。

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
| `FallEnterVelocityThreshold` | Ground/Run 离开平台后进入 Fall 的最小向下速度绝对值 | `3` - `6` |

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
- 新增玩家 `Dash` 动画状态与 `Player_Dash` 状态逻辑，支持地面/空中冲刺、空中次数限制、贴墙反向冲刺、冲刺中二段跳以及落地/墙滑后的额度恢复。
- 玩家跳跃状态拆分为 `Player_JumpStart`、`Player_JumpUp`、`Player_Apex`、`Player_Fall`。
- 玩家下落状态已接入完整的墙滑/墙跳纵向切片：持续朝墙输入时按配置速度滑落，向下输入可加速，松开墙面方向会恢复下落，墙滑中再次按跳跃会翻转并施加 `WallJumpForce`；墙滑使用独立动画，墙跳复用 `JumpUp` 动画。
- 跑步过渡状态移动到 `Assets/_Game/Scripts/Runtime/GamePlay/Player/PlayerState/Player_Run/`。
- `GameScene.unity` 已加入 `StorySceneBindings`、`StoryCameraDirector`、`StoryCanvas`、剧情文本视图、剧情相机绑定和 Input System UI EventSystem。
- `GameScene.unity` 的 HUD 中 HP、MP 分别使用 `UI_HPBarView`、`UI_MPBarView`，继承 `UI_ResourceBarView` 的共有配置，各自绑定 `MaskArea/Fill` 并配置满值宽度。
- `GameScene.unity` 已加入 `Canvas_Inventory` 背包界面雏形，当前包含滚动视图和多个 Slot 图像节点。
- UI 状态条图集 `04.png`、`06.png`、`07.png` 现归档于 `Assets/_Game/Art/UI/HUD/`，保留对应 `.meta`。
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
- 背包用图 `Slot.png`（32 个格子底图共用）与 `Inventory Slots and Selection Frame Set.png`（选中框来源）现归档于 `Assets/_Game/Art/UI/Inventory/`；三张 `ChatGPT Image Aug 30, 2026, *.png` 面板底板归入 `UI/Panels/`；另一张 `10_46_38 PM.png` 实际为三角按钮，归入 `UI/Buttons/`。所有移动保留原 `.meta` 和 Sprite 切片。历史记录中的 `Icy Fantasy RPG UI Asset Sheet.png` 当前不在 Art 目录中。

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
- 玩家墙滑已支持离墙退出、快慢滑落与墙跳；下落重力、墙跳力度和双点落地检测均已配置化或接入场景。
- 剧情序列系统已具备条件触发、顺序执行、等待、等待条件、日志、剧情文本显示和玩家移动模式切换步骤。

仍需留意：

- `FlagBoolCondition` 和 `SetFlagDialogueEvent` 中的 `flagData` 需要在资源里正确赋值，避免空引用。
- `QuestManager` 只能设置已注册在 `QuestDatabase` 中的 Quest。
- `InteractionContext.IneractorTransform` 当前属性名存在拼写问题；若改名会影响引用处，需要统一重构。
- `Player_RunState` 中 `isFirstTimeRelese` 存在拼写问题；若改名需要同步所有引用。
- `Player_Dash` 依赖 Dash 动画末尾调用 `PlayerMotionAnimationTrigger.EndAnimation(AnimationEvent)` 才能正常退出；Player_Dash.anim 已配置 Motion 窗口及结束事件；若冲刺无法结束，应先检查该动画的末帧事件及 Motion Controller 当前登记的来源 Hash。
- `Movement.SetRigibodyVelocity` 方法名存在拼写问题；若改名需要同步所有调用。
- `QuestData` 中 `descrition` 字段存在拼写问题；因为是序列化字段，改名前应考虑 `[FormerlySerializedAs]`。
- `StoryCameraDirector` 中 `lowerPreviousCameera` 字段存在拼写问题；因为是序列化字段，改名前应考虑 `[FormerlySerializedAs]`。
- `StopPlayerStoryStepAction` 中 `playerkey` 字段命名大小写不一致；因为是序列化字段，改名前应考虑 `[FormerlySerializedAs]`。
- `HideStoryTextStepAction` 和 `ShowStoryTextStepAction` 中缺失 `StorySceneBindings` 时的警告文本仍写成了设置玩家移动模式，后续可统一改文案。
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
