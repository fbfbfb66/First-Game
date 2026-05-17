using UnityEngine;
public readonly struct DialogueContext
{
    public GameEventBus EventBus { get; }
    public DialogueLine CurrentLine { get; }
    public DialogueNode CurrentNode { get; }
    public DialogueData DialogueData { get; }
    public GameObject Sender {get;}
    public GameObject Instigator{get;}

    public DialogueContext(GameEventBus gameEventBus, DialogueLine currentLine, DialogueNode currentNode, DialogueData dialogueData, GameObject sender, GameObject instigator)
    {
        EventBus = gameEventBus;
        CurrentLine = currentLine;
        CurrentNode = currentNode;
        DialogueData = dialogueData;
        Sender = sender;
        Instigator = instigator;
    }
}
