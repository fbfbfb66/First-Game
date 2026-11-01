using UnityEngine;

public class EnemyCombatAnimationTrigger : MonoBehaviour
{
    [SerializeField] private EnemyCombatActionController controller;

    public void FinishCombatStage(AnimationEvent animationEvent)
    {
        if (controller == null || controller.CurrentAction == null)
        {
            Debug.LogWarning($"[EnemyCombat] {name}: controller or current action is missing; cannot finish combat stage.", this);
            return;
        }
        controller.NotifyStageFinished(animationEvent.animatorStateInfo.shortNameHash);
    }
}
