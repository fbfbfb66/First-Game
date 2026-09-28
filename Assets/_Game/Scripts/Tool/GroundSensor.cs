using UnityEngine;

public class GroundSensor : MonoBehaviour
{
    [SerializeField] private Transform point;
    [SerializeField] private Transform pointCenter;
    [SerializeField] private Transform point2;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float distance;
    private float lastGroundedTime = float.NegativeInfinity;

    public bool IsGrounded {get;private set;} = true;
    public bool CanEnterGrounded { get; private set; }

    public void UpdateGroundState()
    {
        RaycastHit2D leftHit = Physics2D.Raycast(point.position, Vector2.down, distance, whatIsGround);
        RaycastHit2D centerHit = Physics2D.Raycast(pointCenter.position, Vector2.down, distance, whatIsGround);
        RaycastHit2D rightHit = Physics2D.Raycast(point2.position, Vector2.down, distance, whatIsGround);

        int groundedProbeCount = 0;
        if (leftHit.collider != null) groundedProbeCount++;
        if (centerHit.collider != null) groundedProbeCount++;
        if (rightHit.collider != null) groundedProbeCount++;

        IsGrounded = groundedProbeCount > 0;
        CanEnterGrounded = groundedProbeCount >= 2;

        if (IsGrounded)
        {
            lastGroundedTime = Time.time;
        }
    }

    public bool WasGroundedWithin(float duration)
    {
        if(Time.time -  lastGroundedTime <= duration)
        {
            return true;
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        DrawProbeGizmo(point);
        DrawProbeGizmo(pointCenter);
        DrawProbeGizmo(point2);
    }

    private void DrawProbeGizmo(Transform probe)
    {
        if (probe == null) return;

        Vector2 origin = probe.position;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, distance, whatIsGround);
        Gizmos.color = hit.collider != null ? Color.green : Color.red;
        Gizmos.DrawLine(origin, origin + Vector2.down * distance);

        if (hit.collider != null)
        {
            Gizmos.DrawWireSphere(hit.point, 0.08f);
        }
    }
}
