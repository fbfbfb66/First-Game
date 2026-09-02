using UnityEngine;

public class PlayerInputReceiver : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }

    private bool jumpPressed;
    private float jumpPressedTime;
    private bool attackPressed;
    private bool dashPressed;
    private bool worldInteractPressed;

    public void SetMoveInput(Vector2 moveInput)
    {
        MoveInput = moveInput;
    }

    public void RequestJump()
    {
        jumpPressed = true;
        jumpPressedTime = Time.time;
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
        worldInteractPressed = true;
    }

    public void ClearMoveInput()
    {
        MoveInput = Vector2.zero;

        Debug.Log("Player move input cleared.");
    }

    public bool ConsumeJump(float jumpBufferDuration)
    {
        if(jumpPressed && Time.time - jumpPressedTime < jumpBufferDuration)
        {
            ClearJumpRequest();
            return true;
        }
        else
            ClearJumpRequest();
        return false;
    }

    public bool ConsumeAttack()
    {
        if (attackPressed)
        {
            attackPressed = false;
            return true;
        }
        return false;
    }

    public bool ConsumeDash()
    {
        if (dashPressed)
        {
            dashPressed = false;
            return true;
        }
        return false;
    }

    public bool ConsumeWorldInteract()
    {
        if (worldInteractPressed)
        {
            worldInteractPressed = false;
            return true;
        }
        return false;
    }

    public void ClearJumpRequest()
    {
        jumpPressed = false;
    }
}