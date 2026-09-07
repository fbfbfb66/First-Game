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
        dashPressed = true;
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
        bool result = HasBufferedJump(jumpBufferDuration);
        ClearJumpRequest();
        return result;
    }

    public bool HasBufferedJump(float jumpBufferDuration)
    {
        if (jumpPressed == false) return false;
        if(Time.time - jumpPressedTime >= jumpBufferDuration) return false;
        return true;
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

    public void ClearDashRequset()
    {
        dashPressed = false;
    }

    public void ClearJumpRequest()
    {
        jumpPressed = false;
    }
}