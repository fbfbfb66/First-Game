using System;
using UnityEngine;
public readonly struct ActionRequest
{
    public ActionCommandKey Command { get; }
    public float RequestedTime { get; }

    public ActionRequest(ActionCommandKey command, float requestedTime)
    {
        if(command == null) throw new ArgumentNullException(nameof(command));

        Command = command;
        RequestedTime = requestedTime;
    }
}
