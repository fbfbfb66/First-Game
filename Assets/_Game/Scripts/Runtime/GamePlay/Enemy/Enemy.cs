using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class Enemy : MonoBehaviour
{
    [SerializeField] private CharacterStatsData characterStatsData;

    public CharacterStats Stats { get; private set; }
    public EnemyHealth Health { get; private set; }

    private void Awake()
    {
        Health = GetComponent<EnemyHealth>();
        if (characterStatsData == null)
        {
            Debug.LogWarning($"{name} cannot initialize enemy stats and health: CharacterStatsData is not assigned.", this);
            return;
        }

        Stats = new CharacterStats(characterStatsData);
        Health.Initialize(Stats);
    }
}
