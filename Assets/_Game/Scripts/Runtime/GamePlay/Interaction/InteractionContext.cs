using UnityEngine;

public readonly struct InteractionContext 
{
    public GameObject Interactor{get;}
    public Transform IneractorTransform {get;}

    public InteractionContext(GameObject interactor)
    {
        Interactor = interactor;
        IneractorTransform = interactor != null ? Interactor.transform : null;
    }
}
