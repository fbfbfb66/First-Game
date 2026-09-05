using System;
public readonly struct CombatActionRequest
{
    public CombatActionCommandKey Command { get; }
    public float RequestedTime { get; }

    public CombatActionRequest(CombatActionCommandKey command, float requestedTime)
    {
        if(command == null) throw new ArgumentNullException(nameof(command));

        Command = command;
        RequestedTime = requestedTime;
    }
}
