using UnityEngine;
[CreateAssetMenu(fileName = "AILogTaskDefinition", menuName = "Game/AI/Tasks/Log")]
public class AILogTaskDefinition : AITaskDefinition
{
    [SerializeField] private string logMessage;
    public string LogMessage => logMessage;
    public override AITask CreateTask(in AIContext context)
    {
        if(string.IsNullOrWhiteSpace(logMessage))
        {
            Debug.LogWarning("Log message cannot be null or whitespace.");
            return null;
        }
        return new AILogTask(logMessage);
    }
}
