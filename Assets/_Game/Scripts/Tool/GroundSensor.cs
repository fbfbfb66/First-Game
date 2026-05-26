using UnityEngine;

public class GroundSensor : MonoBehaviour
{
    [SerializeField] private Transform point;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float distance;

    public bool IsGrounded {get;private set;} = true;

    public void UpdateGroundState()
    {
        RaycastHit2D hit = Physics2D.Raycast(point.position,Vector2.down,distance,whatIsGround);
        IsGrounded = hit.collider != null;
    }

    private void OnDrawGizmos()
    {
        if(point == null) return;
        Gizmos.color = Color.red;
        Vector2 from = point.position;
        Vector2 to = from + Vector2.down * distance;
        Gizmos.DrawLine(from,to);
    }
}
