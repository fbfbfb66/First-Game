using UnityEngine;

public class PlayerInputReceiver : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }

    public void SetMoveInput(Vector2 moveInput)
    {
        MoveInput = moveInput;

        Debug.Log($"Player received move input: {MoveInput}");
    }

    public void RequestJump()
    {
        Debug.Log("Player received jump request.");
    }

    public void RequestAttack()
    {
        Debug.Log("Player received attack request.");
    }

    public void RequestDash()
    {
        Debug.Log("Player received dash request.");
    }

    public void RequestWorldInteract()
    {
        Debug.Log("Player received world interact request.");
    }

    public void ClearMoveInput()
    {
        MoveInput = Vector2.zero;

        Debug.Log("Player move input cleared.");
    }
}