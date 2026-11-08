using System;
using UnityEngine;

[Serializable]
public sealed class ActionEntryVelocityRule
{
    [SerializeField] private bool overrideHorizontal;
    [SerializeField] private float localHorizontalVelocity;
    [SerializeField] private bool overrideVertical;
    [SerializeField] private float verticalVelocity;
    [SerializeField, Min(0f)] private float inheritedHorizontalSpeedFactor;
    [Tooltip("Use signed horizontal input instead of facing. Input magnitude scales the configured horizontal speed.")]
    [SerializeField] private bool useHorizontalInput;
    [Tooltip("Alternate local X speed, used by Dash when entering against a wall. Does not inherit momentum.")]
    [SerializeField] private float alternateLocalHorizontalVelocity;

    public bool OverrideHorizontal => overrideHorizontal;

    public Vector2 Resolve(Vector2 currentVelocity, bool facingRight,
        float horizontalInput = 0f, bool useAlternateHorizontal = false)
    {
        float x = currentVelocity.x;
        if (overrideHorizontal)
        {
            float direction = useHorizontalInput ? Mathf.Sign(horizontalInput) : facingRight ? 1f : -1f;
            float speed = useAlternateHorizontal
                ? alternateLocalHorizontalVelocity
                : localHorizontalVelocity * (useHorizontalInput ? Mathf.Abs(horizontalInput) : 1f)
                    + Mathf.Abs(currentVelocity.x) * inheritedHorizontalSpeedFactor;
            x = direction * speed;
        }
        return new Vector2(x, overrideVertical ? verticalVelocity : currentVelocity.y);
    }
}
