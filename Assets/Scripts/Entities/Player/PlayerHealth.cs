// PlayerHealth.cs
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerHealth : EntityHealth
{
    public enum LifeState { Alive, Downed, Buried }

    [SerializeField, Min(0f)] private float buryDelay = 3.0f;

    [Header("References")]
    [SerializeField] private MonoBehaviour[] disableWhenBuried; // e.g. PlayerController, PlayerMovement, etc.

    public LifeState State { get; private set; } = LifeState.Alive;

    public delegate void StateChanged(LifeState state);
    public event StateChanged OnStateChanged;

    private float buryTimer;
    private BuriedReviveSpot reviveSpot;

    private void OnEnable() => PlayerRegistry.Register(this);
    private void OnDisable() => PlayerRegistry.Unregister(this);

    // Awake handled by base

    private void Update()
    {
        if (State == LifeState.Downed)
        {
            buryTimer += Time.deltaTime;
            if (buryTimer >= buryDelay)
                Bury();
        }
    }

    public override void TakeDamage(float amount)
    {
        if (State != LifeState.Alive) return;
        base.TakeDamage(amount);
    }

    protected override void Die()
    {
        base.Die();
        Down();
    }

    private void Down()
    {
        State = LifeState.Downed;
        buryTimer = 0f;
        OnStateChanged?.Invoke(State);
        
        var anim = GetComponent<EntityAnimation>();
        if (anim != null) anim.SetDeadState(true);
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
        SetHealth(maxHealth * healthPercent);

        State = LifeState.Alive;
        OnStateChanged?.Invoke(State);
        
        var anim = GetComponent<EntityAnimation>();
        if (anim != null) 
        {
            anim.SetDeadState(false);
        }

        foreach (var mb in disableWhenBuried)
            if (mb != null) mb.enabled = true;

        if (reviveSpot != null)
        {
            Destroy(reviveSpot.gameObject);
            reviveSpot = null;
        }
    }
}
