using Unity.VisualScripting;
using UnityEngine;

public class DebugDialogueStarter : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueData dialogueData;
    [SerializeField] private WorldDialogueView dialogueView;
    [SerializeField] private WorldDialogueChoiceView choiceView;

    private void Awake()
    {
        if (dialogueManager == null)
        {
            dialogueManager = FindAnyObjectByType<DialogueManager>();
        }
        if (dialogueView == null)
        {
            dialogueView = FindAnyObjectByType<WorldDialogueView>();
        }
        if (choiceView == null)
        {
            choiceView = FindAnyObjectByType<WorldDialogueChoiceView>();
        }
    }

    [ContextMenu("Start Debug Dialogue")]
    private void StartDebugDialogue()
    {
        if (dialogueManager != null && dialogueData != null && dialogueView != null && choiceView != null)
        {
            dialogueManager.StartDialogue(dialogueData, dialogueView, choiceView);
        }
    }
}
