using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "GameLayerRuleDatabase",menuName = "Game/Core/Game Layer Rule Database")]
public class GameLayerRuleDatabase : ScriptableObject
{
    [SerializeField] private List<GameLayerRule> layerRules = new();

    public PlayerActionType GetLockedPlayerActions(GameLayerType layer)
    {
        foreach(var rule in layerRules)
        {
            if(rule.layerType == layer)
            {
                return rule.lockedPlayerActions;
            }
        }
        Debug.LogWarning($"Not found {layer} in rules");
        return PlayerActionType.None;
    }
}
