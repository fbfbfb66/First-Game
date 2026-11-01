using System;
using UnityEngine;

public class AIPlayAnimationTask : AITask
{
    private readonly Transform owner;
    private readonly string stateName;
    private readonly int stateHash;

    public AIPlayAnimationTask(in AIContext context, string stateName)
    {
        if (context.Owner == null)
            throw new ArgumentNullException(nameof(context.Owner));
        if (string.IsNullOrWhiteSpace(stateName))
            throw new ArgumentException("Animation state name cannot be empty.", nameof(stateName));

        owner = context.Owner;
        this.stateName = stateName;
        stateHash = Animator.StringToHash(stateName);
    }

    protected override AITaskStatus OnTick(float deltaTime)
    {
        if (owner == null) return AITaskStatus.Failed;

        Animator animator = owner.GetComponentInChildren<Animator>();
        if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"[AI] {owner.name}: no usable Animator; animation '{stateName}' not requested.", owner);
            return AITaskStatus.Failed;
        }
        if (!animator.HasState(0, stateHash))
        {
            Debug.LogWarning($"[AI] {owner.name}: animation state '{stateName}' is missing on layer 0; animation not requested.", owner);
            return AITaskStatus.Failed;
        }

        animator.CrossFade(stateHash, 0f, 0, 0f);
        // Success means the playback request was submitted, not that the clip ended.
        return AITaskStatus.Succeeded;
    }
}
