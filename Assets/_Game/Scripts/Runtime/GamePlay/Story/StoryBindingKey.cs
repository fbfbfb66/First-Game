using UnityEngine;

[CreateAssetMenu(
    fileName = "StoryBindingKey",
    menuName = "Game/Story/Binding Key")]
public class StoryBindingKey : ScriptableObject
{
    [SerializeField] private string bindingID;

    [TextArea(2,4)]
    [SerializeField] private string description;


    public string BindingID => bindingID;
    public string Description => description;
}
