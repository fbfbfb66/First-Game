using System;
using UnityEngine;
[Serializable]
public sealed class CombatHitWindowDefinition
{
    [SerializeField] private CombatHitEffect[] effects;
    [SerializeField] private int windowId;
    [SerializeField] private CombatHitShape shape;
    [SerializeField] private Vector2 localOffset;
    [SerializeField] private Vector2 boxSize;
    [SerializeField] private float circleRadius;

    public int WindowId => windowId;
    public CombatHitShape Shape => shape;
    public Vector2 LocalOffset => localOffset;
    public Vector2 BoxSize => boxSize;
    public float CircleRadius => circleRadius;

    public void ApplyEffects(in CombatHitEffectContext context)
    {
        if (effects == null || effects.Length <= 0) return;
        foreach (var effect in effects)
        {
            if (effect == null) continue;
            effect.Apply(context);
        }
    }

    public void EndEffects(in CombatHitEffectContext context)
    {
        if (effects == null || effects.Length <= 0) return;
        foreach (var effect in effects)
        {
            if (effect == null) continue;
            effect.End(context);
        }
    }
}
public enum CombatHitShape
{
    Box,
    Circle
}
