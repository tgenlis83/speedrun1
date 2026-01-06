// PlayerHealth.cs
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerHealth : MonoBehaviour
{
    public enum LifeState { Alive, Downed, Buried }

    [Header("Health")]
    [SerializeField, Min(1f)] private float maxHealth = 100f;
    [SerializeField, Min(0f)] private float buryDelay = 3.0f;

    [Header("References")]
    [SerializeField] private MonoBehaviour[] disableWhenBuried; // e.g. PlayerController, PlayerMovement, etc.

    public float Health { get; private set; }
    public LifeState State { get; private set; } = LifeState.Alive;

    public delegate void HealthChanged(float current, float max);
    public event HealthChanged OnHealthChanged;

    public delegate void StateChanged(LifeState state);
    public event StateChanged OnStateChanged;

    private float buryTimer;
    private BuriedReviveSpot reviveSpot;

    private void OnEnable() => PlayerRegistry.Register(this);
    private void OnDisable() => PlayerRegistry.Unregister(this);

    private void Awake()
    {
        Health = maxHealth;
        OnHealthChanged?.Invoke(Health, maxHealth);
    }

    private void Update()
    {
        if (State == LifeState.Downed)
        {
            buryTimer += Time.deltaTime;
            if (buryTimer >= buryDelay)
                Bury();
        }
    }

    public void TakeDamage(float amount)
    {
        if (State != LifeState.Alive) return;

        Health = Mathf.Max(0f, Health - amount);
        OnHealthChanged?.Invoke(Health, maxHealth);

        if (Health <= 0f)
            Down();
    }

    private void Down()
    {
        State = LifeState.Downed;
        buryTimer = 0f;
        OnStateChanged?.Invoke(State);
    }

    private void Bury()
    {
        State = LifeState.Buried;
        OnStateChanged?.Invoke(State);

        foreach (var mb in disableWhenBuried)
            if (mb != null) mb.enabled = false;

        // Create a revive spot at this position.
        reviveSpot = BuriedReviveSpot.CreateAt(transform.position, this);
    }

    public void Revive(float healthPercent = 0.5f)
    {
        Health = Mathf.Clamp(maxHealth * healthPercent, 1f, maxHealth);
        OnHealthChanged?.Invoke(Health, maxHealth);

        State = LifeState.Alive;
        OnStateChanged?.Invoke(State);

        foreach (var mb in disableWhenBuried)
            if (mb != null) mb.enabled = true;

        if (reviveSpot != null)
        {
            Destroy(reviveSpot.gameObject);
            reviveSpot = null;
        }
    }
}
