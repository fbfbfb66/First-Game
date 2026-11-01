using UnityEngine;
[CreateAssetMenu(fileName = "AISelectorTask", menuName = "Game/AI/Tasks/Selector")]
public class AISelectorTaskDefinition : AITaskDefinition
{
    [SerializeField] private AITaskDefinition[] children;
    [System.NonSerialized] private bool isCreatingTask;

    public override AITask CreateTask(in AIContext context)
    {
        if (isCreatingTask)
        {
            Debug.LogWarning($"[AI] Sequence '{name}' contains a recursive reference; sequence not created.", this);
            return null;
        }

        isCreatingTask = true;
        try
        {
            if (children == null || children.Length == 0)
            {
                Debug.LogWarning($"[AI] Sequence '{name}' has no child definitions; sequence not created.", this);
                return null;
            }

            AITask[] tasks = new AITask[children.Length];
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i] == null)
                {
                    Debug.LogWarning($"[AI] Sequence '{name}' is missing child definition at index {i}; sequence not created.", this);
                    return null;
                }

                tasks[i] = children[i].CreateTask(in context);
                if (tasks[i] == null)
                {
                    Debug.LogWarning($"[AI] Sequence '{name}' could not create child '{children[i].name}' at index {i}; sequence not created.", this);
                    return null;
                }
            }

            return new AISelectorTask(tasks);
        }
        finally
        {
            // This marks the synchronous creation call, not the task's execution lifetime.
            isCreatingTask = false;
        }
    }
}
