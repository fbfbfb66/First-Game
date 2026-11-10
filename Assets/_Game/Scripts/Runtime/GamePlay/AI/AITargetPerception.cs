using System;
using System.Collections.Generic;
using UnityEngine;

public class AITargetPerception : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private AIMotor motor;
    [SerializeField] private LayerMask targetLayers = 1 << 6;

    [SerializeField, Min(0f)] private float viewDistance = 10f;

    public float ViewDistance => viewDistance;
    public LayerMask TargetLayers => targetLayers;
    public GameObject DetectedTarget { get; private set; }
    public event Action<GameObject> TargetDetected;
    private readonly List<Collider2D> candidates = new();
    private readonly List<RaycastHit2D> rayHits = new();
    private Rigidbody2D ownerBody;

    private void Awake()
    {
        if (health == null)
            health = GetComponent<Health>();
        if (motor == null)
            motor = GetComponent<AIMotor>();
        ownerBody = GetComponentInParent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (DetectedTarget == null) TryDetectTarget();
    }

    public bool TryDetectTarget()
    {
        if (!isActiveAndEnabled || DetectedTarget != null || motor == null) return false;
        if (!float.IsFinite(viewDistance) || viewDistance <= 0f) return false;

        Vector2 origin = transform.position;
        PhysicsScene2D physicsScene = gameObject.scene.GetPhysicsScene2D();
        var candidateFilter = new ContactFilter2D { useTriggers = true };
        candidateFilter.SetLayerMask(targetLayers);
        candidates.Clear();
        physicsScene.OverlapCircle(origin, viewDistance, candidateFilter, candidates);

        GameObject visibleTarget = null;
        float nearestDistance = float.PositiveInfinity;
        foreach (Collider2D candidate in candidates)
        {
            if (candidate == null || IsSelf(candidate)) continue;
            Vector2 point = candidate.bounds.center;
            Vector2 offset = point - origin;
            bool facingRight = offset.x >= 0f;
            if (!Mathf.Approximately(offset.x, 0f) && motor.FacingRight != facingRight) continue;
            if (offset.sqrMagnitude <= Mathf.Epsilon || offset.magnitude > viewDistance) continue;

            // All layers participate in visibility: filtering this ray to targetLayers would see through walls.
            var sightFilter = new ContactFilter2D { useTriggers = true, useLayerMask = false };
            rayHits.Clear();
            physicsScene.Raycast(origin, offset.normalized, viewDistance, sightFilter, rayHits);
            Collider2D first = null;
            float firstDistance = float.PositiveInfinity;
            foreach (RaycastHit2D hit in rayHits)
            {
                if (hit.collider == null || IsSelf(hit.collider) || hit.distance >= firstDistance) continue;
                // Interaction/sensor triggers are not solid occluders; target-layer triggers remain detectable.
                if (hit.collider.isTrigger && (targetLayers.value & (1 << hit.collider.gameObject.layer)) == 0) continue;
                first = hit.collider;
                firstDistance = hit.distance;
            }
            if (first == null || (targetLayers.value & (1 << first.gameObject.layer)) == 0 ||
                firstDistance >= nearestDistance) continue;

            nearestDistance = firstDistance;
            visibleTarget = first.attachedRigidbody != null
                ? first.attachedRigidbody.gameObject : first.gameObject;
        }
        if (visibleTarget == null) return false;
        DetectedTarget = visibleTarget;
        TargetDetected?.Invoke(visibleTarget);
        return true;
    }

    private bool IsSelf(Collider2D collider) =>
        collider.transform == transform || collider.transform.IsChildOf(transform) ||
        (ownerBody != null && collider.attachedRigidbody == ownerBody);

    private void OnEnable()
    {
        if (health != null) health.Damaged += SetDetectedTarget;
    }

    private void OnDisable()
    {
        ClearTarget();
        if (health != null) health.Damaged -= SetDetectedTarget;
    }

    public void ClearTarget()
    {
        DetectedTarget = null;
    }

    private void SetDetectedTarget(DamageContext context)
    {
        DetectedTarget = context.Attacker;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, viewDistance);
    }
}
