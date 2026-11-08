using UnityEngine;
[CreateAssetMenu(
    fileName = "ActionSystemConfig",
    menuName = "Game/Actions/Action System Config")]
public class ActionSystemConfig : ScriptableObject
{
    [SerializeField, Min(0f)]
    protected float inputBufferDuration = .15f;

    public float InputBufferDuration => inputBufferDuration;
}
