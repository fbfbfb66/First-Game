using UnityEngine;

public readonly struct CombatHitEffectContext
{
    public GameObject SourceOwner { get; }
    public GameObject TargetOwner { get; }

    public CombatHitWindowDefinition WindowDefinition { get; }

    public CombatHitEffectContext(GameObject sourceOwner, GameObject targetOwner, CombatHitWindowDefinition windowDefinition)
    {
        SourceOwner = sourceOwner;
        TargetOwner = targetOwner;
        WindowDefinition = windowDefinition;
    }
}
