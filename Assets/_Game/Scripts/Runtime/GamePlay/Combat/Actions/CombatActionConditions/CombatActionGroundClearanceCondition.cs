using UnityEngine;

[CreateAssetMenu(
    fileName = "Condition_GroundClearance_",
    menuName = "Game/Combat/Conditions/Ground Clearance")]
public sealed class CombatActionGroundClearanceCondition : CombatActionCondition
{
    [SerializeField, Min(0f)] private float minimumClearance = 5f;
    [SerializeField] private LayerMask groundLayers;

    public float MinimumClearance => minimumClearance;
    public LayerMask GroundLayers => groundLayers;

    public override bool IsMet(in CombatActionContext context)
    {
        return HasMinimumClearance(context.GroundProbePosition, out _);
    }

    public bool HasMinimumClearance(Vector2 origin, out RaycastHit2D hit)
    {
        if (minimumClearance <= 0f)
        {
            hit = default;
            return true;
        }

        hit = Physics2D.Raycast(
            origin,
            Vector2.down,
            minimumClearance,
            groundLayers);

        return hit.collider == null;
    }
}
