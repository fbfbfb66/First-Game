using UnityEngine;

public class WallSensor : MonoBehaviour
{
    [SerializeField] private Transform point;
    [SerializeField] private Transform ledgeCheckPoint;
    [SerializeField] private LayerMask whatIsWall;
    [SerializeField] private float bodyDistance;
    [SerializeField] private float ledgeCheckDistance;
    [SerializeField] private float ledgeVerticalSearchDistance = 0.5f;

    public bool ReachedLedgeThisFrame { get; private set; }
    private bool isLedgePointTouchingWall;
    private bool wasTouchingWall;
    private bool wasLedgePointTouchingWall;

    public float HangBodyTargetY { get; private set; }
    public float LedgeVerticalCorrection { get; private set; }
    public bool IsTouchingWall { get; private set; }
    public int WallDirection { get; private set; } = 1;

    public void UpdateWallState(bool facingRight)
    {
        ReachedLedgeThisFrame = false;
        LedgeVerticalCorrection = 0f;

        WallDirection = facingRight ? 1 : -1;
        Vector2 direction = Vector2.right * WallDirection;
        RaycastHit2D hit = Physics2D.Raycast(point.position, direction, bodyDistance, whatIsWall);
        RaycastHit2D hit2 = Physics2D.Raycast(ledgeCheckPoint.position, direction, ledgeCheckDistance, whatIsWall);
        IsTouchingWall = hit.collider != null;
        isLedgePointTouchingWall = hit2.collider != null;
        bool reachedLedgeCandidate =
            isLedgePointTouchingWall != wasLedgePointTouchingWall &&
            IsTouchingWall &&
            wasTouchingWall;

        if (reachedLedgeCandidate)
        {
            Vector2 ledgeRayEnd = (Vector2)ledgeCheckPoint.position + direction * ledgeCheckDistance;
            if (TryFindLedgeTop(ledgeRayEnd, out float ledgeY))
            {
                LedgeVerticalCorrection = ledgeY - ledgeCheckPoint.position.y;
                HangBodyTargetY = transform.root.position.y + LedgeVerticalCorrection;
                ReachedLedgeThisFrame = true;
            }
        }

        wasLedgePointTouchingWall = isLedgePointTouchingWall;
        wasTouchingWall = IsTouchingWall;
    }

    private bool TryFindLedgeTop(Vector2 ledgeRayEnd, out float ledgeY)
    {
        ledgeY = ledgeRayEnd.y;
        Vector2 originalPos = ledgeRayEnd + Vector2.up * ledgeVerticalSearchDistance;
        RaycastHit2D topHit = Physics2D.Raycast(originalPos, Vector2.down, ledgeVerticalSearchDistance * 2, whatIsWall);
        if (topHit.collider == null)
        {
            return false;
        }
        ledgeY = topHit.point.y;
        return true;
    }

    private void OnDrawGizmos()
    {
        if (point == null) return;
        int gizmoDirection = WallDirection == 0 ? 1 : WallDirection;
        Gizmos.color = Color.red;
        Vector2 from = point.position;
        Vector2 from2 = ledgeCheckPoint.position;
        Vector2 ledgeRayEnd = (Vector2)ledgeCheckPoint.position + Vector2.right * gizmoDirection * ledgeCheckDistance;
        Vector2 originalPos = ledgeRayEnd + Vector2.up * ledgeVerticalSearchDistance;
        Vector2 from3 = originalPos;
        Vector2 to = from + Vector2.right * bodyDistance * gizmoDirection;
        Vector2 to2 = from2 + Vector2.right * ledgeCheckDistance * gizmoDirection;
        Vector2 to3 = from3 + Vector2.down * ledgeVerticalSearchDistance * 2;
        Gizmos.DrawLine(from, to);
        Gizmos.DrawLine(from2, to2);
        Gizmos.DrawLine(from3, to3);
    }
}
