using UnityEngine;

public class EnemyCombatExecutor : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    [SerializeField] private Animator anim;

    public CombatExecutionStatus Status { get; private set; } = CombatExecutionStatus.NotStarted;
    public CombatActionController CombatActionController => enemy != null ? enemy.CombatActionController : null;

    private AIDisplacementMotor displacementMotor;

    private void Awake()
    {
        displacementMotor = GetComponent<AIDisplacementMotor>();
        if (enemy == null)
            enemy = GetComponent<Enemy>();
        if (anim == null)
            anim = GetComponentInChildren<Animator>();
    }

    public bool TryStartAction(CombatActionDefinition action, in CombatActionContext context)
    {
        if (!isActiveAndEnabled || Status == CombatExecutionStatus.Running) return false;
        if (action == null || enemy == null ||
            !enemy.isActiveAndEnabled || !CanUseAnimator())
        {
            Debug.LogWarning($"[EnemyCombat] {name}: action, enabled Enemy owner or usable Animator is missing; attack not started.", this);
            return false;
        }

        if (!CombatActionController.TryStartAction(action, in context)) return false;

        Status = CombatExecutionStatus.Running;
        if (!TryPlayCurrentStage())
        {
            CancelCurrentExecution();
            return false;
        }

        CombatActionController.StartCooldown(action);
        Debug.Log($"[EnemyCombat] {name}: started attack '{action.name}'.", this);
        return true;
    }

    // Called by the attack task to check whether the animation finished its stage.
    public void Tick()
    {
        if (Status != CombatExecutionStatus.Running) return;
        if (!isActiveAndEnabled || enemy == null ||
            !enemy.isActiveAndEnabled || !CanUseAnimator())
        {
            Debug.LogWarning($"[EnemyCombat] {name}: executor, Enemy owner or Animator became unavailable; attack cancelled.", this);
            CancelCurrentExecution();
            return;
        }

        // An external interruption must not be reported as successful completion.
        if (CombatActionController.CurrentAction == null)
        {
            CancelCurrentExecution();
            return;
        }

        if (!CombatActionController.TryCompleteAction()) return;

        displacementMotor?.StopDisplacement(this);
        Status = CombatExecutionStatus.Succeeded;
        Debug.Log($"[EnemyCombat] {name}: attack completed successfully.", this);
    }

    // Clears combat runtime only; animation playback is not stopped here.
    public void CancelCurrentExecution()
    {
        if (Status != CombatExecutionStatus.Running) return;

        displacementMotor?.StopDisplacement(this);
        Status = CombatExecutionStatus.Cancelled;
        if (CombatActionController != null)
            CombatActionController.CancelCurrentAction();
        Debug.Log($"[EnemyCombat] {name}: attack cancelled.", this);
    }

    private bool TryPlayCurrentStage()
    {
        CombatActionStage stage = CombatActionController.CurrentCombatStage;
        if (stage == null || !anim.HasState(0, stage.AnimatorStateHash))
        {
            Debug.LogWarning($"[EnemyCombat] {name}: current stage has no matching Animator state on layer 0; attack cannot continue.", this);
            return false;
        }

        displacementMotor?.StopDisplacement(this);
        if (displacementMotor != null)
            displacementMotor.TryStartDisplacement(stage.AIDisplacement, displacementMotor.FacingDirection, this);
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
