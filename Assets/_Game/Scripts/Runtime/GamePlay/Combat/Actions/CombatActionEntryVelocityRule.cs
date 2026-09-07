using System;
using UnityEngine;
[Serializable]
public sealed class CombatActionEntryVelocityRule
{
    [SerializeField] private bool overrideHorizontal;
    [SerializeField] private float localHorizontalVelocity;
    [SerializeField] private bool overrideVertical;
    [SerializeField] private float verticalVelocity;

    public Vector2 Resolve(Vector2 currentVelocity,bool facingRight)
    {
        float finalX = currentVelocity.x;
        float finalY = currentVelocity.y;
        if (overrideHorizontal) finalX = facingRight ? localHorizontalVelocity : -localHorizontalVelocity;
        if (overrideVertical) finalY = verticalVelocity;
        return new Vector2(finalX,finalY);
    }
}
