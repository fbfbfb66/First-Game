using UnityEngine;
[CreateAssetMenu(
    fileName = "CombatActionSystemConfig",
    menuName = "Game/Combat/Action System Config")]
public sealed class CombatActionSystemConfig : ScriptableObject
{
    [SerializeField, Min(0f)]
    private float inputBufferDuration = .15f;

    public float InputBufferDuration => inputBufferDuration;
}
