using System;
using UnityEngine;

public readonly struct AIIntent
{
    public AIIntentKey Key { get; }
    public Transform Target { get; }
    public ActionDefinition Action { get; }

    public AIIntent(AIIntentKey key, Transform target, ActionDefinition action=null)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        Key = key;
        Target = target;
        Action = action;
    }
}
