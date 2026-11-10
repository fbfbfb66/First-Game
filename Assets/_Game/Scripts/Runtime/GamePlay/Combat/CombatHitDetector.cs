using System.Collections.Generic;
using UnityEngine;

public abstract class CombatHitDetector : MonoBehaviour
{
    [Header("Gizmo Preview")]
    [SerializeField] private CombatActionDefinition previewAction;
    [SerializeField, Min(0)] private int previewStageIndex;
    [SerializeField] private int previewHitWindowId;
    protected abstract CombatActionController ActionController { get; }
    protected abstract CharacterStats AttackerStats { get; }
    protected abstract void ResolveOwner();
    private CombatActionController actionController => ActionController;
    private CombatActionController subscribedController;
    [SerializeField] private Transform hitboxRoot;
    [SerializeField] private LayerMask targetLayers;

    private readonly HashSet<GameObject> hitOwners = new();
    private CombatActionDefinition trackedAction;
    private CombatHitWindowDefinition trackedHitWindow;
    private readonly List<CombatHitEffectContext> appliedEffectContexts = new();


    protected virtual void Awake()
    {
        ResolveOwner();
        if (actionController == null)
            Debug.LogWarning($"{name} cannot deal damage: combat owner is missing. Hit detection disabled.", this);
    }

    protected virtual void OnEnable()
    {
        subscribedController = actionController;
        if (subscribedController != null)
            subscribedController.HitWindowEnded += StopTrackingHitWindow;
    }

    protected virtual void OnDisable()
    {
        if (subscribedController != null)
            subscribedController.HitWindowEnded -= StopTrackingHitWindow;
        subscribedController = null;
        StopTrackingHitWindow();
    }

    protected virtual void FixedUpdate()
    {
        if (AttackerStats == null || actionController == null || hitboxRoot == null) return;
        if (actionController.IsHitWindowOpen == false)
        {
            StopTrackingHitWindow();
            return;
        }
        CombatActionDefinition currentAction = actionController.CurrentAction;
        if (currentAction == null) return;
        if (actionController.CurrentCombatStage.TryGetHitWindow(actionController.ActiveHitWindowId, out var hitWindow) == false) return;
        if(currentAction != trackedAction || hitWindow != trackedHitWindow)
        {
            BeginTrackingHitWindow(currentAction,hitWindow);
        }
        if (hitWindow == null) return;
        Collider2D[] targets = DetectTargets(hitWindow);
        foreach(Collider2D target in targets)
        {
            if (TryRegisterTarget(target, out var hurtbox) == false) continue;
            if (!hurtbox.Owner.TryGetComponent<IDamageable>(out var damageable)) continue;
            DamageResult damage = GameplayCalculator.CalculateOutgoingDamage(AttackerStats, hitWindow.DamageMultiplier);
            DamageContext damageContext = new DamageContext(gameObject, hurtbox.Owner, damage, hitWindow.AttackType);
            if (!damageable.TryTakeDamage(damageContext)) continue;
            actionController.NotifyHitConfirmed(hurtbox.Owner);
            CombatHitEffectContext context = new CombatHitEffectContext(transform.gameObject, hurtbox.Owner,hitWindow);
            hitWindow.ApplyEffects(context);
            appliedEffectContexts.Add(context);
        }
    }

    private void StopTrackingHitWindow()
    {
        hitOwners.Clear();
        trackedAction = null;
        if (trackedHitWindow != null)
        {
            foreach (var context in appliedEffectContexts)
            {
                trackedHitWindow.EndEffects(context);
            }

            trackedHitWindow = null;
        }

        appliedEffectContexts.Clear();
    }

    private void BeginTrackingHitWindow(CombatActionDefinition aciton,CombatHitWindowDefinition hitWindow)
    {
        StopTrackingHitWindow();
        trackedAction = aciton;
        trackedHitWindow = hitWindow;
    }

    private bool TryRegisterTarget(Collider2D candidate,out CombatHurtbox hurtbox)
    {
        hurtbox = null;
        if(candidate == null) return false;
        if (candidate.TryGetComponent(out hurtbox) == false) return false;
        GameObject target = hurtbox.Owner;
        if(target == null || target == gameObject) return false;
        return hitOwners.Add(target);
    }

    private Collider2D[] DetectTargets(CombatHitWindowDefinition hitWindow)
    {
        Vector2 center = hitboxRoot.TransformPoint(hitWindow.LocalOffset);
        switch (hitWindow.Shape)
        {
            case CombatHitShape.Box:
                return Physics2D.OverlapBoxAll(center, hitWindow.BoxSize, hitboxRoot.eulerAngles.z, targetLayers);
            case CombatHitShape.Circle:
                return Physics2D.OverlapCircleAll(center, hitWindow.CircleRadius, targetLayers);
        }
        return System.Array.Empty<Collider2D>();
    }

    protected virtual void OnDrawGizmos()
    {
        if (hitboxRoot == null) return;

        if (Application.isPlaying && actionController != null && actionController.IsHitWindowOpen)
        {
            var stage = actionController.CurrentCombatStage;
            if (stage != null && stage.TryGetHitWindow(actionController.ActiveHitWindowId, out var activeWindow))
                DrawHitWindowGizmo(activeWindow, Color.red);
            return;
        }

        var stages = previewAction != null ? previewAction.CombatStages : null;
        if (stages == null || previewStageIndex < 0 || previewStageIndex >= stages.Length) return;
        var previewStage = stages[previewStageIndex];
        if (previewStage != null && previewStage.TryGetHitWindow(previewHitWindowId, out var previewWindow))
            DrawHitWindowGizmo(previewWindow, Color.green);
    }

    private void DrawHitWindowGizmo(CombatHitWindowDefinition hitWindow, Color color)
    {
        var previousColor = Gizmos.color;
        Gizmos.color = color;
        Vector2 center = hitboxRoot.TransformPoint(hitWindow.LocalOffset);
        // Match Physics2D queries: world-axis box and unscaled world-space size/radius.
        switch (hitWindow.Shape)
        {
            case CombatHitShape.Box:
                var previousMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(center, hitboxRoot.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, hitWindow.BoxSize);
                Gizmos.matrix = previousMatrix;
                break;
            case CombatHitShape.Circle:
                Gizmos.DrawWireSphere(center, hitWindow.CircleRadius);
                break;
        }
        Gizmos.color = previousColor;
    }
}
