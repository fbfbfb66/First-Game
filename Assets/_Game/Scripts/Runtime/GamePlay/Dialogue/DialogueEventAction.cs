using UnityEngine;

public abstract class DialogueEventAction : ScriptableObject
{
    public abstract void Execute(DialogueContext context);
}
