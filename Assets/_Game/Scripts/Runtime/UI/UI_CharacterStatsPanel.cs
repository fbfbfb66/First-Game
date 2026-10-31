using TMPro;
using UnityEngine;

public class UI_CharacterStatsPanel : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private TMP_Text healthValueText;
    [SerializeField] private TMP_Text attackValueText;
    [SerializeField] private TMP_Text defenseValueText;
    [SerializeField] private TMP_Text criticalChanceValueText;
    [SerializeField] private TMP_Text criticalDamageValueText;
    [SerializeField] private TMP_Text baseManaValueText;

    private bool refreshRequested = true;
    private float displayedHealth;
    private float displayedMaxHealth;
    private float displayedAttack;
    private float displayedDefense;
    private float displayedCriticalChance;
    private float displayedCriticalDamage;
    private float displayedBaseMana;

    private void Awake()
    {
        if (player == null)
            player = FindAnyObjectByType<Player>();

        if (player == null || healthValueText == null || attackValueText == null
            || defenseValueText == null || criticalChanceValueText == null
            || criticalDamageValueText == null || baseManaValueText == null)
        {
            Debug.LogWarning($"{name} cannot display character stats: assign Player and all six value texts. Panel updates disabled.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        refreshRequested = true;
        RefreshStats();
    }

    private void LateUpdate()
    {
        RefreshStats();
    }

    private void RefreshStats()
    {
        // The first OnEnable can run before Player.Awake initializes runtime data.
        if (player == null || player.Stats == null || player.Health == null
            || player.Health.Stats == null)
            return;

        CharacterStats stats = player.Stats;
        float health = player.Health.CurrentHealth;
        float maxHealth = stats.MaxHealth.Value;
        if (refreshRequested || displayedHealth != health || displayedMaxHealth != maxHealth)
        {
            healthValueText.text = $"{health:0.##}/{maxHealth:0.##}";
            displayedHealth = health;
            displayedMaxHealth = maxHealth;
        }

        RefreshValue(attackValueText, stats.BaseAttack.Value, ref displayedAttack);
        RefreshValue(defenseValueText, stats.Defense.Value, ref displayedDefense);
        RefreshValue(criticalChanceValueText, stats.CriticalChance.Value, ref displayedCriticalChance, true);
        RefreshValue(criticalDamageValueText, stats.CriticalDamageMultiplier.Value, ref displayedCriticalDamage, true);
        RefreshValue(baseManaValueText, stats.BaseMana.Value, ref displayedBaseMana);
        refreshRequested = false;
    }

    private void RefreshValue(TMP_Text text, float value, ref float displayedValue, bool percentage = false)
    {
        if (!refreshRequested && displayedValue == value)
            return;

        text.text = percentage ? $"{value * 100f:0.##}%" : value.ToString("0.##");
        displayedValue = value;
    }
}
