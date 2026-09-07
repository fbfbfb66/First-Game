using UnityEngine;
[CreateAssetMenu(
    fileName = "Condition_RequiredHits_",
    menuName = "Game/Combat/Conditions/Required Hits")]
public sealed class CombatActionRequiredHitsCondition : CombatActionCondition
{
    [SerializeField] private CombatActionDefinition[] requiredActions;

    public override bool IsMet(in CombatActionContext context)
    {
        if(requiredActions == null || requiredActions.Length == 0)
        {
            Debug.LogWarning("requiredActions is wrong");
            return false;
        }
        foreach(var action in requiredActions)
        {
            if(action == null)
            {
                Debug.LogWarning("action is null");
                return false;
            }
            if (context.HasConfirmedHit(action) == false) return false;
        }
        return true;
    }
}
