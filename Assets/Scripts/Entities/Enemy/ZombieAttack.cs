// ZombieAttack.cs
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ZombieAttack : MonoBehaviour
{
    [SerializeField, Min(0f)] private float damagePerSecond = 12f;
    [SerializeField, Min(0.1f)] private float attackRange = 1.2f;

    private PlayerHealth target;

    private void Update()
    {
        // Acquire/validate target
        if (target == null || target.State != PlayerHealth.LifeState.Alive)
            target = FindClosestInRange();

        if (target == null) return;

        // Deal DPS
        target.TakeDamage(damagePerSecond * Time.deltaTime);
    }

    private PlayerHealth FindClosestInRange()
    {
        PlayerHealth best = null;
        float bestDist = attackRange * attackRange;

        foreach (var p in PlayerRegistry.Players)
        {
            if (p == null || p.State != PlayerHealth.LifeState.Alive) continue;
            float d = (p.transform.position - transform.position).sqrMagnitude;
            if (d <= bestDist)
            {
                bestDist = d;
                best = p;
            }
        }

        return best;
    }
}
