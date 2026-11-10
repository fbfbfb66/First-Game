using UnityEngine;
using UnityEngine.Serialization;

public class EnemyAIBrain : MonoBehaviour
{
    [SerializeField] private EnemyCombatExecutor combatExecutor;
    [SerializeField] private EnemyFlightMotor flightMotor;
    [FormerlySerializedAs("behaviorMapping")]
    [SerializeField] private AIIntentTaskMapDefinition intentTaskMap;
    [FormerlySerializedAs("candidateTarget")]
    [SerializeField] private Transform target;
    [SerializeField] private Transform idlePoint;
    public Vector2 IdlePosition { get; private set; }
    [SerializeField] private bool repeat;
    [SerializeField] private AIUtilityOptionSetDefinition utilityOptionSet;
    private readonly AIUtilityDecisionMaker utilityDecisionMaker = new AIUtilityDecisionMaker();
    private AITask currentTask;
    private AIIntent currentIntent;
    public bool IsSuspended { get; private set; }
    public AIUtilityOptionSetDefinition UtilityOptionSet => utilityOptionSet;
    public AIUtilityEvaluation? CurrentEvaluation { get; private set; }
    public AITaskDefinition CurrentTaskDefinition { get; private set; }
    public AITaskStatus? CurrentTaskStatus => currentTask?.Status;
    public bool CanReplanCurrentTask => !IsSuspended && currentTask != null &&
        currentTask.Status == AITaskStatus.Running && currentTask.CanBeReplanned;

    // Read-only evaluation for debugging; it never selects, starts or cancels a task.
    public bool TryEvaluateUtilityOption(AIUtilityOptionDefinition option, out AIUtilityEvaluation evaluation)
    {
        evaluation = default;
        if (option == null) return false;
        AIAgentContext context = new AIAgentContext(transform, target, flightMotor, combatExecutor, idlePosition: IdlePosition);
        return option.TryEvaluate(in context, out evaluation);
    }

    private void Awake()
    {
        IdlePosition = idlePoint != null ? (Vector2)idlePoint.position : (Vector2)transform.position;
        if (flightMotor == null)
            flightMotor = GetComponent<EnemyFlightMotor>();
        if (combatExecutor == null)
            combatExecutor = GetComponent<EnemyCombatExecutor>();
    }

    private void FixedUpdate()
    {
        if (IsSuspended || currentTask == null) return;

        TryReplanCurrentTask();

        if (currentTask.Status != AITaskStatus.Running)
        {
            if (repeat) TryStartNextTask();
            return;
        }
        currentTask.Tick(WorldTime.fixedDeltaTime);
    }

    private void OnEnable()
    {
        var interrupt = GetComponent<EnemyInterruptExecutor>();
        if (interrupt == null || !interrupt.IsRunning) IsSuspended = false;
        TryStartNextTask();
    }

    private bool TryReplanCurrentTask()
    {

        if (intentTaskMap == null || utilityOptionSet == null)
        {
            Debug.LogWarning($"[EnemyAIBrain] {name}: intent task map or utility option set is missing; task not created.", this);
            return false;
        }
        if (IsSuspended || currentTask == null || !currentTask.CanBeReplanned || currentTask.Status != AITaskStatus.Running) return false;

        AIAgentContext context = new AIAgentContext(transform, target, flightMotor, combatExecutor, idlePosition: IdlePosition);

        if (!utilityDecisionMaker.TrySelect(in context, utilityOptionSet.Options, out AIUtilityEvaluation selectedEvaluation))
        {
            Debug.LogWarning($"[EnemyAIBrain] {name}: no valid feasible candidate was found; task not created.", this);
            return false;
        }

        AIIntent intent = selectedEvaluation.Intent;
        if (!intentTaskMap.TryGetTask(intent.Key, out AITaskDefinition selectedDefinition)) return false;
        AIAgentContext executionContext = new AIAgentContext(transform, intent.Target, flightMotor, combatExecutor, intent.Action, IdlePosition);

        AITask candidateTask = selectedDefinition.CreateTask(in executionContext);

        if (currentIntent.Key == intent.Key && currentIntent.Action == intent.Action && currentIntent.Target == intent.Target) return false;

        AbortCurrentTask();

        currentTask = candidateTask;
        currentIntent = intent;

        if (currentTask != null)
        {
            CurrentEvaluation = selectedEvaluation;
            CurrentTaskDefinition = selectedDefinition;
        }

        return true;
    }

    private bool TryStartNextTask()
    {
        if (IsSuspended) return false;
        if (intentTaskMap == null || utilityOptionSet == null)
        {
            Debug.LogWarning($"[EnemyAIBrain] {name}: intent task map or utility option set is missing; task not created.", this);
            return false;
        }

        AIAgentContext context = new AIAgentContext(transform, target, flightMotor, combatExecutor, idlePosition: IdlePosition);
        if (!utilityDecisionMaker.TrySelect(in context, utilityOptionSet.Options, out AIUtilityEvaluation selectedEvaluation))
        {
            Debug.LogWarning($"[EnemyAIBrain] {name}: no valid feasible candidate was found; task not created.", this);
            return false;
        }

        AIIntent intent = selectedEvaluation.Intent;
        if (!intentTaskMap.TryGetTask(intent.Key, out AITaskDefinition selectedDefinition)) return false;
        AIAgentContext executionContext = new AIAgentContext(transform, intent.Target, flightMotor, combatExecutor,intent.Action, IdlePosition);

        AbortCurrentTask();
        currentTask = selectedDefinition.CreateTask(in executionContext);
        currentIntent = intent;
        if (currentTask == null)
        {
            Debug.LogWarning($"[EnemyAIBrain] {name}: selected definition '{selectedDefinition.name}' could not create a task.", this);
            return false;
        }

        CurrentEvaluation = selectedEvaluation;
        CurrentTaskDefinition = selectedDefinition;
        string targetName = intent.Target != null ? intent.Target.name : "none";
        Debug.Log($"[EnemyAIBrain] {name}: intent='{intent.Key.name}', target='{targetName}', score={selectedEvaluation.UtilityScore:F4} -> task='{selectedDefinition.name}'. Reason: {selectedEvaluation.Reason}", this);
        return true;
    }

    public void SuspendExecution()
    {
        IsSuspended = true;
        AbortCurrentTask();
    }

    public void ResumeExecution()
    {
        if (!IsSuspended) return;
        IsSuspended = false;
        var enemy = GetComponent<Enemy>();
        if (isActiveAndEnabled && (enemy == null || enemy.Health == null || !enemy.Health.IsDead))
            TryStartNextTask();
    }

    private void OnDisable()
    {
        AbortCurrentTask();
    }

    private void AbortCurrentTask()
    {
        AITask task = currentTask;
        currentTask = null;
        currentIntent = default;
        CurrentEvaluation = null;
        CurrentTaskDefinition = null;
        task?.Abort();
    }
}
