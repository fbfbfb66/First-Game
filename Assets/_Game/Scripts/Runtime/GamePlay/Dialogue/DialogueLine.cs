using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    [SerializeField] private string lineID;
    [SerializeField] private string speakerName;

    [TextArea(3, 10)]
    [SerializeField] private string lineText;
    [Min(0f)]
    [SerializeField] private float autoAdvanceDelay;

    [SerializeField] private bool waitForInput;

    [SerializeField] private List<DialogueEventAction> eventOnStart = new();
    [SerializeField] private List<DialogueEventAction> eventOnEnd = new();

    public string LineID => lineID;
    public string SpeakerName => speakerName;
    public string LineText => lineText;
    public float AutoAdvanceDelay => autoAdvanceDelay;
    public bool WaitForInput => waitForInput;
    public IReadOnlyList<DialogueEventAction> EventOnStart => eventOnStart;
    public IReadOnlyList<DialogueEventAction> EventOnEnd => eventOnEnd;
}
