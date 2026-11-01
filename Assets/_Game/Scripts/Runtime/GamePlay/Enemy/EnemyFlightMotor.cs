using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyFlightMotor : MonoBehaviour
{
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private Transform visual;
    [SerializeField] private CombatEffectReceiver effectReceiver;
    private bool facingRight = true;
    public Vector2 Position => body.position;
    public bool FacingRight => facingRight;

    private void Awake()
    {
        if (body == null)
            body = GetComponent<Rigidbody2D>();
        if (effectReceiver == null)
            effectReceiver = GetComponent<CombatEffectReceiver>();
    }

    public void MoveTowards(Vector2 target,float speed,float stoppingDistance,float deltaTime)
    {
        if (body == null || !isActiveAndEnabled || deltaTime <= 0 || effectReceiver.BlocksLocomotion) return;
        float distance = Vector2.Distance(Position, target);
        Vector2 dir = (target - Position).normalized;
        if (distance <= stoppingDistance)
        {
            return;
        }
        HandleFlip(dir);
        float remainingDistance = distance - stoppingDistance;
        float step = Mathf.Min(speed * deltaTime, remainingDistance);
        Vector2 nextPos = step * dir + Position;
        body.MovePosition(nextPos);
    }

    public void HandleFlip(Vector2 dir)
    {
        if (dir.x > 0 && !facingRight)
        {
            Flip();
        }
        else if (dir.x < 0 && facingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = visual.localScale;
        scale.x *= -1;
        visual.localScale = scale;
    }
}
