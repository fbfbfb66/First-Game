using System;
using UnityEngine;
[Serializable]
public sealed class CombatHitWindowDefinition
{
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
}
public enum CombatHitShape
{
    Box,
    Circle
}
