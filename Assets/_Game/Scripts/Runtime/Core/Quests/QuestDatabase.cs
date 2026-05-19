using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(
    fileName = "QuestDatabase",
    menuName = "Game/Quests/Quest Database")]
public class QuestDatabase : ScriptableObject
{
    [SerializeField] private List<QuestData> questDatas = new();

    public IReadOnlyList<QuestData> QuestDatas => questDatas; 
}
