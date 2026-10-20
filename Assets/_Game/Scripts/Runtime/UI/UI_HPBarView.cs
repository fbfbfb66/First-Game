using UnityEngine;

public  class UI_HPBarView : UI_ResourceBarView
{
    [SerializeField] protected PlayerHealth playerHealth;

    public PlayerHealth PlayerHealth => playerHealth;

    private void OnEnable()
    {
        if (playerHealth == null)
        {
            Debug.LogWarning($"{name} HP bar needs a PlayerHealth reference; health updates are not bound.", this);
            return;
        }

        playerHealth.Damaged += RefreshHealth;
        RefreshHealth();
    }

    private void Start()
    {
        RefreshHealth();
    }

    protected virtual void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.Damaged -= RefreshHealth;
    }


    protected virtual void RefreshHealth()
    {
        if (playerHealth == null || playerHealth.Stats == null) return;

        float maxHealth = playerHealth.Stats.MaxHealth.Value;
        if (maxHealth <= 0f) return;

        SetPercent(Mathf.Clamp01(playerHealth.CurrentHealth / maxHealth));
    }

    public override void SetPercent(float percent)
    {
        base.SetPercent(percent);
    }
}
