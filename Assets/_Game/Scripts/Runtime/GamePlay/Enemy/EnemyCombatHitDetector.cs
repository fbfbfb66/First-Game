using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyCombatHitDetector : CombatHitDetector
{
    private Enemy owner;
    protected override CombatActionController ActionController => owner != null ? owner.CombatActionController : null;
    protected override CharacterStats AttackerStats => owner != null ? owner.Stats : null;

    protected override void ResolveOwner()
    {
        owner = GetComponent<Enemy>();
    }
}
