using UnityEngine;

/// <summary>
/// Owns current health and uses its owner's shared runtime stats for damage mitigation.
/// </summary>
public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float currentHealth;
    [SerializeField] private LayerMask acceptedAttackerLayers = ~0;

    public CharacterStats Stats { get; protected set; }
    public float CurrentHealth
    {
        get => currentHealth;
        protected set => currentHealth = value;
    }
    public bool IsDead => Stats != null && currentHealth <= 0f;

    public bool CanBeAttacked(GameObject attacker)
    {
        return attacker != null
            && attacker != gameObject
            && (acceptedAttackerLayers.value & (1 << attacker.layer)) != 0;
    }

    public bool TryTakeDamage(DamageContext context)
    {
        if (Stats == null)
        {
            Debug.LogWarning($"{name} cannot take damage: Health has not been initialized. Damage ignored.", this);
            return false;
        }
        if (context.Target != gameObject)
        {
            Debug.LogWarning($"{name} cannot take damage: DamageContext.Target does not match this object. Damage ignored.", this);
            return false;
        }
        if (IsDead) return false;
        if (!CanBeAttacked(context.Attacker)) return false;

        float damage = GameplayCalculator.CalculateIncomingDamage(Stats, context.Damage.Damage);
        float previousHealth = currentHealth;
        currentHealth = Mathf.Max(0f, currentHealth - damage);
        if (currentHealth >= previousHealth) return false;

        Debug.Log($"[Health] {name} 收到 {context.Attacker.name} 的攻击：原始伤害 {context.Damage.Damage}，实际扣血 {previousHealth - currentHealth}，暴击 {context.Damage.IsCritical}，生命值 {previousHealth} -> {currentHealth}。", this);
        return true;
    }
}
