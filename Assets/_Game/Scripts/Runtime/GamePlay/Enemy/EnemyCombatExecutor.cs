using UnityEngine;

public class EnemyCombatExecutor : MonoBehaviour
{
    [SerializeField] private EnemyCombatActionController combatActionController;
    [SerializeField] private Animator anim;

    public CombatExecutionStatus Status { get; private set; } = CombatExecutionStatus.NotStarted;

    private void Awake()
    {
        if (combatActionController == null)
            combatActionController = GetComponent<EnemyCombatActionController>();
        if (anim == null)
            anim = GetComponentInChildren<Animator>();
    }

    public bool TryStartAction(CombatActionDefinition action, in CombatActionContext context)
    {
        if (!isActiveAndEnabled || Status == CombatExecutionStatus.Running) return false;
        if (action == null || combatActionController == null ||
            !combatActionController.isActiveAndEnabled || !CanUseAnimator())
        {
            Debug.LogWarning($"[EnemyCombat] {name}: action, enabled controller or usable Animator is missing; attack not started.", this);
            return false;
        }

        if (!combatActionController.TryStartAction(action, in context)) return false;

        Status = CombatExecutionStatus.Running;
        if (!TryPlayCurrentStage())
        {
            CancelCurrentExecution();
            return false;
        }

        Debug.Log($"[EnemyCombat] {name}: started attack '{action.name}'.", this);
        return true;
    }

    // Called by the attack task to check whether the animation finished its stage.
    public void Tick()
    {
        if (Status != CombatExecutionStatus.Running) return;
        if (!isActiveAndEnabled || combatActionController == null ||
            !combatActionController.isActiveAndEnabled || !CanUseAnimator())
        {
            Debug.LogWarning($"[EnemyCombat] {name}: executor, controller or Animator became unavailable; attack cancelled.", this);
            CancelCurrentExecution();
            return;
        }

        // An external interruption must not be reported as successful completion.
        if (combatActionController.CurrentAction == null)
        {
            CancelCurrentExecution();
            return;
        }

        if (!combatActionController.TryCompleteAction()) return;

        Status = CombatExecutionStatus.Succeeded;
        Debug.Log($"[EnemyCombat] {name}: attack completed successfully.", this);
    }

    // Clears combat runtime only; animation playback is not stopped here.
    public void CancelCurrentExecution()
    {
        if (Status != CombatExecutionStatus.Running) return;

        Status = CombatExecutionStatus.Cancelled;
        if (combatActionController != null)
            combatActionController.CancelCurrentAction();
        Debug.Log($"[EnemyCombat] {name}: attack cancelled.", this);
    }

    private bool TryPlayCurrentStage()
    {
        CombatActionStage stage = combatActionController.CurrentCombatStage;
        if (stage == null || !anim.HasState(0, stage.AnimatorStateHash))
        {
            Debug.LogWarning($"[EnemyCombat] {name}: current stage has no matching Animator state on layer 0; attack cannot continue.", this);
            return false;
        }

        anim.CrossFade(stage.AnimatorStateHash, 0f, 0);
        return true;
    }

    private bool CanUseAnimator()
    {
        return anim != null && anim.isActiveAndEnabled && anim.runtimeAnimatorController != null;
    }

    private void OnDisable()
    {
        CancelCurrentExecution();
    }
}
