using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatHitDetector : MonoBehaviour
{
    [SerializeField] private CombatHitShape shape;
    [SerializeField] private Vector2 localOffset = new Vector2(.5f, .5f);
    [SerializeField] private Vector2 boxSize = new Vector2(1, 1);
    [SerializeField] private float circleRadius = 1;
    [Space]
    [SerializeField] private PlayerCombatActionController actionController;
    [SerializeField] private Transform hitboxRoot;
    [SerializeField] private LayerMask targetLayers;

    private bool IsOnGamePlaye = false;
    private Player player;
    private readonly HashSet<GameObject> hitOwners = new();
    private CombatActionDefinition trackedAction;
    private CombatHitWindowDefinition trackedHitWindow;
    private readonly List<CombatHitEffectContext> appliedEffectContexts = new();


    private void Awake()
    {
        player = GetComponent<Player>();
        if (player == null)
            Debug.LogWarning($"{name} cannot deal damage: Player is missing. Hit detection disabled.", this);
        if (actionController == null)
            actionController = GetComponent<PlayerCombatActionController>();
        IsOnGamePlaye = true;
    }

    private void OnDisable()
    {
        StopTrackingHitWindow();
    }

    private void FixedUpdate()
    {
        if (player == null || player.Stats == null || actionController == null || hitboxRoot == null) return;
        if (actionController.IsHitWindowOpen == false)
        {
            StopTrackingHitWindow();
            return;
        }
        CombatActionDefinition currentAction = actionController.CurrentAction;
        if (currentAction == null) return;
        if (currentAction.Stage.TryGetHitWindow(actionController.ActiveHitWindowId, out var hitWindow) == false) return;
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
            DamageResult damage = GameplayCalculator.CalculateOutgoingDamage(player.Stats, hitWindow.DamageMultiplier);
            DamageContext damageContext = new DamageContext(player.gameObject, hurtbox.Owner, damage);
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
                return Physics2D.OverlapBoxAll(center, hitWindow.BoxSize, 0f, targetLayers);
            case CombatHitShape.Circle:
                return Physics2D.OverlapCircleAll(center, hitWindow.CircleRadius, targetLayers);
        }
        return null;
    }

    private void OnDrawGizmos()
    {
        if(hitboxRoot == null || actionController == null) return;
        Gizmos.color = actionController.IsHitWindowOpen ? Color.red : Color.green;
        if(IsOnGamePlaye == false)
        {
            Vector2 center = hitboxRoot.TransformPoint(localOffset);
            switch (shape)
            {
                case CombatHitShape.Box:
                    Gizmos.DrawWireCube(center, boxSize);
                    break;
                case CombatHitShape.Circle:
                    Gizmos.DrawWireSphere(center, circleRadius);
                    break;
            }

        }

        else
        {
            if (actionController.IsHitWindowOpen == false) return;
            CombatActionDefinition currentAction = actionController.CurrentAction;
            if (currentAction == null) return;
            if (currentAction.Stage.TryGetHitWindow(actionController.ActiveHitWindowId, out var hitWindow) == false) return;
            if (hitWindow == null) return;
            Vector2 center = hitboxRoot.TransformPoint(hitWindow.LocalOffset);

            switch (hitWindow.Shape)
            {
                case CombatHitShape.Box:
                    Gizmos.DrawCube(center, hitWindow.BoxSize);
                    return;
                case CombatHitShape.Circle:
                    Gizmos.DrawSphere(center, hitWindow.CircleRadius);
                    return;
            }
        }
    }
}
