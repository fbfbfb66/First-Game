using System;
using UnityEngine;

public readonly struct AIIntent
{
    public AIIntentKey Key { get; }
    public Transform Target { get; }

    public AIIntent(AIIntentKey key, Transform target)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        Key = key;
        Target = target;
    }
}
