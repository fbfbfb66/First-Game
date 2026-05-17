using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(
    fileName = "NPCDialogueProfile",
    menuName = "Game/Dialogue/NPC Dialogue Profile")]
public class NPCDialogueProfile : ScriptableObject
{
    [SerializeField] private string profiledID;
    [SerializeField] private List<ConditionalDialogueEntry> dialogueEntries = new();

    public string ProfiledID => profiledID;

    public DialogueData SelectDialogue(GameConditionContext context)
    {
        foreach(var entry in dialogueEntries)
        {
            if(entry == null) continue;

            if (entry.IsMet(context))
            {
                return entry.DialogueData;
            }
        }
        return null;
    }
}
