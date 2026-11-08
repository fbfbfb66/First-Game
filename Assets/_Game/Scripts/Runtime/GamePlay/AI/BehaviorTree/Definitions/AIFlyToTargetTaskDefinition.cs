using UnityEngine;
[CreateAssetMenu(fileName = "AIFlyTaskDefinition", menuName = "Game/AI/Tasks/Fly")]
public class AIFlyToTargetTaskDefinition : AITaskDefinition
{
   [SerializeField] private float speed;
   [SerializeField] private float stoppingDistance;
   [SerializeField] private float giveUpDistance;

    public override AITask CreateTask(in AIAgentContext context)
    {
        if(context.FlightMotor == null)
        {
            Debug.LogWarning("非法数值");
            return null;
        }
        return new AIFlyToTargetTask(in context, speed, stoppingDistance, giveUpDistance);
    }
}
