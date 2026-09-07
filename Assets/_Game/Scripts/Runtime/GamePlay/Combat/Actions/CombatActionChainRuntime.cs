using System.Collections.Generic;
using UnityEngine;

public sealed class CombatActionChainRuntime
{
    private readonly Dictionary<CombatActionDefinition, HashSet<GameObject>> hitTargetsByAction = new();
    public int ConfirmedActionCount => hitTargetsByAction.Count;

    public bool RecordHit(CombatActionDefinition action, GameObject targetOwner)
    {
        if(action == null || targetOwner == null) return false;

        if (hitTargetsByAction.TryGetValue(action,out var hitTarget)==false)
        {
            hitTarget = new HashSet<GameObject>();
            hitTargetsByAction.Add(action, hitTarget);
        }
        return hitTarget.Add(targetOwner);
    }

    public bool HasConfirmedHit(CombatActionDefinition action)
    {
        if (action == null) return false;
        if (hitTargetsByAction.TryGetValue(action,out var hitTarget) == false) return false;
        if (hitTarget.Count <= 0) return false;
        return true;
    }

    public void Clear()
    {
        hitTargetsByAction.Clear();
    }
}
