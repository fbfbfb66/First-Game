using UnityEngine;

/// <summary>
/// Carries one attack's participants and outgoing damage before target mitigation.
/// </summary>
public readonly struct DamageContext
{
    public GameObject Attacker { get; }
    public GameObject Target { get; }
    public DamageResult Damage { get; }
    public AttackType AttackType { get; }

    public DamageContext(GameObject attacker, GameObject target, DamageResult damage, AttackType attackType)
    {
        Attacker = attacker;
        Target = target;
        Damage = damage;
        AttackType = attackType;
    }
}
