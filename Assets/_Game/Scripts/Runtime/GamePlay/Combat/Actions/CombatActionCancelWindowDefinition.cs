using System;
using UnityEngine;
[Serializable]
public sealed class CombatActionCancelWindowDefinition
{
    [SerializeField] private int windowId;
    [SerializeField] private CombatActionCommandKey[] allowedCommands;

    public int WindowId => windowId;
    public CombatActionCommandKey[] AllowedCommands => allowedCommands;

    public bool Accepts(CombatActionCommandKey command)
    {
        if (command == null) return false;
        if(allowedCommands == null || allowedCommands.Length == 0) return false;
        foreach(CombatActionCommandKey commandKey in allowedCommands)
        {
            if (commandKey == null) continue;
            if(commandKey == command) return true;
        }
        return false;
    }
}
