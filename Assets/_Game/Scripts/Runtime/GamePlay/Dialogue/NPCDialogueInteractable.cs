using UnityEngine;


public class NPCDialogueInteractable : MonoBehaviour,IInteractable
{
    [Header("Dialogue")]
    [SerializeField] private NPCDialogueProfile dialogueProfile;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private WorldDialogueView worldDialogueView;
    [SerializeField] private WorldDialogueChoiceView worldDialogueChoiceView;
    [SerializeField] private string interactionPrompt = "Talk";

    [Header("Condition")]
    [SerializeField] private GameFlagCenter flagCenter;

    private void Awake()
    {
        if(dialogueManager == null)
            dialogueManager = FindAnyObjectByType<DialogueManager>();
        if(flagCenter == null)
            flagCenter = FindAnyObjectByType<GameFlagCenter>();
        if(worldDialogueView == null)
            worldDialogueView = GetComponentInChildren<WorldDialogueView>(true);
        if(worldDialogueChoiceView == null)
            worldDialogueChoiceView = GetComponentInChildren<WorldDialogueChoiceView>(true);
    }

    public Transform InteractionTransform => transform;

    public bool CanInteract(InteractionContext context)
    {
        if(dialogueProfile == null || dialogueManager == null || worldDialogueChoiceView == null || worldDialogueView == null || dialogueManager.IsRunning)
            return false;
        return true;
    }

    public string GetInteractionPrompt(InteractionContext context)
    {
        return interactionPrompt;
    }

    public void Interact(InteractionContext context)
    {
        if(CanInteract(context) == false)
        {
            Debug.LogWarning("Cannot start dialogue");
            return;
        }
        DialogueData dialogueData = SelectDialogueData(context);
        if(dialogueData == null)
        {
            Debug.LogWarning($"Cannot start dialogue. No dialogue matched profile on {name}.");
            return;
        }

        dialogueManager.StartDialogue(dialogueData,worldDialogueView,worldDialogueChoiceView,gameObject,context.Interactor);
    }

    private DialogueData SelectDialogueData(InteractionContext interactionContext)
    {
        GameConditionContext context = new(flagCenter,gameObject,interactionContext.Interactor);
        return dialogueProfile.SelectDialogue(context);
    }
}
