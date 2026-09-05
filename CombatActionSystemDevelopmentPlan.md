# Combat Action System 开发计划书

> 文档状态：设计阶段  
> 当前版本：v0.1  
> 当前目标：在保留现有 Player FSM 的前提下，建立一套可逐步扩展、不过度设计的战斗 Action 执行框架。  
> 当前范围：只制定架构边界与分阶段开发计划，不在本阶段实现攻击伤害、完整连招或所有动作内容。

## 1. 背景与动机

当前 Player FSM 已经承担角色移动与基础状态，包括 Idle、Walk、Run、Jump、Fall、WallSlide、Dash、ClimbUp 等。它直接负责：

- 更新 GroundSensor 与 WallSensor；
- 决定移动状态切换；
- 驱动 Rigidbody2D；
- 通过 `Animator.CrossFade` 播放当前状态动画；
- 消费跳跃、冲刺与世界交互等输入请求。

后续战斗行为将包含普通攻击、重攻击、空中攻击、闪避派生、Parry、Counter、多阶段下落攻击，以及尚未确定的其他武器和状态派生。如果继续为每个攻击创建 Player FSM State，状态数量和相互转换会快速增长；如果只使用 `AttackState + comboIndex`，又无法清晰表达：

- 动画中任意帧开放或关闭派生窗口；
- 没有派生时继续播放剩余收尾帧；
- 命中与挥空拥有不同派生结果；
- 同一个输入在不同上下文中选择不同 Action；
- 一个 Action 内部包含多个自动推进的动画阶段；
- 动作对移动、目标、资源、状态与临时机会的条件要求；
- Normal、PowerUp 或武器版本共享动作关系但更换表现。

因此需要在现有 FSM 之上加入独立的 Combat Action System，但必须明确两个系统的职责和控制权，避免它们同时修改 Animator 或 Rigidbody2D。

## 2. 设计目标

### 2.1 必须达到

1. Combat Action System 核心不知道项目中有哪些具体攻击。
2. 新增招式时，优先通过数据、Condition、Effect 和 Transition 组合完成，而不是修改 Controller 主流程。
3. 支持一个输入对应多个候选 Action，并根据实时上下文选择。
4. 支持输入缓冲。
5. 支持动画任意帧开启、关闭派生窗口。
6. 支持当前动画没有成功派生时继续播放收尾。
7. 支持一个 Action 包含一个或多个 Stage。
8. 支持命中结果、临时机会、目标状态等条件参与派生。
9. 支持动作开始、动画信号、命中、阶段切换、动作结束等时机执行 Effect。
10. 保留现有 Player FSM，让它继续负责移动与基础状态。
11. 保证任意时刻只有一个系统拥有 Animator 和主要 Rigidbody2D 写入权。
12. 每个阶段都能在 Play Mode 中得到明确、可观察的验证结果。

### 2.2 当前不追求

- 不实现类似 Unreal GAS 的完整 Ability、Attribute、Cooldown、Cost、Tag 框架。
- 不创建可视化 Graph Editor。
- 不预先制作所有 Condition、Effect、Signal 和 Action 类型。
- 不一次性实现全部普通攻击、空中连招、Parry、下落攻击和 PowerUp。
- 不在第一阶段重构现有移动 FSM。
- 不在第一阶段接入完整装备、武器切换或伤害系统。
- 不使用字符串 Dictionary 作为万能 Blackboard。

## 3. 核心设计原则

### 3.1 系统只认识能力，不认识招式名称

核心运行时代码只处理以下概念：

- Request：玩家或其他系统提出了什么动作意图；
- Context：当前世界与角色事实；
- Definition：某个 Action 的静态数据；
- Runtime：当前 Action 的运行时进度和结果；
- Stage：一个 Action 内部的执行阶段；
- Signal：动画、物理或战斗系统传来的时机通知；
- Window：一段临时开放的输入或派生机会；
- Condition：当前是否允许执行；
- Effect：在指定时机产生什么结果；
- Transition：满足哪些条件时可以进入哪个目标 Action；
- Opportunity：由动作或外部状态临时授予的派生资格。

核心代码中不得出现针对具体动作名称的分支，例如：

```text
if currentAction is LauncherAttack ...
if currentAction is DiveAttack ...
```

具体招式名称只能出现在 ScriptableObject 内容资源、Animator State 和调试信息中。

### 3.2 静态数据与运行时状态分离

`CombatActionDefinition` 是只读的 ScriptableObject 配置，不保存本次攻击是否命中、当前阶段、窗口是否打开等运行时数据。

`CombatActionRuntime` 在每次 Action 开始时创建或重置，持有：

- 当前 Definition；
- 当前 Stage；
- 已开放窗口；
- 本次 Action 的命中或其他 Outcome；
- 已命中过的目标记录；
- 开始时间；
- 是否完成、取消或被替换。

这可以避免多个角色共用同一 ScriptableObject 时互相污染状态。

### 3.3 Condition 只判断，Effect 只执行

Condition 不改变世界状态；Effect 不负责决定自己是否应该执行。

例如，“目标可以被浮空”是 Condition，“给目标和角色施加浮空速度”是 Effect。两者不能藏在同一个巨大 Action Controller 分支里。

### 3.4 Action 与 Stage 不等价

多数 Action 可以只有一个 Stage，但系统必须允许一个 Action 包含多个自动推进阶段。

Action 之间的转换属于招式派生；Action 内 Stage 的推进属于同一招式的生命周期。即使 Runtime 内部记录当前 Stage 位置，也不能用它代替 Action Transition 或连招关系。

### 3.5 派生窗口与动画结束分离

一次攻击可能在动画中段允许派生，后续帧属于没有派生时播放的收尾。因此至少需要区分：

- Hit Window Open / Close；
- Transition Window Open / Close；
- Stage Finished；
- Action Finished。

只有成功选择目标 Action 时才跳过当前收尾；没有合法请求时继续当前动画，直到真正结束。

## 4. 分层架构

```text
Combat Action Core
├── CombatActionRequest
├── CombatActionDefinition
├── CombatActionStage
├── CombatActionTransition
├── CombatActionRuntime
├── CombatActionContext
├── CombatActionCondition
├── CombatActionEffect
├── CombatActionSet
└── PlayerCombatActionController
            │
            ▼
Player Adapter
├── Player_CombatActionState
├── Player Context 构建
├── PlayerMovement / Rigidbody2D 适配
├── GroundSensor / WallSensor 读取
└── PlayerAnimationTrigger 信号转发
            │
            ▼
Combat Content
├── Action Definition 资源
├── Condition 资源
├── Effect 资源
├── Command Key 资源
└── 后续武器或形态 Action Set
```

依赖方向只能从内容层指向核心抽象，核心层不得反向引用任何具体招式内容。

## 5. 核心概念与职责

### 5.1 CombatActionCommandKey

输入意图使用 ScriptableObject Key 或等价的稳定资源身份表示，而不是在核心 Controller 中维护 `LightAttack`、`HeavyAttack`、`Parry` 等硬编码分支。

输入层负责把 Input Action 映射到对应 Command Key；Action System 只比较资源身份。

第一阶段只需要一个攻击 Command Key，但结构不能要求每新增一种输入都修改 Action Controller。

### 5.2 CombatActionRequest

推荐使用 `readonly struct`，至少保存：

- Command Key；
- 请求时间；
- 可选输入值；
- 可选来源。

Request 表达意图，不直接指定最终 Action。同一个 Request 可以根据 Context 解析为不同动作。

### 5.3 CombatActionContext

Context 是一次候选选择时的只读快照。第一阶段只加入真正需要的数据，例如：

- Player 引用；
- 当前移动状态的通用事实；
- GroundSensor/WallSensor 结果；
- Rigidbody2D 速度；
- 移动输入和面向方向；
- 当前 Action Runtime；
- 当前或候选目标；
- 当前可查询的 Opportunity。

不在第一阶段做任意键值 Blackboard。出现第二类真实扩展需求后，再评估是否引入 Context Provider 接口。

### 5.4 CombatActionDefinition

Action 的静态 ScriptableObject 数据，建议包含：

- 显示名称；
- 一个或多个 Stage；
- 入口 Conditions；
- 从当前 Action 出发的 Transitions；
- 默认动作结束策略；
- 调试颜色或说明等仅编辑器数据。

Transition 直接引用目标 Action Definition，避免使用 `comboIndex + 1`。

### 5.5 CombatActionStage

初期作为 `CombatActionDefinition` 内部的可序列化数据，不单独创建大量资产。建议逐步支持：

- Animator State 名称及运行时 Hash；
- Stage 进入时的移动策略；
- Stage 结束条件；
- Stage 生命周期 Effect；
- 可由 Animation Event 打开的 Hit/Transition Window；
- 自动进入的下一 Stage。

Stage 的结束条件可能来自：

- Animation Event；
- 接地；
- 离地；
- 命中结果；
- 外部显式 Signal。

第一阶段只实现 Animation Event 结束。

### 5.6 CombatActionTransition

一条 Transition 至少描述：

- 所需 Command Key；
- 所需 Window ID；
- Conditions；
- 目标 Action Definition；
- 成功转换时是否消费 Request；
- 可选的转换 Effects。

Transition 的判断顺序应保持稳定，并在发现多条规则同时成立时提供可读警告。

### 5.7 CombatActionCondition

无状态 ScriptableObject，只提供条件检查。预定抽象签名：

```csharp
public abstract bool IsMet(in CombatActionContext context);
```

只有当前 Checkpoint 真正需要某种条件时，才创建对应实现。首批可能只有接地条件，不预建目标、资源、武器、Buff 等未来条件。

### 5.8 CombatActionEffect

无状态 ScriptableObject，在明确的生命周期时机执行。预定抽象签名：

```csharp
public abstract void Apply(in CombatActionExecutionContext context);
```

Effect 的具体种类按行为增加，不提前实现完整库。

### 5.9 CombatActionOpportunity

Opportunity 用于表达由 FSM、命中结果、目标状态或其他 Action 临时授予的资格。它应包含稳定 Key、来源、可选目标、失效条件和消费规则。

该能力只保留扩展位置，第一阶段不实现通用 Opportunity Store。等闪避攻击、Parry Counter 或其他真实行为需要时，再用最小版本接入。

### 5.10 CombatActionSet

Action Set 提供当前角色或武器可用的入口规则，并定义同一 Request 在没有当前 Action 派生时如何选择候选。

候选规则按明确的编辑器顺序判断，第一个满足全部 Condition 的入口获胜。兜底动作放在列表末尾，不依赖散落在代码中的魔法优先级数值。

第一阶段可以只有一个默认 Action Set。装备系统接通后再决定由武器、角色形态或两者组合提供 Active Action Set。

## 6. 候选选择算法

收到有效 Request 后，按以下顺序处理：

1. 清理超过缓冲时间的 Request。
2. 如果当前有 Action：
   - 读取当前 Stage 已开放的 Window；
   - 按当前 Action 的 Transition 顺序查找匹配 Request；
   - 检查 Transition Conditions；
   - 第一条成立的 Transition 成为目标 Action。
3. 如果当前没有 Action，或当前动作允许退出到新入口：
   - 从 Active Action Set 中取得与 Request 对应的入口规则；
   - 按资源顺序检查入口 Conditions；
   - 第一条成立的规则成为目标 Action。
4. 只有成功准备目标 Action 后才消费 Request。
5. 没有候选成立时，Request 保留到缓冲过期，而不是立即丢弃。
6. 如果当前动作没有成功派生，它继续播放现有 Stage 和收尾帧。

核心算法不按照动作名称分支。

## 7. 与 Player FSM 的协作方式

### 7.1 通用桥接状态

Player FSM 只新增一个 `Player_CombatActionState`。所有战斗 Action 共用它，不为每招创建 Player State。

它负责：

- 进入时启动 Controller 已准备好的 Action；
- LogicalUpdate 中继续刷新必要传感器并推进 Action Runtime；
- PhysicalUpdate 中按当前 Stage 的移动策略更新 Rigidbody2D；
- 动作完成后根据实时环境恢复合适的移动状态；
- 退出时清理 Action 临时控制权。

### 7.2 控制权约束

| 运行情况 | Animator 主要控制者 | Rigidbody2D 主要控制者 | 状态传感器 |
| --- | --- | --- | --- |
| 普通移动 | Player FSM 当前状态 | Player FSM 当前状态 | PlayerState 更新 |
| Combat Action 执行中 | Combat Action Runtime | Player_CombatActionState / Action Effect | 继续更新 |
| Action 完成 | 交还 Player FSM | 交还 Player FSM | 用实时结果选择恢复状态 |

禁止 Action Controller 在普通移动状态后台持续写速度或播放动画。

### 7.3 预定运行顺序

`Player.Update()` 的目标顺序：

1. 当前 FSM State 执行 LogicalUpdate，让传感器和基础状态更新到本帧结果。
2. Action Controller 检查缓冲 Request 并构建 Context。
3. 若成功准备 Action，则切入 `Player_CombatActionState`。
4. Action 执行期间，由桥接状态调用 Controller 的 Logical Tick。

`Player.FixedUpdate()` 在 Action 状态中只执行 Action 的 Physical Tick，避免普通移动覆盖动作速度。

## 8. 动画信号设计

现有 `PlayerAnimationTrigger` 继续服务移动动画，同时增加战斗专用信号转发，避免战斗逻辑依赖共享的全局完成 bool。

初步预定的 Animation Event 入口：

```csharp
public void OpenCombatTransitionWindow(int windowId);
public void CloseCombatTransitionWindow(int windowId);
public void OpenCombatHitWindow(int windowId);
public void CloseCombatHitWindow(int windowId);
public void FinishCombatStage();
```

这些函数只转发 Signal，不直接决定下一个 Action，也不直接计算伤害。

必须保证：

- 旧动画的信号不会结束新 Action；
- 没有 Active Action 时收到战斗信号只产生一次明确警告；
- Action 或 Stage 切换时关闭旧 Window；
- 动画事件名称、Window ID 与 Definition 配置可以被验证。

## 9. 表现变体与 PowerUp

`Normal`、`PowerUp` 等应优先视为同一语义 Action 的表现变体，不复制整张派生关系。

目标关系：

```text
Semantic Action
├── Shared Conditions
├── Shared Transitions
├── Normal Presentation
└── PowerUp Presentation
```

PowerUp 内容实际制作前不实现完整 Variant 系统，只确保：

- Transition 引用语义 Action 资源，而不是某个 Normal 动画；
- Animator State 名只属于 Stage 表现数据；
- Runtime 预留从角色形态选择 Presentation 的入口。

如果 PowerUp 只替换相同时间轴上的 Sprite/VFX，可评估 AnimatorOverrideController；如果动画帧、窗口或动作规则不同，再使用独立 Stage Variant。

## 10. 建议目录

第一阶段计划使用：

```text
Assets/_Game/Scripts/Runtime/GamePlay/Combat/Actions/
├── CombatActionCommandKey.cs
├── CombatActionRequest.cs
├── CombatActionContext.cs
├── CombatActionDefinition.cs
├── CombatActionStage.cs
├── CombatActionRuntime.cs
├── CombatActionTransition.cs
├── CombatActionSet.cs
├── CombatActionCondition.cs
└── CombatActionEffect.cs

Assets/_Game/Scripts/Runtime/GamePlay/Player/Combat/
├── PlayerCombatActionController.cs
└── Player_CombatActionState.cs

Assets/_Game/Data/Combat/
├── Commands/
├── Conditions/
├── Effects/
├── Actions/
└── ActionSets/
```

这是一张目标目录图，不代表第一阶段会一次创建其中所有脚本和文件夹。只有进入对应 Checkpoint 时才落地所需类型。

第一版仍位于 `Assembly-CSharp`。当纯逻辑选择器和输入缓冲稳定、确实需要 EditMode 独立测试时，再讨论是否拆出 `FirstGame.Combat` Assembly Definition；不为了测试结构提前改变整个项目程序集边界。

## 11. 关键函数边界草案

以下签名用于明确调用关系，正式实现时可在不改变职责的前提下微调命名。

### InputRouter → PlayerCombatActionController

```csharp
public void RequestAction(CombatActionCommandKey command);
```

InputRouter 在 Game Layer 和 PlayerControlArbitration 检查通过后调用。它只写入缓冲，不立即播放动画。

### Player → PlayerCombatActionController

```csharp
public bool TryPrepareAction(
    in CombatActionContext context,
    out CombatActionDefinition action);
```

Player 在本帧移动 FSM 更新之后调用。Controller 检查当前派生和入口候选，成功时返回准备执行的 Action。

### Player_CombatActionState → PlayerCombatActionController

```csharp
public void BeginAction(
    CombatActionDefinition action,
    in CombatActionContext context);

public void LogicalTick(in CombatActionContext context);

public void PhysicalTick(in CombatActionContext context);

public void ExitAction(CombatActionExitReason reason);
```

桥接状态在 Enter、Update、FixedUpdate 与 Exit 对应时机调用。

### PlayerAnimationTrigger → PlayerCombatActionController

```csharp
public void NotifySignal(in CombatActionSignal signal);
```

动画组件把具体 Animation Event 转成统一 Signal 后调用。Controller 根据当前 Runtime、Stage 和 Signal 决定开关窗口、执行 Effect 或推进阶段。

### CombatActionCondition

```csharp
public abstract bool IsMet(in CombatActionContext context);
```

调用者是候选选择器；返回值只被用来判断入口或 Transition 是否成立。

### CombatActionEffect

```csharp
public abstract void Apply(in CombatActionExecutionContext context);
```

调用者是 Action Runtime；实参来自当前 Player、Stage、目标、Signal 和 Outcome。

## 12. 分阶段开发计划

### Checkpoint 0：冻结基线与确认素材

当前目标：在开始写代码前保护现有未提交内容，并确认第一个测试动画的 Animator State 名称和 Animation Event 状态。

本次只做：

- 查看完整 `git status --short` 和相关 diff；
- 不修改无关动画、场景、贴图导入设置或 Player_DoubleJump；
- 确认第一段普通攻击的 Animator State；
- 确认测试用输入绑定；
- 记录首个 Checkpoint 所需的动画事件位置。

正确结果：能够明确指出第一个 Action 使用哪条动画、哪个输入以及最后一帧在哪里结束。

### Checkpoint 1：单个 Action 完整执行闭环

游戏行为：玩家在允许的移动状态中按攻击，播放一段完整攻击动画；动作期间普通移动不会覆盖动画或速度；动作结束后恢复正确移动状态。

本次只实现：

- 一个 Command Key；
- 一个时间戳输入缓冲；
- 一个单 Stage Action Definition；
- 一个 PlayerCombatActionController；
- 一个 Player_CombatActionState；
- 一个 FinishCombatStage 动画事件；
- 一个默认 Action Set 或最小等价配置；
- 必要的临时 Debug 日志。

暂不实现：

- Hitbox；
- 伤害；
- 派生；
- 多 Stage；
- Opportunity；
- PowerUp。

Play Mode 验证：

1. 在 Idle 中按鼠标左键或 Enter，进入第一段攻击。
2. 在 Run 中按攻击，攻击取得控制权，普通移动不覆盖动作。
3. 攻击期间继续按方向，不打断当前动画。
4. 最后一帧 Signal 到达后，根据当前输入回到 Idle、Walk 或 Run。
5. Console 能看到 Request Buffered、Action Started、Action Finished 和恢复状态。

Checkpoint 复盘重点：输入从哪里产生、何时被消费、FSM 如何交出和收回执行权、为什么不会出现两个系统同时写 Animator/Rigidbody2D。

### Checkpoint 2：派生窗口与收尾帧

游戏行为：第一段攻击在指定动画帧开放派生窗口；提前输入可以被缓冲；窗口内的合法输入先记录为 Pending Transition，并在动画指定的 Commit 帧切入第二个 Action；没有输入时继续播放第一段收尾并正常结束。

本次只新增：

- 一个 Transition Window；
- Open/Close Window Animation Event；
- 一条 ActionTransition；
- 第二个单 Stage Action；
- 当前 Action 优先检查 Transition 的选择逻辑。

Play Mode 验证：

- 窗口前稍早按键，可以在窗口打开时派生；
- 窗口内按键，只记录派生并在固定 Commit 帧切换；
- 窗口关闭后按键，不会错误跳过收尾；
- 不输入时第一段完整播放；
- 派生后旧动画事件不会结束新 Action。

Checkpoint 复盘重点：派生窗口和动画完成为什么是两个概念，以及 Request 为什么只能在成功准备目标 Action 后消费。

### Checkpoint 3：单次命中纵向切片

游戏行为：一段攻击只在指定 Hit Window 内命中测试目标，并对同一目标按规则结算一次。

本次只新增：

- Hit Window Signal；
- 最小 Hitbox；
- 最小 Damage Receiver 或测试 Dummy；
- Action Runtime 命中记录；
- HitConfirmed Outcome；
- Gizmo 与有意义的命中日志。

必须先决定：同一 Stage、同一 Hit Window 和整个 Action 对重复命中的作用域。

### Checkpoint 4：Condition、Outcome 与 Effect

游戏行为：至少一条派生或动作效果由命中结果和目标能力决定，而不是由动作名称硬编码。

本次才正式落地：

- 第一批真实 CombatActionCondition；
- 第一批真实 CombatActionEffect；
- 目标能力检查；
- Outcome 驱动的 Transition 或 Effect。

验证重点：替换 Action 资源后，Controller 不需要增加具体招式分支。

### Checkpoint 5：多 Stage Action

游戏行为：一个 Action 能通过 Animation Signal 和环境 Signal 在多个 Stage 之间自动推进，每个 Stage 拥有独立动画、移动策略和 Hit Window。

验证重点：Stage 推进不需要再次输入，也不会被误当成 Combo 派生；动作完成前 FSM 不重新取得控制权。

### Checkpoint 6：临时 Opportunity 与跨系统派生

游戏行为：FSM、Parry 结果、命中结果或其他系统可以授予一个有生命周期的 Action Opportunity；对应 Condition 能读取并按规则消费它。

本次才决定 Opportunity 的最小数据结构，避免提前制造通用 Tag/Blackboard 系统。

### Checkpoint 7：Action Set、武器与表现变体

游戏行为：角色切换武器或形态后，输入和语义派生关系保持稳定，但可用 Action、动画表现或 Effect 发生变化。

本次才接入：

- Active CombatActionSet 切换；
- 武器 Combat Profile；
- Normal / PowerUp Presentation；
- 必要时的 AnimatorOverrideController。

### Checkpoint 8：作者工具与数据验证

当 Action 数量开始造成真实维护压力后，再增加编辑器验证：

- Animator State 是否存在；
- Action 是否缺少 Stage；
- Transition 的目标是否为空；
- Window ID 是否与动画事件匹配；
- 是否存在永远不可达或明显循环的派生；
- 多条同级候选是否会同时成立；
- Action Definition 是否错误保存运行时状态。

只有 ScriptableObject 引用已经难以阅读时，才评估可视化 Graph Editor。

## 13. 测试与调试策略

### 13.1 Play Mode

每个 Checkpoint 都需要一个可以主动触发的测试入口和明确观察结果，不能以“没有编译错误”为完成标准。

临时日志应包含：

- Request 的 Command 与时间；
- 候选 Action 及拒绝原因；
- 当前 Action/Stage；
- 打开和关闭的 Window；
- Transition 成功目标；
- Action ExitReason；
- 交还 FSM 后的状态。

Checkpoint 通过后删除高频临时日志，只保留缺少配置、非法 Signal、无效引用等长期警告。

### 13.2 EditMode

纯逻辑部分稳定后，优先测试：

- 输入缓冲到期与消费；
- Transition 顺序；
- Condition 全部满足/任一失败；
- 没有成功候选时 Request 保留；
- Stage 自动推进；
- Action Definition 不保存 Runtime 状态。

是否拆分 Combat Assembly Definition 在纯逻辑边界稳定后单独决策。

## 14. 已知风险与防护

### 两套系统争夺控制权

防护：所有战斗 Action 必须通过唯一桥接状态执行；普通移动 FSM 不在后台继续写动作速度和动画。

### Animation Event 污染新 Action

防护：Signal 必须由当前 Runtime 和 Stage 校验；Action/Stage 切换时关闭旧窗口并清理旧信号状态。

### ScriptableObject 保存运行时数据

防护：Definition、Condition、Effect 全部无状态；运行时数据只进入 CombatActionRuntime。

### Controller 逐渐变成巨大 if/switch

防护：新增具体行为应优先增加 Condition、Effect 或内容资源；Controller 只处理通用生命周期。

### Priority 变成难以理解的数字

防护：当前 Action Transitions 优先；没有当前派生时，Action Set 使用清晰的序列化顺序，第一个满足条件的入口获胜。

### 多 Stage Hit Window 造成重复伤害

防护：实现伤害前明确命中记录作用域，并让每个 Stage/Window 显式声明重复命中策略。

### PowerUp 复制整套动作关系

防护：语义 Action 和 Presentation 分离；只有真实规则不同才建立独立 Definition 或覆盖规则。

### 未提交素材被误改或遗漏

防护：每个 Checkpoint 开始和结束都查看完整工作区；不丢弃作者现有修改；Unity 资源和 `.meta` 一起管理。

## 15. 暂未决定但不阻塞骨架的问题

- 轻攻击、重攻击、Parry 的最终 Input Action 与按下/按住/松开语义；
- Action 是否可以直接取消回 Dodge、Jump 等移动 FSM 状态；
- 目标选择、锁定与前方检测方式；
- 敌人浮空、霸体、重量和抗性模型；
- 命中停顿、受击停顿、击退与相机反馈；
- 同一目标在多 Stage Action 中能被命中几次；
- PowerUp 只更换表现，还是也改变时间轴、伤害和派生；
- 武器 ItemData、装备数据和 CombatActionSet 的最终关联位置；
- 是否以及何时拆出独立 Combat Assembly Definition。

这些问题在对应行为进入当前 Checkpoint 时再做决定，不提前阻塞最小执行闭环。

## 16. 文档与 Git 要求

每个完成的 Checkpoint 必须：

1. 在 Unity Play Mode 验证可观察行为。
2. 检查 Console，并区分项目错误与工具产生的日志。
3. 执行 `git status --short` 和相关 `git diff`。
4. 确认没有混入无关的动画、场景、字体或导入设置修改。
5. 确认新增 Unity 资源带对应 `.meta`。
6. 同步更新 `FirstGameDetails.md` 中真实存在的脚本职责、核心函数、资源与调用链。
7. 如果遇到难以定位的系统性 Bug，在 `DevLog.md` 记录现象、假设、最小实验、证据、根因、修复和剩余问题。

## 17. 第一轮实施边界

下一轮正式开始编码时，只执行 Checkpoint 0 和 Checkpoint 1。

第一轮成功标准不是“已经有成熟连招”，而是：

> 一个不包含任何具体招式判断的 Action Runtime，能够从输入缓冲中选择一项数据资源，通过唯一桥接状态取得 Player 控制权，完整播放一个 Stage，并在收到动画结束 Signal 后把控制权安全交还给移动 FSM。

只有该闭环在 Idle、移动和边界输入情况下都稳定后，才进入派生窗口 Checkpoint。
