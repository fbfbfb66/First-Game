
using UnityEngine;

public interface IIteractable 
{
    public Transform InteractionTransform {get;}
    public bool CanInteract(InteractionContext context);
    public void Interact(InteractionContext context);
    string GetInteractionPrompt(InteractionContext context);
}
