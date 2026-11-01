using UnityEngine;

public class AILogTask : AITask
{
    private string _logMessage;

    public AILogTask(string logMessage)
    {
        if(string.IsNullOrWhiteSpace(logMessage))
        {
            throw new System.ArgumentException("Log message cannot be null or whitespace.", nameof(logMessage));
        }

        _logMessage = logMessage;
    }
    protected override AITaskStatus OnTick(float deltaTime)
    {
        Debug.Log(_logMessage);
        return AITaskStatus.Succeeded;
    }
}
