using UnityEngine;

[DefaultExecutionOrder(100)]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class AIDisplacementMotor : MonoBehaviour
{
    [SerializeField] private Transform facingRoot;
    private Rigidbody2D body;
    private readonly DisplacementProgress displacement = new();
    private Object activeSource;
    private bool hitReaction;
    private bool paused;
    private bool hasLocomotion;
    private Vector2 locomotionTarget;
    public bool IsDisplacementActive => displacement.IsRunning;
    public bool IsHitReactionActive => IsDisplacementActive && hitReaction;
    public float FacingDirection => facingRoot != null && facingRoot.localScale.x < 0f ? -1f : 1f;

    private void Awake() => body = GetComponent<Rigidbody2D>();

    public bool TryStartDisplacement(DisplacementRule rule, float horizontalDirection, Object source, bool isHitReaction = false)
    {
        if (!float.IsFinite(horizontalDirection)) return false;
        return TryStartDisplacement(rule, horizontalDirection < 0f ? Vector2.left : Vector2.right, source, isHitReaction);
    }

    public bool TryStartDisplacement(DisplacementRule rule, Vector2 direction, Object source, bool isHitReaction = false)
    {
        if (!isActiveAndEnabled || body == null || source == null || rule == null) return false;
        if (!rule.IsValid || !float.IsFinite(direction.x) || !float.IsFinite(direction.y))
        {
            Debug.LogWarning($"[AIMovement] {name}: displacement needs finite direction/distance, positive duration and a curve from 0 to 1; request ignored.", this);
            return false;
        }
        if (rule.Distance == Vector2.zero) return false;
        if (IsDisplacementActive && hitReaction && !isHitReaction) return false;
        if (!displacement.TryStart(rule, direction)) return false;
        activeSource = source;
        hitReaction = isHitReaction;
        hasLocomotion = false;
        return true;
    }

    public void StopDisplacement(Object source)
    {
        if (activeSource != source) return;
        ClearDisplacement();
    }

    public void SetPaused(bool value) => paused = value;

    public void RequestLocomotion(Vector2 position)
    {
        if (!isActiveAndEnabled || paused || IsDisplacementActive) return;
        locomotionTarget = position;
        hasLocomotion = true;
    }

    private void FixedUpdate()
    {
        if (body == null) return;
        Vector2 next = body.position;
        if (!paused && IsDisplacementActive)
        {
            if (activeSource == null || !displacement.TryAdvance(WorldTime.fixedDeltaTime, out var delta))
                ClearDisplacement();
            else next += delta;
        }
        else if (!paused && hasLocomotion) next = locomotionTarget;
        hasLocomotion = false;
        // All AI movement has one writer, including holding position during hit stop.
        body.MovePosition(next);
    }

    private void ClearDisplacement()
    {
        displacement.Clear();
        activeSource = null;
        hitReaction = false;
    }

    private void OnDisable()
    {
        ClearDisplacement();
        paused = hasLocomotion = false;
        if (body != null && body.gameObject.activeInHierarchy && body.simulated) body.MovePosition(body.position);
    }
}
