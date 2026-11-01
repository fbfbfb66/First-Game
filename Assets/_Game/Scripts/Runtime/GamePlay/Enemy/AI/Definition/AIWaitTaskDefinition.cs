using UnityEngine;
[CreateAssetMenu(fileName = "AIWaitTaskDefinition", menuName = "Game/AI/Tasks/Wait")]
public class AIWaitTaskDefinition : AITaskDefinition
{
    [Min(0f)]
    [SerializeField] private float duration = 3f;

    public float Duration => duration;

    public override AITask CreateTask(in AIContext context)
    {
        if (duration <= 0f)
        {
            Debug.LogWarning("非法配置");
            return null;
        }
        return new AIWaitTask(duration);
    }
}
