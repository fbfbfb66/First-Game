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
    [SerializeField] private bool repeat;
    [SerializeField] private AIUtilityOptionSetDefinition utilityOptionSet;
    private readonly AIUtilityDecisionMaker utilityDecisionMaker = new AIUtilityDecisionMaker();
    private AITask currentTask;

    private void Awake()
    {
        if (flightMotor == null)
            flightMotor = GetComponent<EnemyFlightMotor>();
        if(combatExecutor == null)
            combatExecutor = GetComponent<EnemyCombatExecutor>();
    }

    private void FixedUpdate()
    {
        if (currentTask == null) return;
        if (currentTask.Status != AITaskStatus.Running)
        {
            if (repeat) TryStartNextTask();
            return;
        }
        currentTask.Tick(WorldTime.fixedDeltaTime);
    }

    private void OnEnable()
    {
        TryStartNextTask();
    }

    private bool TryStartNextTask()
    {
        if (intentTaskMap == null || utilityOptionSet == null)
        {
            Debug.LogWarning($"[EnemyAIBrain] {name}: intent task map or utility option set is missing; task not created.", this);
            return false;
        }

        AIAgentContext context = new AIAgentContext(transform, target, flightMotor, combatExecutor);
        if (!utilityDecisionMaker.TrySelect(in context, utilityOptionSet.Options, out AIUtilityEvaluation selectedEvaluation))
        {
            Debug.LogWarning($"[EnemyAIBrain] {name}: no valid feasible candidate was found; task not created.", this);
            return false;
        }

        AIIntent intent = selectedEvaluation.Intent;
        if (!intentTaskMap.TryGetTask(intent.Key, out AITaskDefinition selectedDefinition)) return false;
        AIAgentContext executionContext = new AIAgentContext(transform, intent.Target, flightMotor, combatExecutor);

        AbortCurrentTask();
        currentTask = selectedDefinition.CreateTask(in executionContext);
        if (currentTask == null)
        {
            Debug.LogWarning($"[EnemyAIBrain] {name}: selected definition '{selectedDefinition.name}' could not create a task.", this);
            return false;
        }

        string targetName = intent.Target != null ? intent.Target.name : "none";
        Debug.Log($"[EnemyAIBrain] {name}: intent='{intent.Key.name}', target='{targetName}', score={selectedEvaluation.UtilityScore:F4} -> task='{selectedDefinition.name}'. Reason: {selectedEvaluation.Reason}", this);
        return true;
    }

    private void OnDisable()
    {
        AbortCurrentTask();
    }

    private void AbortCurrentTask()
    {
        AITask task = currentTask;
        currentTask = null;
        task?.Abort();
    }
}
