using UnityEngine;

public sealed class PlayerControlArbitration
{
    private readonly GameLayerStack layerStack;
    private readonly GameLayerRuleDatabase gameLayerRuleDatabase;

    public bool CanMove => CanDo(PlayerActionType.Move);
    public bool CanJump => CanDo(PlayerActionType.Jump);
    public bool CanAttack => CanDo(PlayerActionType.Attack);
    public bool CanDash => CanDo(PlayerActionType.Dash);
    public bool CanWorldInteract => CanDo(PlayerActionType.WorldInteract);
    public bool CanUseItem => CanDo(PlayerActionType.UseItem);

    public PlayerControlArbitration(GameLayerStack layerStack, GameLayerRuleDatabase gameLayerRuleDatabase)
    {
        this.layerStack = layerStack != null ? layerStack : throw new System.ArgumentNullException(nameof(layerStack));
        this.gameLayerRuleDatabase = gameLayerRuleDatabase != null ? gameLayerRuleDatabase : throw new System.ArgumentNullException(nameof(gameLayerRuleDatabase));
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
            Debug.LogWarning("GameLayerRuleDatabase is missing. Player action locked");
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
