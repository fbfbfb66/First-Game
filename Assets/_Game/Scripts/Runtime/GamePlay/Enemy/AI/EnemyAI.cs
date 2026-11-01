using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private EnemyCombatExecutor combatExecutor;
    [SerializeField] private EnemyFlightMotor flightMotor;
    [SerializeField] private AITaskDefinition taskDefinition;
    [SerializeField] private Transform candidateTarget;
    [SerializeField] private bool repeat;
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
            if (repeat) TryCreateTask();
            return;
        }
        currentTask.Tick(WorldTime.fixedDeltaTime);
    }

    private void OnEnable()
    {
        TryCreateTask();
    }

    private bool TryCreateTask()
    {
        AbortCurrentTask();
        if (taskDefinition == null)
        {
            Debug.LogWarning($"{name} has no task definition assigned.");
            return false;
        }
        AIContext context = new AIContext(transform, candidateTarget,flightMotor,combatExecutor);
        currentTask = taskDefinition.CreateTask(in context);
        if (currentTask == null) return false;
        Debug.Log($"{name} started task : now {Time.time}.");
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
