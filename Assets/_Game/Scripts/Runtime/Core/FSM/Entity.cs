using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] protected Animator anim;
    protected StateMachine stateMachine;

    protected virtual void Awake()
    {
        if(anim == null)
            anim = GetComponentInChildren<Animator>();
        stateMachine = new StateMachine();
    }
}
