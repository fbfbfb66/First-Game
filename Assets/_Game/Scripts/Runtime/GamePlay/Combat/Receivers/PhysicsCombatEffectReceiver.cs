using UnityEngine;

[DefaultExecutionOrder(50)]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class PhysicsCombatEffectReceiver : CombatEffectReceiver
{
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private bool canBeKnockedBack = true;
    [SerializeField] private bool canBeLaunched = true;
    private readonly DisplacementProgress knockback = new();
    private bool controlsX;
    private bool controlsY;
    private float gravityBeforeKnockback;
    private bool isMotionPaused;
    private Vector2 velocityBeforePause;
    private float gravityScaleBeforePause;
    public override bool CanBeKnockedBack => canBeKnockedBack;
    public override void SetCanBeKnockedBack(bool value) => canBeKnockedBack = value;
    public override bool CanBeLaunched => canBeLaunched;
    public override void SetCanBeLaunched(bool value) => canBeLaunched = value;
    public override bool BlocksLocomotion => knockback.IsRunning || isMotionPaused;

    private void Awake()
    {
        if (body == null) body = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (body == null) return;
        if (isMotionPaused) { body.linearVelocity = Vector2.zero; return; }
        if (!knockback.IsRunning) return;
        float dt = WorldTime.fixedDeltaTime;
        if (dt <= 0f) { StopControlledVelocity(); return; }
        if (!knockback.TryAdvance(dt, out var delta)) { EndKnockback(); return; }
        Vector2 velocity = body.linearVelocity;
        // Dynamic physics uses real fixed seconds; progress follows WorldTime.
        if (controlsX) velocity.x = delta.x / Time.fixedDeltaTime;
        if (controlsY) velocity.y = delta.y / Time.fixedDeltaTime;
        body.linearVelocity = velocity;
    }

    public override bool TryApplyKnockback(DisplacementRule rule, float horizontalDirection = 1f)
    {
        if (!isActiveAndEnabled || body == null || !canBeKnockedBack || rule == null ||
            !rule.IsValid || rule.Distance == Vector2.zero || !float.IsFinite(horizontalDirection)) return false;
        EndKnockback();
        if (!knockback.TryStart(rule, horizontalDirection)) return false;
        // Knockback owns horizontal motion, including zero-X downward slams.
        controlsX = true;
        controlsY = knockback.ControlsVertical;
        if (controlsY)
        {
            gravityBeforeKnockback = isMotionPaused ? gravityScaleBeforePause : body.gravityScale;
            body.gravityScale = 0f;
        }
        return true;
    }

    public override bool TryBeginMotionPause()
    {
        if (!isActiveAndEnabled || body == null || isMotionPaused) return false;
        velocityBeforePause = body.linearVelocity;
        gravityScaleBeforePause = controlsY ? gravityBeforeKnockback : body.gravityScale;
        body.linearVelocity = Vector2.zero;
        body.gravityScale = 0f;
        isMotionPaused = true;
        return true;
    }

    public override void EndMotionPause()
    {
        if (!isMotionPaused || body == null) return;
        isMotionPaused = false;
        body.gravityScale = controlsY ? 0f : gravityScaleBeforePause;
        body.linearVelocity = velocityBeforePause;
        if (knockback.IsRunning) StopControlledVelocity();
    }

    public override bool TryLaunch(float verticalVelocity, DisplacementRule aiDisplacement = null)
    {
        if (!isActiveAndEnabled || !canBeLaunched || body == null) return false;
        EndKnockback();
        Vector2 velocity = body.linearVelocity;
        velocity.y = verticalVelocity;
        body.linearVelocity = velocity;
        return true;
    }

    private void StopControlledVelocity()
    {
        if (body == null) return;
        Vector2 velocity = body.linearVelocity;
        if (controlsX) velocity.x = 0f;
        if (controlsY) velocity.y = 0f;
        body.linearVelocity = velocity;
    }

    private void EndKnockback()
    {
        if (!knockback.IsRunning) return;
        StopControlledVelocity();
        if (controlsY && body != null && !isMotionPaused) body.gravityScale = gravityBeforeKnockback;
        knockback.Clear();
        controlsX = controlsY = false;
    }

    private void OnDisable()
    {
        EndMotionPause();
        EndKnockback();
    }
}
