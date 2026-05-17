using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueChoice
{
    [SerializeField] private string choiceID;
    [TextArea(3, 10)]
    [SerializeField] private string choiceText;

    [SerializeField] private string nextNodeID;

    [SerializeField] private List<DialogueEventAction> eventOnSelect = new();
    public string ChoiceID => choiceID;
    public string ChoiceText => choiceText;
    public string NextNodeID => nextNodeID;
    public IReadOnlyList<DialogueEventAction> EventOnSelect => eventOnSelect;
}
