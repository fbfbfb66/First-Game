using UnityEngine;

[RequireComponent(typeof(Enemy))]
public sealed class EnemyInterruptExecutor : MonoBehaviour
{
    [SerializeField] private Animator anim;
    private Enemy enemy;
    private EnemyAIBrain brain;
    private MotionController controller;
    private AIDisplacementMotor displacementMotor;
    public bool IsRunning { get; private set; }

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        displacementMotor = GetComponent<AIDisplacementMotor>();
        brain = GetComponent<EnemyAIBrain>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        controller = new MotionController(this);
    }

    public bool TryStartAction(MotionActionDefinition motionDef, in AIActionContext context)
    {
        if (!isActiveAndEnabled || IsRunning || enemy == null || !enemy.isActiveAndEnabled ||
            enemy.Health == null || enemy.Health.IsDead) return false;
        if (motionDef == null || anim == null || !anim.isActiveAndEnabled)
        {
            Debug.LogWarning($"[EnemyInterrupt] {name}: Motion Action or enabled Animator is missing; external action not started.", this);
            return false;
        }
        if (!enemy.CombatActionController.CanInterrupt()) return false;
        if (!motionDef.TrySelectStage(in context, out var stage) || !anim.HasState(0, stage.AnimatorStateHash))
        {
            Debug.LogWarning($"[EnemyInterrupt] {name}: no usable stage in {motionDef.name}; external action not started.", this);
            return false;
        }
        if (!controller.TryStartAction(motionDef, in context)) return false;
        // Gate task selection before abort callbacks can finish the current task.
        IsRunning = true;
        brain?.SuspendExecution();
        enemy.CombatActionController.TryInterrupt();
        controller.BeginState(stage.AnimatorStateHash, motionDef, in context);
        if (displacementMotor != null)
        {
            float direction = displacementMotor.FacingDirection;
            if (context.DamageContext.HasValue && context.DamageContext.Value.Attacker != null)
            {
                float dx = transform.position.x - context.DamageContext.Value.Attacker.transform.position.x;
                if (!Mathf.Approximately(dx, 0f)) direction = Mathf.Sign(dx);
            }
            displacementMotor.TryStartDisplacement(stage.AIDisplacement, direction, this);
        }
        anim.CrossFade(stage.AnimatorStateHash, 0f, 0, 0f);
        return true;
    }

    public void NotifyAnimationFinished(int sourceStateHash)
    {
        if (!IsRunning || !controller.IsCurrentStateAnimation(sourceStateHash)) return;
        displacementMotor?.StopDisplacement(this);
        controller.EndState(sourceStateHash);
        IsRunning = false;
        if (enemy != null && enemy.isActiveAndEnabled && enemy.Health != null && !enemy.Health.IsDead)
            brain?.ResumeExecution();
    }

    private void OnDisable()
    {
        displacementMotor?.StopDisplacement(this);
        if (controller != null && controller.CurrentStage != null)
            controller.EndState(controller.CurrentStage.AnimatorStateHash);
        bool wasRunning = IsRunning;
        IsRunning = false;
        if (wasRunning && enemy != null && enemy.isActiveAndEnabled && enemy.Health != null && !enemy.Health.IsDead)
            brain?.ResumeExecution();
    }
}
