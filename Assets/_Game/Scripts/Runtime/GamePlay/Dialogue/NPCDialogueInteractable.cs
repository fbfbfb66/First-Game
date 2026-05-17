using UnityEngine;


public class NPCDialogueInteractable : MonoBehaviour,IInteractable
{
    [SerializeField] private DialogueData dialogueData;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private WorldDialogueView worldDialogueView;
    [SerializeField] private WorldDialogueChoiceView worldDialogueChoiceView;

    private void Awake()
    {
        if(dialogueManager == null)
            dialogueManager = FindAnyObjectByType<DialogueManager>();
        if(worldDialogueView == null)
            worldDialogueView = GetComponentInChildren<WorldDialogueView>(true);
        if(worldDialogueChoiceView == null)
            worldDialogueChoiceView = GetComponentInChildren<WorldDialogueChoiceView>(true);
    }

    public Transform InteractionTransform => transform;

    public bool CanInteract(InteractionContext context)
    {
        if(dialogueData == null || dialogueManager == null || worldDialogueChoiceView == null || worldDialogueView == null || dialogueManager.IsRunning)
            return false;
        return true;
    }

    public string GetInteractionPrompt(InteractionContext context)
    {
        throw new System.NotImplementedException();
    }

    public void Interact(InteractionContext context)
    {
        if(CanInteract(context) == false)
        {
            Debug.LogWarning("Cannot start dialogue");
            return;
        } 

        dialogueManager.StartDialogue(dialogueData,worldDialogueView,worldDialogueChoiceView,gameObject,context.Interactor);
    }
}
