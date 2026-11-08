using UnityEngine;

public class EnemyCombatAnimationTrigger : MonoBehaviour
{
    [SerializeField] private Enemy enemy;

    private void Awake()
    {
        if (enemy == null)
            enemy = GetComponentInParent<Enemy>();
    }

    public void OpenCombatHitWindow(AnimationEvent animationEvent)
    {
        if (enemy == null) return;
        enemy.CombatActionController.NotifyHitWindowOpened(
            animationEvent.intParameter, animationEvent.animatorStateInfo.shortNameHash);
    }

    public void CloseCombatHitWindow(AnimationEvent animationEvent)
    {
        if (enemy == null) return;
        enemy.CombatActionController.NotifyHitWindowClosed(
            animationEvent.intParameter, animationEvent.animatorStateInfo.shortNameHash);
    }

    public void OpenCombatCancelWindow(AnimationEvent animationEvent)
    {
        if (enemy == null) return;
        enemy.CombatActionController.NotifyCancelWindowOpened(
            animationEvent.intParameter, animationEvent.animatorStateInfo.shortNameHash);
    }

    public void CloseCombatCancelWindow(AnimationEvent animationEvent)
    {
        if (enemy == null) return;
        enemy.CombatActionController.NotifyCancelWindowClosed(
            animationEvent.intParameter, animationEvent.animatorStateInfo.shortNameHash);
    }

    public void FinishInterruptAction(AnimationEvent animationEvent)
    {
        if (enemy != null && enemy.InterruptExecutor != null)
            enemy.InterruptExecutor.NotifyAnimationFinished(animationEvent.animatorStateInfo.shortNameHash);
    }

    public void FinishCombatStage(AnimationEvent animationEvent)
    {
        if (enemy == null || enemy.CombatActionController.CurrentAction == null)
        {
            Debug.LogWarning($"[EnemyCombat] {name}: Enemy owner or current action is missing; cannot finish combat stage.", this);
            return;
        }
        enemy.CombatActionController.NotifyStageFinished(animationEvent.animatorStateInfo.shortNameHash);
    }
}
