// ZombieBrain.cs
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ZombieBrain : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private AIController aiController;

    [Header("Targeting")]
    [SerializeField, Min(0.1f)] private float retargetInterval = 0.5f;

    private float nextRetarget;

    private void Reset()
    {
        aiController = GetComponent<AIController>();
    }

    private void Awake()
    {
        if (aiController == null) aiController = GetComponent<AIController>();
    }

    private void Update()
    {
        if (aiController == null) return;

        if (Time.time >= nextRetarget)
        {
            nextRetarget = Time.time + retargetInterval;

            var best = FindNearestAlivePlayer();
            if (best != null)
                aiController.SetFollowTarget(best.transform);
        }
    }

    private PlayerHealth FindNearestAlivePlayer()
    {
        PlayerHealth best = null;
        float bestDist = float.MaxValue;

        foreach (var p in PlayerRegistry.Players)
        {
            if (p == null) continue;
            if (p.State != PlayerHealth.LifeState.Alive) continue;

            float d = (p.transform.position - transform.position).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = p;
            }
        }

        return best;
    }
}
