using System;

/// <summary>
/// A character's independent runtime attributes, initialized from shared configuration.
/// </summary>
public sealed class CharacterStats
{
    public Stat MaxHealth { get; }
    public Stat BaseAttack { get; }
    public Stat CriticalChance { get; }
    public Stat CriticalDamageMultiplier { get; }
    public Stat Defense { get; }
    public Stat BaseMana { get; }

    public CharacterStats(CharacterStatsData data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));

        MaxHealth = new Stat(data.MaxHealth);
        BaseAttack = new Stat(data.BaseAttack);
        CriticalChance = new Stat(data.CriticalChance);
        CriticalDamageMultiplier = new Stat(data.CriticalDamageMultiplier);
        Defense = new Stat(data.Defense);
        BaseMana = new Stat(data.BaseMana);
    }
}
