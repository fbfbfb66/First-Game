using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(
    fileName = "DialogueData",
    menuName = "Game/Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [SerializeField] private string dialogueID;
    [SerializeField] private string startingNodeID = "start";
    [SerializeField] private List<DialogueNode> dialogueNodes = new();

    public string DialogueID => dialogueID;
    public string StartingNodeID => startingNodeID;
    public IReadOnlyList<DialogueNode> DialogueNodes => dialogueNodes;

    public DialogueNode GetNode(string nodeID)
    {
        foreach (var node in dialogueNodes)
        {
            if (node.NodeID == nodeID)
                return node;
        }
        Debug.Log($"Node with ID {nodeID} not found in dialogue {dialogueID}");
        return null;
    }
}
