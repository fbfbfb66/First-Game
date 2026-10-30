using UnityEngine;

// Continuous input belongs to the player and survives locomotion state changes.
public sealed class PlayerCommandInput
{
    public Vector2 MoveInput { get; private set; }

    public void SetMoveInput(Vector2 moveInput)
    {
        MoveInput = moveInput;
    }

    public void ClearMoveInput()
    {
        MoveInput = Vector2.zero;
    }
}
