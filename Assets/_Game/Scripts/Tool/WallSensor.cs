using UnityEngine;

public class WallSensor : MonoBehaviour
{
    [SerializeField] private Transform point;
    [SerializeField] private LayerMask whatIsWall;
    [SerializeField] private float distance;

    public bool IsTouchingWall {get;private set;}
    public int WallDirection {get;private set;} = 1;

    public void UpdateWallState(bool facingRight)
    {
        WallDirection = facingRight ? 1 : -1;
        Vector2 direction = Vector2.right * WallDirection;
        RaycastHit2D hit = Physics2D.Raycast(point.position,direction,distance,whatIsWall);
        IsTouchingWall = hit.collider != null;
    }

    private void OnDrawGizmos()
    {
        if(point == null) return;
        int gizmoDirection = WallDirection == 0 ? 1 : WallDirection;
        Gizmos.color = Color.red;
        Vector2 from = point.position;
        Vector2 to = from + Vector2.right * distance * gizmoDirection;
        Gizmos.DrawLine(from,to);
    }
}
