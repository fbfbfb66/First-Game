using UnityEngine;
using UnityEngine.Serialization;
[CreateAssetMenu(
    fileName = "FlagBoolCondition",
    menuName = "Game/Condition/Flag Bool")]
public class FlagBoolCondition : GameCondition
{
    [SerializeField] private GameFlagData flagData;
    [FormerlySerializedAs("expecteValue")]
    [SerializeField] private bool expectedValue;

    public override bool IsMet(GameConditionContext context)
    {
        string flagID = flagData.FlagID;

        if (string.IsNullOrWhiteSpace(flagID))
        {
            Debug.LogWarning("NullOrWhitSpace : flagID");
            return false;
        }

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
