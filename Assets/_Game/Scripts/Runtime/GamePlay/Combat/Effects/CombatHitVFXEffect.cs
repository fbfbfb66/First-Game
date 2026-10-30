using UnityEngine;
[CreateAssetMenu(
    fileName = "Effect_VFX",
    menuName = "Game/Actions/Combat/Effects/VFX")]
public class CombatHitVFXEffect : CombatHitEffect
{
    [SerializeField] private GameObject hitVFXPrefab;
    [SerializeField] private float offsetY = 0.5f;
    [SerializeField] private float offsetX = 0.5f;

    public override void Apply(CombatHitEffectContext context)
    {
        if (context.TargetOwner == null) return;

        if (hitVFXPrefab == null)
        {
            Debug.LogWarning($"No hit VFX prefab assigned to {name}");
            return;
        }
        Vector3 targetPos = context.TargetOwner.transform.position;
        float offsetX = Random.Range(-this.offsetX, this.offsetX);
        float offsetY = Random.Range(-this.offsetY, this.offsetY);
        Vector3 finalPos = targetPos + new Vector3(offsetX, offsetY, 0f);
        Instantiate(hitVFXPrefab, finalPos, Quaternion.identity, context.TargetOwner.transform);
    }
}
