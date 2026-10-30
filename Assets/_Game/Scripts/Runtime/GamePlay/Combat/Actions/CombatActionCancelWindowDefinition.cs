using System;
using UnityEngine;
[Serializable]
public sealed class CombatActionCancelWindowDefinition
{
    [SerializeField] private int windowId;
    [SerializeField] private ActionCommandKey[] allowedCommands;

    public int WindowId => windowId;
    public ActionCommandKey[] AllowedCommands => allowedCommands;

    public bool Accepts(ActionCommandKey command)
    {
        if (command == null) return false;
        if(allowedCommands == null || allowedCommands.Length == 0) return false;
        foreach(ActionCommandKey commandKey in allowedCommands)
        {
            if (commandKey == null) continue;
            if(commandKey == command) return true;
        }
        return false;
    }
}
