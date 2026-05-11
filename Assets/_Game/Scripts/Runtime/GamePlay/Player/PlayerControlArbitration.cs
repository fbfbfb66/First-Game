using UnityEngine;

public class PlayerControlArbitration : MonoBehaviour
{
    [SerializeField] private GameLayerStack layerStack;
    [SerializeField] private GameLayerRuleDatabase gameLayerRuleDatabase;

    public bool CanMove => CanDo(PlayerActionType.Move);
    public bool CanJump => CanDo(PlayerActionType.Jump);
    public bool CanAttack => CanDo(PlayerActionType.Attack);
    public bool CanDash => CanDo(PlayerActionType.Dash);
    public bool CanWorldInteract => CanDo(PlayerActionType.WorldInteract);
    public bool CanUseItem => CanDo(PlayerActionType.UseItem);

    private void Awake()
    {
        layerStack = FindAnyObjectByType<GameLayerStack>();
    }
    private bool CanDo(PlayerActionType actionType)
    {
        if(layerStack == null)
        {
            Debug.LogWarning("GameLayerStack is missing. Player action locked");
            return false;
        }
        if(gameLayerRuleDatabase == null)
        {
            Debug.LogWarning("GameLayerStack is missing. Player action locked");
            return false;           
        }
        PlayerActionType lockedActions = GetAllPlayerActions();
        return !lockedActions.HasFlag(actionType);
    }

    private PlayerActionType GetAllPlayerActions()
    {
        GameLayerType[] activeLayers = layerStack.GetActiveLayers();
        PlayerActionType lockedActions = PlayerActionType.None;
        foreach(var layer in activeLayers)
        {
            PlayerActionType actionType = gameLayerRuleDatabase.GetLockedPlayerActions(layer);
            lockedActions |= actionType;
        }
        return lockedActions;        
    }
}
