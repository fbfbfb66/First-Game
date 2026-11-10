using UnityEngine;
[RequireComponent(typeof(Rigidbody2D), typeof(AIDisplacementMotor))]
public class EnemyFlightMotor : MonoBehaviour
{
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private Transform visual;
    [SerializeField] private CombatEffectReceiver effectReceiver;
    private AIDisplacementMotor displacementMotor;
    private bool facingRight = true;
    private Object facingLockOwner;
    private Quaternion rotationBeforeLock;
    public bool IsFacingLocked => facingLockOwner != null;
    public Vector2 Position => body.position;
    public bool FacingRight => facingRight;

    private void Awake()
    {
        displacementMotor = GetComponent<AIDisplacementMotor>();
        if (body == null)
            body = GetComponent<Rigidbody2D>();
        if (effectReceiver == null)
            effectReceiver = GetComponent<CombatEffectReceiver>();
    }

    public void MoveTowards(Vector2 target,float speed,float stoppingDistance,float deltaTime)
    {
        if (body == null || !isActiveAndEnabled || deltaTime <= 0 || (effectReceiver != null && effectReceiver.BlocksLocomotion) || displacementMotor.IsDisplacementActive) return;
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
        displacementMotor.RequestLocomotion(nextPos);
    }

    public bool IsFacingTarget(Vector2 targetPosition)
    {
        float dx = targetPosition.x - transform.position.x;
        return Mathf.Approximately(dx, 0f) || (dx > 0f) == facingRight;
    }

    public void HandleFlip(Vector2 dir)
    {
        if (IsFacingLocked) return;
        if (dir.x > 0 && !facingRight)
        {
            Flip();
        }
        else if (dir.x < 0 && facingRight)
        {
            Flip();
        }
    }

    public bool TryLockFacing(Vector2 direction, Object source)
    {
        if (source == null || visual == null || IsFacingLocked || direction.sqrMagnitude <= Mathf.Epsilon ||
            !float.IsFinite(direction.x) || !float.IsFinite(direction.y)) return false;
        HandleFlip(direction);
        rotationBeforeLock = visual.localRotation;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        // Mirrored left-facing sprites already point along local -X.
        visual.rotation = Quaternion.Euler(0f, 0f, angle - (facingRight ? 0f : 180f));
        facingLockOwner = source;
        return true;
    }

    public void ReleaseFacing(Object source)
    {
        if (facingLockOwner != source) return;
        if (visual != null) visual.localRotation = rotationBeforeLock;
        facingLockOwner = null;
    }

    private void OnDisable()
    {
        if (facingLockOwner != null) ReleaseFacing(facingLockOwner);
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = visual.localScale;
        scale.x *= -1;
        visual.localScale = scale;
    }
}
