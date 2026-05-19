using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "GameFlagDatabase",
    menuName = "Game/Flags/Game Flag Database")]
public class GameFlagDatabase : ScriptableObject
{
    [SerializeField] private List<GameFlagData> boolFlags = new();

    public IReadOnlyList<GameFlagData> BoolFlags => boolFlags;
}
