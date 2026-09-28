using UnityEngine;

public sealed class CombatActionGroundClearanceGizmo : MonoBehaviour
{
    [SerializeField] private CombatActionGroundClearanceCondition condition;

    private void OnDrawGizmos()
    {
        if (condition == null) return;

        Vector2 origin = transform.position;
        bool hasMinimumClearance = condition.HasMinimumClearance(origin, out RaycastHit2D hit);

        Gizmos.color = hasMinimumClearance ? Color.green : Color.red;
        Gizmos.DrawLine(origin, origin + Vector2.down * condition.MinimumClearance);

        if (hit.collider != null)
        {
            Gizmos.DrawWireSphere(hit.point, 0.12f);
        }
    }
}
