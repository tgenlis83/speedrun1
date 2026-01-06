using UnityEngine;

[DisallowMultipleComponent]
public class EntityHealth : MonoBehaviour
{
    [Header("Entity Health")]
    [SerializeField, Min(1f)] protected float maxHealth = 100f;
    [SerializeField] private HealthbarController healthBarPrefab;

    public float Health { get; protected set; }
    public float MaxHealth => maxHealth;

    public delegate void HealthChanged(float current, float max);
    public event HealthChanged OnHealthChanged;

    public delegate void DeathEvent();
    public event DeathEvent OnDeath;

    protected virtual void Awake()
    {
        Health = maxHealth;
    }

    protected virtual void Start()
    {
        if (healthBarPrefab != null)
        {
            var bar = Instantiate(healthBarPrefab, transform.position, Quaternion.identity);
            bar.Initialize(this.transform);
            bar.UpdateHealth(Health, maxHealth);
            
            // Subscribe the bar to updates
            OnHealthChanged += bar.UpdateHealth;
        }

        OnHealthChanged?.Invoke(Health, maxHealth);
    }

    public virtual void TakeDamage(float amount)
    {
        if (Health <= 0f) return;

        Health = Mathf.Max(0f, Health - amount);
        OnHealthChanged?.Invoke(Health, maxHealth);

        if (Health <= 0f)
        {
            Die();
        }
    }

    public virtual void SetHealth(float value)
    {
        Health = Mathf.Clamp(value, 0f, maxHealth);
        OnHealthChanged?.Invoke(Health, maxHealth);
    }

    protected virtual void Die()
    {
        OnDeath?.Invoke();
    }
}
