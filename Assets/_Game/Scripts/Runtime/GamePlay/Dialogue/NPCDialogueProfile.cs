using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
[CreateAssetMenu(
    fileName = "NPCDialogueProfile",
    menuName = "Game/Dialogue/NPC Dialogue Profile")]
public class NPCDialogueProfile : ScriptableObject
{
    [FormerlySerializedAs("profiledID")]
    [SerializeField] private string profileID;
    [SerializeField] private List<ConditionalDialogueEntry> dialogueEntries = new();

    public string ProfileID => profileID;

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
