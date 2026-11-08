using UnityEngine;

/// <summary>
/// Initializes player health from the runtime stats created by Player.
/// </summary>
public class PlayerHealth : Health
{


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
