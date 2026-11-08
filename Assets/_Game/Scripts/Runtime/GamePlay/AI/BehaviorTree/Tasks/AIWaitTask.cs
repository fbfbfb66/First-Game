public class AIWaitTask : AITask
{
    public float duration { get; }
    public float elapsed { get; private set; }

    public AIWaitTask(float duration)
    {
        if(duration <= 0f)
        {
            throw new System.ArgumentException("Duration must be greater than zero.", nameof(duration));
        }

        this.duration = duration;
        elapsed = 0f;
    }

    protected override AITaskStatus OnTick(float deltaTime)
    {
        elapsed += deltaTime;
        if (elapsed >= duration)
        {
            return AITaskStatus.Succeeded;
        }
        return AITaskStatus.Running;
    }
}
