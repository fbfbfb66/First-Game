using UnityEngine;

public abstract class AITaskDefinition : ScriptableObject
{
    public abstract AITask CreateTask(in AIAgentContext context);
}
