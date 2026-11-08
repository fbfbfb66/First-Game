using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStatsData", menuName = "Game/Character/Character Stats Data")]
public class CharacterStatsData : ScriptableObject
{
    [SerializeField, Min(1f)] private float maxHealth = 100f;
    [SerializeField, Min(0f)] private float baseAttack = 10f;
    [SerializeField, Range(0f, 1f)] private float criticalChance = 0.05f;
    [SerializeField, Min(1f)] private float criticalDamageMultiplier = 1.5f;
    [SerializeField, Min(0f)] private float defense;
    [SerializeField, Min(0f)] private float baseMana = 100f;

    public float MaxHealth => maxHealth;
    public float BaseAttack => baseAttack;
    public float CriticalChance => criticalChance;
    public float CriticalDamageMultiplier => criticalDamageMultiplier;
    public float Defense => defense;
    public float BaseMana => baseMana;
}
