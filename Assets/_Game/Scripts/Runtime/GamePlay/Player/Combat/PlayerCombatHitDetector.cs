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


    private void Awake()
    {
        if (actionController == null)
            actionController = GetComponent<PlayerCombatActionController>();
        IsOnGamePlaye = true;
    }

    private void FixedUpdate()
    {
        if (actionController == null || hitboxRoot == null) return;
        if (actionController.IsHitWindowOpen == false) return;
        CombatActionDefinition currentAction = actionController.CurrentAction;
        if (currentAction == null) return;
        if (currentAction.Stage.TryGetHitWindow(actionController.ActiveHitWindowId, out var hitWindow) == false) return;
        if (hitWindow == null) return;
        Collider2D[] targets = DetectTargets(hitWindow);
        foreach(Collider2D target in targets)
        {
            Debug.Log($"Action : {actionController.CurrentAction.Stage.AnimatorStateName} Hit {target.gameObject.name} in WindowId {actionController.ActiveHitWindowId}");
        }
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
