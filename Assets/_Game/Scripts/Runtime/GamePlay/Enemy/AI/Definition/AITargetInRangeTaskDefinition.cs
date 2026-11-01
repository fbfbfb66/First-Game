using UnityEngine;
[CreateAssetMenu(fileName = "AITargetInRangeTaskDefinition", menuName = "Game/AI/Tasks/TargetInRange")]
public class AITargetInRangeTaskDefinition : AITaskDefinition
{
    [SerializeField] private float maxRange;

    public float MaxRange => maxRange;

    public override AITask CreateTask(in AIContext context)
    {
        if(maxRange <= 0f)
        {
            Debug.LogWarning("非法配置");
            return null;
        }

        return new AITargetInRangeTask(context, maxRange);
    }
}
