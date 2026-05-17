using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private GameLayerStack layerStack;
    [SerializeField] private GameEventBus eventBus;

    private DialogueData currentDialogue;
    private DialogueNode currentNode;
    private WorldDialogueView currentDialogueView;
    private WorldDialogueChoiceView currentChoiceView;
    private GameObject currentSender;
    private GameObject currentInstigator;

    private Coroutine dialogueCoroutine;
    private bool advanceRequested;

    public bool IsRunning => currentDialogue != null;

    private void Awake()
    {
        if (layerStack == null)
        {
            layerStack = FindAnyObjectByType<GameLayerStack>();
        }
        if (eventBus == null)
        {
            eventBus = FindAnyObjectByType<GameEventBus>();
        }
    }

    public void StartDialogue(DialogueData dialogue, WorldDialogueView view, WorldDialogueChoiceView choiceView, GameObject sender = null, GameObject instigator = null)
    {
        if (IsRunning)
        {
            Debug.LogWarning("DialogueManager: Attempted to start a new dialogue while one is already running!");
            return;
        }
        if (dialogue == null)
        {
            Debug.LogError("DialogueManager: Attempted to start a dialogue with null DialogueData!");
            return;
        }
        if (view == null)
        {
            Debug.LogError("DialogueManager: Attempted to start a dialogue with null WorldDialogueView!");
            return;
        }
        if (eventBus == null)
        {
            Debug.LogError("DialogueManager: No GameEventBus found!");
            return;
        }

        currentDialogue = dialogue;
        currentDialogueView = view;
        currentChoiceView = choiceView;
        currentSender = sender;
        currentInstigator = instigator;
        PushLayer(GameLayerType.Dialogue);

        dialogueCoroutine = StartCoroutine(RunDialogue());
    }

    public void HandleChoiceSelectedNavigate(Vector2 navigateInput)
    {
        if(!IsRunning || currentNode == null || currentNode.Choices.Count == 0)
        {
            return;
        }
        currentChoiceView.MoveSelection(navigateInput);
    }

    public void HandleChoiceConfirmed()
    {
        if (!IsRunning || currentNode == null || currentNode.Choices.Count == 0)
        {
            return;
        }
        DialogueChoice selectedChoice = currentChoiceView.GetSelectedChoice();
        if (selectedChoice != null)
        {
            currentNode = currentDialogue.GetNode(selectedChoice.NextNodeID);
            ExecuteEvents(selectedChoice.EventOnSelect, null);
            if (currentNode != null)
            {
                ExitChoiceMode();
                if (dialogueCoroutine != null)
                {
                    StopCoroutine(dialogueCoroutine);
                }
                dialogueCoroutine = StartCoroutine(RunNode(currentNode));
            }
        }
    }

    public void RequestAdvance()
    {
        if(!IsRunning) return;
        advanceRequested = true;
    }

    private IEnumerator RunDialogue()
    {
        currentNode = currentDialogue.GetNode(currentDialogue.StartingNodeID);
        if(currentNode == null)
        {
            Debug.LogError($"DialogueManager: Starting node with ID {currentDialogue.StartingNodeID} not found in dialogue {currentDialogue.name}!");
            EndDialogue();
            yield break;
        }
        yield return RunNode(currentNode);
    }

    private IEnumerator RunNode(DialogueNode node)
    {
        foreach (var line in node.DialogueLines)
        {
            currentDialogueView.ShowLine(line);
            ExecuteEvents(line.EventOnStart,line);
            advanceRequested = false;

            if (line.WaitForInput)
            {
                yield return new WaitUntil(() => advanceRequested);
            }
            else
            {
                float timer = 0f;
                while (timer < line.AutoAdvanceDelay && !advanceRequested)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }
            }
            ExecuteEvents(line.EventOnEnd,line);
        }
        if(node.Choices.Count > 0)
        {
            EnterChoiceMode(node);
            yield break;
        }
        EndDialogue();
    }

    private void EnterChoiceMode(DialogueNode node)
    {
        currentChoiceView.ShowChoices(node.Choices);
        PushLayer(GameLayerType.DialogueChoice);
    }
    private void ExitChoiceMode()
    {
        currentChoiceView.Hide();
        PopDialogueLayer(GameLayerType.DialogueChoice);
    }

    private void ExecuteEvents(IReadOnlyList<DialogueEventAction> events,DialogueLine line)
    {
        DialogueContext context = new DialogueContext(eventBus,line,currentNode,currentDialogue,currentSender,currentInstigator);
        foreach (var action in events)
        {
            if(action == null) continue;
            action.Execute(context);
        }
    }

    private void EndDialogue()
    {
        if(dialogueCoroutine != null)
        {
            StopCoroutine(dialogueCoroutine);
            dialogueCoroutine = null;
        }

        if (currentDialogueView != null)
        {
            currentDialogueView.Hide();
            currentDialogueView = null;
        }
        PopDialogueLayer(GameLayerType.Dialogue);
        currentDialogue = null;
        currentNode = null;
        currentSender = null;
        currentInstigator = null;
        advanceRequested = false;
    }

    private void PushLayer(GameLayerType layerType)
    {
        if(layerStack == null)
        {
            Debug.LogError("DialogueManager: No GameLayerStack found!");
            return;
        }

        if (!layerStack.ContainsLayer(layerType))
        {
            layerStack.PushLayer(layerType);
        }
    }

    private void PopDialogueLayer(GameLayerType layerType)
    {
        if (layerStack == null)
        {
            Debug.LogError("DialogueManager: No GameLayerStack found!");
            return;
        }

        if (layerStack.ContainsLayer(layerType))
        {
            layerStack.PopLayer(layerType);
        }
    }
}
