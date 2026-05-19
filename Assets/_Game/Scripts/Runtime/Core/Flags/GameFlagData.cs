
using UnityEngine;
[CreateAssetMenu(
    fileName = "GameFlagDatabase",
    menuName = "Game/Flags/Flag Data")]
public class GameFlagData : ScriptableObject
{
    [SerializeField] private string flagID;
    [SerializeField] private bool defaultValue;

    [TextArea(2,4)]
    [SerializeField] private string description;

    public string FlagID => flagID;
    public bool DefaultValue => defaultValue;
    public string Description => description;

}
