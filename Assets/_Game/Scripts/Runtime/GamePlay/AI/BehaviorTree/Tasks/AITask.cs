public abstract class AITask
{
    private bool hasStarted;
    private bool hasEnded;
    public virtual bool CanBeReplanned { get; }
    public AITaskStatus Status { get; private set; } = AITaskStatus.Running;

    public AITaskStatus Tick(float deltaTime)
    {
        if (hasEnded) return Status;

        if (!hasStarted)
        {
            hasStarted = true;
            AITaskStatus startStatus = OnStart();
            if (hasEnded) return Status;
            Status = startStatus;
        }

        if (Status == AITaskStatus.Running)
        {
            AITaskStatus tickStatus = OnTick(deltaTime);
            if (hasEnded) return Status;
            Status = tickStatus;
        }

        if (Status != AITaskStatus.Running) Finish(false);
        return Status;
    }

    public void Abort()
    {
        if (hasEnded) return;
        Status = AITaskStatus.Failed;
        Finish(true);
    }

    protected virtual AITaskStatus OnStart() => AITaskStatus.Running;
    protected abstract AITaskStatus OnTick(float deltaTime);
    protected virtual void OnEnd(bool interrupted) { }

    private void Finish(bool interrupted)
    {
        // Mark ended before callbacks so cancellation during cleanup is harmless.
        hasEnded = true;
        if (hasStarted) OnEnd(interrupted);
    }
}
