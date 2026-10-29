using UnityEngine;
[CreateAssetMenu(
    fileName = "VFX_",
    menuName = "Game/VFX")]
public class VFXData : ScriptableObject
{
    [SerializeField] private string vfxAnimName;

    public string VFXAnimName => vfxAnimName;
}
