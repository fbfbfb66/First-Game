
using UnityEngine;

public interface IInteractable 
{
    public Transform InteractionTransform {get;}
    public bool CanInteract(InteractionContext context);
    public void Interact(InteractionContext context);
    string GetInteractionPrompt(InteractionContext context);
}
