using UnityEngine;

public class PlayerAnimationTrigger : MonoBehaviour
{
    public bool IsAnimationFinished { get; private set; }
    public bool canPerformAction { get; private set; }
    public void EndAnimation()
    {
        IsAnimationFinished = true;
    }
    public void StartAnimation()
    {
        IsAnimationFinished = false;
    }

    public void EnableAction()
    {
        canPerformAction = true;
    }
    public void DisableAction()
    {
        canPerformAction = false;
    }
}
