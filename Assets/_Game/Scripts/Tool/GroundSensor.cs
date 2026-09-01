using UnityEngine;

public class GroundSensor : MonoBehaviour
{
    [SerializeField] private Transform point;
    [SerializeField] private Transform point2;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float distance;

    public bool IsGrounded {get;private set;} = true;

    public void UpdateGroundState()
    {
        RaycastHit2D hit = Physics2D.Raycast(point.position,Vector2.down,distance,whatIsGround);
        RaycastHit2D hit2 = Physics2D.Raycast(point2.position, Vector2.down, distance, whatIsGround);
        IsGrounded = hit.collider != null || hit2.collider != null;
    }

    private void OnDrawGizmos()
    {
        if(point == null) return;
        Gizmos.color = Color.red;
        Vector2 from = point.position;
        Vector2 from2 = point2.position;
        Vector2 to = from + Vector2.down * distance;
        Vector2 to2 = from2 + Vector2.down * distance;
        Gizmos.DrawLine(from,to);
        Gizmos.DrawLine(from2, to2);
    }
}
