using UnityEngine;

public class AITargetInRangeTask : AITask
{
    private float maxRange;
    private Transform targetTransform;
    private Transform Owner;

    public AITargetInRangeTask(in AIContext context,float maxRange)
    {
        if(context.Owner == null)
        {
            throw new System.ArgumentNullException("CandidateTarget or Owner is null");
        }

        this.maxRange = maxRange;
        targetTransform = context.CandidateTarget;
        Owner = context.Owner;
    }
    protected override AITaskStatus OnTick(float deltaTime)
    {
        if(targetTransform == null || Owner == null)
        {
            return AITaskStatus.Failed;
        }
        if(targetTransform == Owner || !targetTransform.gameObject.activeInHierarchy)
        {
            return AITaskStatus.Failed;
        }
        float distance = Vector2.Distance(targetTransform.position, Owner.position);
        if(distance <= maxRange)
        {
            return AITaskStatus.Succeeded;
        }
        return AITaskStatus.Failed;
    }
}
