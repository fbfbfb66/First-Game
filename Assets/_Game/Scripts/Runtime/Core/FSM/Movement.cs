using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] protected Transform visualLayer;
    [SerializeField] protected Rigidbody2D rb;
    public bool facingRight {get;protected set;} = true;

    protected virtual void Awake()
    {
        if(rb == null)
            rb = GetComponent<Rigidbody2D>();
    }

    public virtual void HandleMoveAndFlip(Vector2 inputMove)
    {
        
    }
    public virtual void SetRigibodyVelocity(Vector2 velocity)
    {
        rb.linearVelocity = velocity;
    }

    public Vector2 GetCurrentVelocity()
    {
        return rb.linearVelocity;
    }

    public void HandleFlip(Vector2 inputMove)
    {
        if(inputMove.x < 0 && facingRight) Flip();
        if(inputMove.x > 0 && !facingRight) Flip();
    }
    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = visualLayer.localScale;
        scale.x = -scale.x;
        visualLayer.localScale = scale;
    }
}
