using UnityEngine;

public class VFXAnimationEvent : MonoBehaviour
{
    [SerializeField] private VFXPlayer vfxPlayer;

    private void Awake()
    {
        if (vfxPlayer == null)
        {
            vfxPlayer = GetComponentInParent<VFXPlayer>();
        }
    }
    public void DestoryVFX()
    {
        vfxPlayer.Destory();
    }
}
