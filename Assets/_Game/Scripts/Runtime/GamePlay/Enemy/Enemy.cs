using UnityEngine;

[RequireComponent(typeof(EnemyHealth), typeof(CombatHurtbox))]
public class Enemy : MonoBehaviour
{
    [SerializeField] private CharacterStatsData characterStatsData;
    private CombatActionController combatActionController;
    [SerializeField] private MotionActionDefinition hurtAction;
    public EnemyInterruptExecutor InterruptExecutor { get; private set; }

    public CombatActionController CombatActionController => combatActionController ??= new CombatActionController(this);
    public CharacterStats Stats { get; private set; }
    public EnemyHealth Health { get; private set; }

    private void FixedUpdate()
    {
        CombatActionController.TickCooldowns(WorldTime.fixedDeltaTime);
    }

    private void OnEnable()
    {
        if (Health != null) Health.Damaged += OnDamaged;
    }

    private void OnDisable()
    {
        if (Health != null) Health.Damaged -= OnDamaged;
    }

    private void OnDamaged(DamageContext context) => TryEnterHurtState(context);

    public bool TryEnterHurtState(DamageContext context)
    {
        if (!isActiveAndEnabled || InterruptExecutor == null || context.Target != gameObject) return false;
        var facts = new AIActionContext(context);
        return InterruptExecutor.TryStartAction(hurtAction, in facts);
    }

    private void Awake()
    {
        _ = CombatActionController;
        Health = GetComponent<EnemyHealth>();
        InterruptExecutor = GetComponent<EnemyInterruptExecutor>();
        if (characterStatsData == null)
        {
            Debug.LogWarning($"{name} cannot initialize enemy stats and health: CharacterStatsData is not assigned.", this);
            return;
        }

        Stats = new CharacterStats(characterStatsData);
        Health.Initialize(Stats);
    }
}
