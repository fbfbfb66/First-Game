using UnityEngine;

public class EnemyHealth : Health
{
    public void Initialize(CharacterStats stats)
    {
        if (Stats != null)
        {
            Debug.LogWarning($"{name} EnemyHealth is already initialized. Existing stats and health were preserved.", this);
            return;
        }
        if (stats == null || stats.MaxHealth.Value <= 0f)
        {
            Debug.LogWarning($"{name} cannot initialize EnemyHealth: runtime stats with positive maximum health are required. Health remains uninitialized.", this);
            return;
        }

        Stats = stats;
        CurrentHealth = stats.MaxHealth.Value;
    }
}
