using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "GameFlagDatabase",
    menuName = "Game/Flags/Game Flag Database")]
public class GameFlagDatabase : ScriptableObject
{
    [SerializeField] private List<GameFlagEntry> boolFlags = new();

    public IReadOnlyList<GameFlagEntry> BoolFlags => boolFlags;
}
