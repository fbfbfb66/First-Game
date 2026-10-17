/// <summary>
/// Damage produced by the attacker, before the receiver applies defense.
/// </summary>
public readonly struct DamageResult
{
    public float Damage { get; }
    public bool IsCritical { get; }

    public DamageResult(float damage, bool isCritical)
    {
        Damage = damage;
        IsCritical = isCritical;
    }
}
