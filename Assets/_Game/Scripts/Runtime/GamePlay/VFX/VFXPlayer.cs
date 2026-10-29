using UnityEngine;
public class VFXPlayer : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private VFXData vfxData;

    private void Awake()
    {
        if(anim == null)
        {
            anim = GetComponentInChildren<Animator>();
        }
    }
    private void Start()
    {
        if (vfxData == null)
        {
            Debug.LogWarning($"No VFX data assigned to {name}");
            return;
        }
        anim.CrossFade(vfxData.VFXAnimName, 0f);
    }

    public void Destory()
    {
        Destroy(gameObject);
    }
}
