using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class ConditionalDialogueEntry 
{
    [SerializeField] private string entryID;
    [SerializeField] private DialogueData dialogueData;
    [SerializeField] private List<GameCondition> conditions = new();

    public string EntryID => entryID;
    public DialogueData DialogueData => dialogueData;

    public bool IsMet(GameConditionContext context)
    {
        if(dialogueData == null)
        {
            Debug.LogWarning($"Conditional dialogue entry has no DialogueData. EntryId: {entryID}");
            return false;
        }

        foreach(var condition in conditions)
        {
            if(condition == null)
            {
                Debug.LogWarning($"Conditional dialogue entry has a null condition. EntryId: {entryID}");
                return false;
            }
            if (!condition.IsMet(context))
            {
                return false;
            }
        }
        return true;
    }
}
