// AIController.cs
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NavMesh-driven AI controller that reuses PlayerMovement.
/// - NavMeshAgent does pathfinding ONLY (does not move/rotate the transform).
/// - Converts agent desired direction into the Vector2 "move input" PlayerMovement expects.
/// - Optional slowdown near destination (by scaling input magnitude).
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[DisallowMultipleComponent]
public sealed class AIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EntityMovement movement;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Rigidbody rb;

    [Header("Destination")]
    [SerializeField] private bool followTarget = true;
    [SerializeField] private Transform target;
    [Tooltip("Used when Follow Target is false.")]
    [SerializeField] private Vector3 staticDestination;

    [Header("Repath")]
    [Tooltip("How often we refresh SetDestination (seconds). Lower = more responsive, higher = cheaper.")]
    [SerializeField, Min(0.02f)] private float repathInterval = 0.25f;

    [Header("Arrive / Speed")]
    [Tooltip("Stop when within this distance to destination.")]
    [SerializeField, Min(0f)] private float stoppingDistance = 1.0f;

    [Tooltip("Extra epsilon to reduce jitter around stopping distance.")]
    [SerializeField, Min(0f)] private float arriveEpsilon = 0.05f;

    [Tooltip("Start slowing down when remaining distance is below this. Set <= stoppingDistance to disable slowdown.")]
    [SerializeField, Min(0f)] private float slowDownDistance = 3.0f;

    [Tooltip("If true, forces input to zero when arrived (prevents creeping).")]
    [SerializeField] private bool hardStopOnArrive = true;

    [Header("Sync")]
    [Tooltip("If true, sync NavMeshAgent.speed to PlayerMovement.MoveSpeed (useful if you upgrade speed).")]
    [SerializeField] private bool syncAgentSpeedToMovement = true;

    private float nextRepathTime;

    public Transform Target
    {
        get => target;
        set
        {
            target = value;
            followTarget = (target != null);
            ForceRepath();
        }
    }

    public void SetDestination(Vector3 worldPos)
    {
        staticDestination = worldPos;
        followTarget = false;
        ForceRepath();
    }

    public void SetFollowTarget(Transform newTarget)
    {
        target = newTarget;
        followTarget = (target != null);
        ForceRepath();
    }

    public void ForceRepath() => nextRepathTime = 0f;

    private void Reset()
    {
        movement = GetComponent<EntityMovement>();
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
    }

    private void Awake()
    {
        movement ??= GetComponent<EntityMovement>();
        agent ??= GetComponent<NavMeshAgent>();
        rb ??= GetComponent<Rigidbody>();

        // Pathfinding only: we move via Rigidbody (EntityMovement).
        agent.updatePosition = false;
        agent.updateRotation = false;

        agent.stoppingDistance = stoppingDistance;
    }

    private void Update()
    {
        if (movement == null || agent == null || !agent.enabled) return;
        if (!agent.isOnNavMesh) return;

        agent.stoppingDistance = stoppingDistance;

        if (syncAgentSpeedToMovement)
            agent.speed = movement.MoveSpeed;

        if (Time.time >= nextRepathTime)
        {
            nextRepathTime = Time.time + repathInterval;

            Vector3 dest = (followTarget && target != null) ? target.position : staticDestination;
            agent.SetDestination(dest);
        }
    }

    private void FixedUpdate()
    {
        if (movement == null || agent == null || !agent.enabled)
            return;

        if (!agent.isOnNavMesh)
        {
            movement.SetMoveInput(Vector2.zero);
            return;
        }

        // Keep the agent's internal position synced with the Rigidbody-driven character.
        agent.nextPosition = (rb != null) ? rb.position : transform.position;

        bool arrived =
            !agent.pathPending &&
            agent.hasPath &&
            agent.remainingDistance <= (stoppingDistance + arriveEpsilon);

        if (hardStopOnArrive && arrived)
        {
            movement.SetMoveInput(Vector2.zero);
            return;
        }

        Vector3 desiredVel = agent.desiredVelocity;
        desiredVel.y = 0f;

        if (desiredVel.sqrMagnitude < 0.000001f)
        {
            movement.SetMoveInput(Vector2.zero);
            return;
        }

        Vector3 desiredDir = desiredVel.normalized;

        // Convert world direction to the Vector2 input PlayerMovement expects (world-relative or relative-to camera).
        Transform rel = movement.RelativeTo; // may be null
        Vector2 input = WorldDirToMoveInput(desiredDir, rel);

        // Optional slowdown near destination by scaling input magnitude.
        float scale = 1f;
        if (slowDownDistance > stoppingDistance + 0.0001f && agent.hasPath && !agent.pathPending)
        {
            float dist = agent.remainingDistance;
            scale = Mathf.Clamp01((dist - stoppingDistance) / (slowDownDistance - stoppingDistance));
        }

        movement.SetMoveInput(input * scale);
    }

    private static Vector2 WorldDirToMoveInput(Vector3 worldDir, Transform relativeTo)
    {
        // PlayerMovement interprets Vector2 as:
        // - world-relative if RelativeTo == null: (x -> world X, y -> world Z)
        // - relative-to transform if set: (x -> right, y -> forward)
        if (relativeTo == null)
            return new Vector2(worldDir.x, worldDir.z);

        Vector3 fwd = relativeTo.forward; fwd.y = 0f; fwd.Normalize();
        Vector3 right = relativeTo.right; right.y = 0f; right.Normalize();

        float x = Vector3.Dot(worldDir, right);
        float y = Vector3.Dot(worldDir, fwd);
        return new Vector2(x, y);
    }
}
