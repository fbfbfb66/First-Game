using UnityEngine;
using UnityEngine.Serialization;
[CreateAssetMenu(
    fileName = "FlagBoolCondition",
    menuName = "Game/Condition/Flag Bool")]
public class FlagBoolCondition : GameCondition
{
    [SerializeField] private string flagID;
    [FormerlySerializedAs("expecteValue")]
    [SerializeField] private bool expectedValue;

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


        return expectedValue == context.FlagCenter.GetBool(flagID);
    }
}
