using System;

[Flags]
public enum PlayerActionType 
{
    None = 0,

    Move = 1 << 0,
    Jump = 1 << 1,
    Attack = 1 << 2,
    Dash = 1 << 3,
    WorldInteract = 1 << 4,
    UseItem = 1 << 5
}
