using UnityEngine;

/// <summary>
/// Initializes player health from the runtime stats created by Player.
/// </summary>
public class PlayerHealth : Health
{
#if UNITY_EDITOR
    [ContextMenu("Debug/Take 10 Damage")]
    private void DebugTakeDamage()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning($"{name} damage debug requires Play Mode. Health unchanged.", this);
            return;
        }

        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer < 0)
        {
            Debug.LogWarning($"{name} damage debug requires the Enemy layer. Health unchanged.", this);
            return;
        }

        var attacker = new GameObject("Debug Damage Source")
        {
            layer = enemyLayer,
            hideFlags = HideFlags.HideAndDontSave
        };
        try
        {
            var context = new DamageContext(attacker, gameObject, new DamageResult(10f, false));
            if (!TryTakeDamage(context))
                Debug.LogWarning($"{name} debug damage was rejected. Check health initialization, death state and accepted attacker layers.", this);
        }
        finally
        {
            Destroy(attacker);
        }
    }
#endif

    public void Initialize(CharacterStats stats)
    {
        if (Stats != null)
        {
            Debug.LogWarning($"{name} PlayerHealth is already initialized. Existing stats and health were preserved.", this);
            return;
        }
        if (stats == null || stats.MaxHealth.Value <= 0f)
        {
            Debug.LogWarning($"{name} cannot initialize PlayerHealth: runtime stats with positive maximum health are required. Health remains uninitialized.", this);
            return;
        }

        Stats = stats;
        CurrentHealth = stats.MaxHealth.Value;
    }
}
