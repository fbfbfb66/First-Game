using System.Collections.Generic;
using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    [SerializeField] private GameObject interactor;
    private readonly List<IIteractable> interactions = new();

    private void Awake()
    {
        interactor = transform.root.gameObject;
    }

    public void TryInteract()
    {
        IIteractable interaction = FindClosetInteraction();
        if(interaction == null)
        {
            Debug.Log("Did not find valide interaction");
            return;
        }
        InteractionContext context = new InteractionContext(interactor);
        interaction.Interact(context);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        IIteractable interaction = collision.GetComponent<IIteractable>();
        if(interaction == null) return;
        if(interactions.Contains(interaction)) return;
        InteractionContext context = new InteractionContext(interactor);
        if(!interaction.CanInteract(context)) return;
        interactions.Add(interaction);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        IIteractable interaction = collision.GetComponent<IIteractable>();
        if (interactions.Contains(interaction))
        {
            interactions.Remove(interaction);
        }
    }
    private IIteractable FindClosetInteraction()
    {
        IIteractable closestInteraction = null;
        float minDist = Mathf.Infinity;
        foreach(var interaction in interactions)
        {
            float dist = Vector2.Distance(interaction.InteractionTransform.position,interactor.transform.position);
            if(dist < minDist)
            {
                closestInteraction = interaction;
                minDist = dist;
            }
        }
        return closestInteraction;
    }
}
