// ZombieSpawner.cs
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public sealed class ZombieSpawner : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField, Min(0f)] private float baseSpawnInterval = 3.5f;
    [SerializeField, Min(0)] private int maxAlive = 12;
    [SerializeField, Min(0f)] private float spawnRadius = 18f;

    [Header("Runtime")]
    [SerializeField, Range(0f, 3f)] private float intensityMultiplier = 1f;

    private float nextSpawn;
    private int aliveCount;

    public void SetIntensity(float multiplier)
    {
        intensityMultiplier = Mathf.Clamp(multiplier, 0f, 3f);
    }

    private void Update()
    {
        if (zombiePrefab == null) return;
        if (intensityMultiplier <= 0f) return;

        if (aliveCount >= maxAlive) return;

        if (Time.time >= nextSpawn)
        {
            float interval = baseSpawnInterval / Mathf.Max(0.01f, intensityMultiplier);
            nextSpawn = Time.time + interval;

            SpawnOne();
        }
    }

    private void SpawnOne()
    {
        Vector3 center = transform.position;
        Vector3 pos = center + Random.insideUnitSphere * spawnRadius;
        pos.y = center.y;

        if (!NavMesh.SamplePosition(pos, out var hit, 5f, NavMesh.AllAreas))
            return;

        var go = Instantiate(zombiePrefab, hit.position, Quaternion.identity);
        aliveCount++;

        // Decrement aliveCount when destroyed (simple; later use pooling).
        var deathHook = go.AddComponent<DestroyHook>();
        deathHook.OnDestroyed += () => aliveCount--;
    }

    private sealed class DestroyHook : MonoBehaviour
    {
        public System.Action OnDestroyed;
        private void OnDestroy() => OnDestroyed?.Invoke();
    }
}
