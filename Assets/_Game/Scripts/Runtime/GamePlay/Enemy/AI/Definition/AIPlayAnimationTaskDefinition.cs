using UnityEngine;

[CreateAssetMenu(fileName = "Task_PlayAnimation", menuName = "Game/AI/Tasks/Play Animation")]
public class AIPlayAnimationTaskDefinition : AITaskDefinition
{
    [SerializeField] private string stateName;

    public string StateName => stateName;

    public override AITask CreateTask(in AIContext context)
    {
        if (string.IsNullOrWhiteSpace(stateName) || context.Owner == null)
        {
            Debug.LogWarning($"[AI] {name}: animation state name or Owner is missing; task not created.", this);
            return null;
        }

        return new AIPlayAnimationTask(in context, stateName);
    }
}
