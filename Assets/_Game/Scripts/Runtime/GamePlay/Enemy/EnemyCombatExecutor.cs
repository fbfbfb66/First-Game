using UnityEngine;

public class EnemyCombatExecutor : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    [SerializeField] private Animator anim;

    public CombatExecutionStatus Status { get; private set; } = CombatExecutionStatus.NotStarted;
    public CombatActionController CombatActionController => enemy != null ? enemy.CombatActionController : null;

    private AIDisplacementMotor displacementMotor;
    private EnemyFlightMotor facingMotor;
    public Vector2 LockedDirection { get; private set; }

    private void Awake()
    {
        displacementMotor = GetComponent<AIDisplacementMotor>();
        facingMotor = GetComponent<EnemyFlightMotor>();
        if (enemy == null)
            enemy = GetComponent<Enemy>();
        if (anim == null)
            anim = GetComponentInChildren<Animator>();
    }

    public bool TryStartAction(CombatActionDefinition action, in CombatActionContext context, Transform target = null)
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
        if (!TryPlayCurrentStage(target))
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

        EndStageMovement();
        Status = CombatExecutionStatus.Succeeded;
        Debug.Log($"[EnemyCombat] {name}: attack completed successfully.", this);
    }

    // Clears combat runtime only; animation playback is not stopped here.
    public void CancelCurrentExecution()
    {
        if (Status != CombatExecutionStatus.Running) return;

        EndStageMovement();
        Status = CombatExecutionStatus.Cancelled;
        if (CombatActionController != null)
            CombatActionController.CancelCurrentAction();
        Debug.Log($"[EnemyCombat] {name}: attack cancelled.", this);
    }

    private bool TryPlayCurrentStage(Transform target)
    {
        CombatActionStage stage = CombatActionController.CurrentCombatStage;
        if (stage == null || !anim.HasState(0, stage.AnimatorStateHash))
        {
            Debug.LogWarning($"[EnemyCombat] {name}: current stage has no matching Animator state on layer 0; attack cannot continue.", this);
            return false;
        }

        EndStageMovement();
        Vector2 direction = displacementMotor != null && displacementMotor.FacingDirection < 0f ? Vector2.left : Vector2.right;
        if (stage is AICombatActionStage aiStage && aiStage.DisplacementDirection == AICombatDisplacementDirection.TowardsTarget)
        {
            if (target == null || !target.gameObject.activeInHierarchy || facingMotor == null || displacementMotor == null)
            {
                Debug.LogWarning($"[EnemyCombat] {name}: target or directional motors missing; targeted attack not started.", this);
                return false;
            }
            Vector2 delta = target.position - transform.position;
            if (delta.sqrMagnitude > Mathf.Epsilon) direction = delta.normalized;
            if (!facingMotor.TryLockFacing(direction, this)) return false;
            LockedDirection = direction;
        }
        if (displacementMotor != null)
            displacementMotor.TryStartDisplacement(stage.AIDisplacement, direction, this);
        anim.CrossFade(stage.AnimatorStateHash, 0f, 0, 0f);
        return true;
    }

    private void EndStageMovement()
    {
        displacementMotor?.StopDisplacement(this);
        facingMotor?.ReleaseFacing(this);
        LockedDirection = Vector2.zero;
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
