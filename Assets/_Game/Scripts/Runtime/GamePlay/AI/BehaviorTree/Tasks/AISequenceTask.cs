using System;

public class AISequenceTask : AITask
{
    private readonly AITask[] children;
    private int currentIndex;

    public override bool CanBeReplanned => ChildCanbBePlanned();

    private bool ChildCanbBePlanned()
    {
        foreach (AITask child in children)
        {
            if (child.CanBeReplanned)
            {
                return true;
            }
        }
        return false;
    }

    public AISequenceTask(AITask[] children)
    {
        if (children == null)
        {
            throw new ArgumentNullException(nameof(children));
        }
        if (children.Length == 0)
        {
            throw new ArgumentException("Children tasks cannot be empty.", nameof(children));
        }
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] == null)
            {
                throw new ArgumentException($"Child task at index {i} cannot be null.", nameof(children));
            }
        }

        this.children = (AITask[])children.Clone();
        currentIndex = 0;
    }

    protected override AITaskStatus OnTick(float deltaTime)
    {
        AITask child = children[currentIndex];
        AITaskStatus childStatus = child.Tick(deltaTime);
        if (childStatus == AITaskStatus.Succeeded)
        {
            currentIndex++;
            if (currentIndex >= children.Length)
            {
                return AITaskStatus.Succeeded;
            }
        }
        else if (childStatus == AITaskStatus.Failed)
        {
            return AITaskStatus.Failed;
        }
        return AITaskStatus.Running;
    }

    protected override void OnEnd(bool interrupted)
    {
        if (interrupted && currentIndex < children.Length)
            children[currentIndex].Abort();
    }
}
