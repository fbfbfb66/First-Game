using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class DialogueNode
{
    [SerializeField] private string nodeID;
    [SerializeField] private List<DialogueLine> dialogueLine = new();
    [SerializeField] private List<DialogueChoice> choices = new();

    public string NodeID => nodeID;
    public IReadOnlyList<DialogueLine> DialogueLines => dialogueLine;
    public IReadOnlyList<DialogueChoice> Choices => choices;
}
