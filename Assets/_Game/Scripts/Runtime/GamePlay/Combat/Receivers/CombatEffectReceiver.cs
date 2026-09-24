using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]
public class CombatEffectReceiver : MonoBehaviour
{
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private bool canBeLaunched = true;

    private bool isMotionPaused;
    private Vector2 velocityBeforePause;
    private float gravityScaleBeforePause;

    private void Awake()
    {
        if (body == null)
            body = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (isMotionPaused)
        {
            body.linearVelocity = Vector2.zero;
        }
    }

    public bool TryBeginMotionPause()
    {
        if (body == null)
        {
            Debug.LogWarning("body is null");
            return false;
        }

        if (isMotionPaused) return false;
        velocityBeforePause = body.linearVelocity;
        gravityScaleBeforePause = body.gravityScale;
        body.linearVelocity = Vector2.zero;
        body.gravityScale = 0;
        isMotionPaused = true;
        return true;
    }

    public void EndMotionPause()
    {
        if (isMotionPaused == false) return;
        isMotionPaused = false;
        body.linearVelocity = velocityBeforePause;
        body.gravityScale = gravityScaleBeforePause;
    }

    public bool TryLaunch(float verticalVelocity)
    {
        if (canBeLaunched == false) return false;
        if (body == null)
        {
            Debug.LogWarning("body is null");
            return false;
        }

        Vector2 temp = body.linearVelocity;
        temp.y = verticalVelocity;
        body.linearVelocity = temp;
        return true;
    }
}
