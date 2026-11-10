using UnityEngine;
[RequireComponent(typeof(Rigidbody2D), typeof(AIDisplacementMotor))]
public abstract class AIMotor : MonoBehaviour
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

    protected virtual void Awake()
    {
        displacementMotor = GetComponent<AIDisplacementMotor>();
        if (body == null)
            body = GetComponent<Rigidbody2D>();
        if (effectReceiver == null)
            effectReceiver = GetComponent<CombatEffectReceiver>();
    }

    protected Rigidbody2D Body => body;
    protected AIDisplacementMotor DisplacementMotor => displacementMotor;
    protected bool BlocksLocomotion => effectReceiver != null && effectReceiver.BlocksLocomotion;

    public abstract void MoveTowards(Vector2 target, float speed, float stoppingDistance, float deltaTime);

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

    protected virtual void OnDisable()
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
