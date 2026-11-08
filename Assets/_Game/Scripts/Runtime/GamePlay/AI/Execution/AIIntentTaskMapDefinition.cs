using System;
using UnityEngine;

[Serializable]
public sealed class AIIntentTaskBinding
{
    [SerializeField] private AIIntentKey intentKey;
    [SerializeField] private AITaskDefinition taskDefinition;

    public AIIntentKey IntentKey => intentKey;
    public AITaskDefinition TaskDefinition => taskDefinition;
}

[CreateAssetMenu(fileName = "IntentTaskMap_", menuName = "Game/AI/Intent Task Map")]
public class AIIntentTaskMapDefinition : ScriptableObject
{
    [SerializeField] private AIIntentTaskBinding[] bindings;

    public bool TryGetTask(AIIntentKey key, out AITaskDefinition taskDefinition)
    {
        taskDefinition = null;
        if (key == null)
        {
            Debug.LogWarning($"[AIBehavior] {name}: intent key is missing; task not resolved.", this);
            return false;
        }

        if (bindings == null || bindings.Length == 0)
        {
            Debug.LogWarning($"[AIBehavior] {name}: no bindings configured for intent '{key.name}'; task not resolved.", this);
            return false;
        }

        AIIntentTaskBinding selectedBinding = null;
        foreach (AIIntentTaskBinding binding in bindings)
        {
            if (binding == null || binding.IntentKey != key) continue;
            if (selectedBinding != null)
            {
                Debug.LogWarning($"[AIBehavior] {name}: duplicate bindings for intent '{key.name}'; task not resolved.", this);
                return false;
            }

            selectedBinding = binding;
        }

        if (selectedBinding == null)
        {
            Debug.LogWarning($"[AIBehavior] {name}: no binding found for intent '{key.name}'; task not resolved.", this);
            return false;
        }

        if (selectedBinding.TaskDefinition == null)
        {
            Debug.LogWarning($"[AIBehavior] {name}: intent '{key.name}' has no task definition assigned; task not resolved.", this);
            return false;
        }

        taskDefinition = selectedBinding.TaskDefinition;
        return true;
    }
}
