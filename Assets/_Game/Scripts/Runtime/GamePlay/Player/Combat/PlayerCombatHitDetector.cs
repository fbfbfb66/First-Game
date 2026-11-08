using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerCombatHitDetector : CombatHitDetector
{
    private Player owner;
    protected override CombatActionController ActionController => owner != null ? owner.CombatActionController : null;
    protected override CharacterStats AttackerStats => owner != null ? owner.Stats : null;

    protected override void ResolveOwner()
    {
        owner = GetComponent<Player>();
    }
}
