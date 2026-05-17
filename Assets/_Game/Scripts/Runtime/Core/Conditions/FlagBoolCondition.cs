using UnityEngine;
[CreateAssetMenu(
    fileName = "FlagBoolCondition",
    menuName = "Game/Condition/Flag Bool")]
public class FlagBoolCondition : GameCondition
{
    [SerializeField] private string flagID;
    [SerializeField] private bool expecteValue;

    public override bool IsMet(GameConditionContext context)
    {
        if(context.FlagCenter == null)
        {
            Debug.LogWarning("Missing flag Center");
            return false;
        }
        if (string.IsNullOrWhiteSpace(flagID))
        {
            Debug.LogWarning("Not valide flagId");
            return false;
        }
        if (!context.FlagCenter.HasBool(flagID))
        {
            Debug.LogWarning("Cannot check flag condition. FlagId is empty.");
            return false;
        }


        return expecteValue == context.FlagCenter.GetBool(flagID);
    }
}
