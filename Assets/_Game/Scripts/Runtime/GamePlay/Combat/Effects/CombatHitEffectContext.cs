using UnityEngine;

public readonly struct CombatHitEffectContext
{
    public GameObject SourceOwner { get; }
    public GameObject TargetOwner { get; }

    public CombatHitEffectContext(GameObject sourceOwner, GameObject targetOwner)
    {
        SourceOwner = sourceOwner;
        TargetOwner = targetOwner;
    }
}
