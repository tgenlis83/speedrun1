// PlayerMovement.cs
using UnityEngine;

/// <summary>
/// Rigidbody-based top-down movement.
/// - Receives a desired move input from PlayerController (or any input source).
/// - Moves using velocity change (plays nice with other rigidbodies).
/// - Smoothly rotates to face movement direction.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public sealed class EntityMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)] private float moveSpeed = 6f;
    [Tooltip("How fast we accelerate toward target speed (m/s^2). Higher = snappier.")]
    [SerializeField, Min(0f)] private float acceleration = 40f;
    [Tooltip("How fast we decelerate when no input (m/s^2). Higher = stops quicker.")]
    [SerializeField, Min(0f)] private float deceleration = 60f;

    [Header("Rotation")]
    [Tooltip("Degrees per second when turning to face movement direction.")]
    [SerializeField, Min(0f)] private float turnSpeedDeg = 720f;
    [Tooltip("Minimum input magnitude before we rotate.")]
    [SerializeField, Range(0f, 0.5f)] private float rotateDeadzone = 0.05f;

    [Header("Reference Frame")]
    [Tooltip("If set, movement is relative to this transform (e.g., your camera). If null, world-relative.")]
    [SerializeField] private Transform relativeTo;

    [Header("Physics")]
    [Tooltip("If true, keeps current Rigidbody Y velocity (gravity/jumps) while controlling XZ.")]
    [SerializeField] private bool preserveYVelocity = true;

    [SerializeField] private EntityAnimation entityAnimation;

    private Rigidbody rb;
    private Vector2 moveInput; // raw input (-1..1)
    private bool hasInput;
    private bool isDragging;
    private Transform draggingTarget;
    private Vector3? lookAtPosition;

    public bool IsDragging => isDragging;

    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = Mathf.Max(0f, value);
    }

    public Transform RelativeTo
    {
        get => relativeTo;
        set => relativeTo = value;
    }

    public void SetDragging(bool dragging, Transform target = null)
    {
        isDragging = dragging;
        draggingTarget = target;
    }

    public void SetLookAtPosition(Vector3? pos)
    {
        lookAtPosition = pos;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        entityAnimation ??= GetComponent<EntityAnimation>();
        // Recommended for smoother visuals with physics movement:
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    /// <summary>Set desired movement input. Expected range is -1..1 per axis.</summary>
    public void SetMoveInput(Vector2 input)
    {
        moveInput = input.normalized;
        hasInput = moveInput.sqrMagnitude > 0.0001f;
        entityAnimation.SetFloats(moveInput.magnitude, rb.linearVelocity.magnitude);
    }

    private void FixedUpdate()
    {
        MoveFixed();
        RotateFixed();
    }

    private void MoveFixed()
    {
        // Convert input into a world-space direction on XZ.
        Vector3 desiredDir = GetWorldMoveDirection(moveInput);
        Vector3 desiredVel = desiredDir * moveSpeed;

        Vector3 v = rb.linearVelocity;
        Vector3 planarVel = new Vector3(v.x, 0f, v.z);

        // Choose accel/decel depending on input.
        float rate = hasInput ? acceleration : deceleration;

        // Target planar velocity (0 when no input).
        Vector3 targetPlanarVel = hasInput ? desiredVel : Vector3.zero;

        // Compute needed change, clamp by rate.
        Vector3 delta = targetPlanarVel - planarVel;
        float maxDelta = rate * Time.fixedDeltaTime;
        if (delta.magnitude > maxDelta)
            delta = delta.normalized * maxDelta;

        // Apply velocity change (mass-independent).
        rb.AddForce(new Vector3(delta.x, 0f, delta.z), ForceMode.VelocityChange);

        if (!preserveYVelocity)
        {
            // If you want strictly top-down no-gravity characters.
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        }
    }

    private void RotateFixed()
    {
        Vector3 targetDir = Vector3.zero;

        if (lookAtPosition.HasValue)
        {
            targetDir = lookAtPosition.Value - rb.position;
        }
        else if (isDragging && draggingTarget != null)
        {
            targetDir = draggingTarget.position - rb.position;
        }
        else if (hasInput && moveInput.magnitude >= rotateDeadzone)
        {
            targetDir = GetWorldMoveDirection(moveInput);
        }

        targetDir.y = 0f;

        if (targetDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(targetDir, Vector3.up);
            Quaternion newRot = Quaternion.RotateTowards(rb.rotation, targetRot, turnSpeedDeg * Time.fixedDeltaTime);
            rb.MoveRotation(newRot);
        }
    }

    private Vector3 GetWorldMoveDirection(Vector2 input)
    {
        Vector3 dir = new Vector3(input.x, 0f, input.y);

        if (relativeTo == null)
            return dir.normalized;

        // Camera-relative (or any transform): project onto XZ plane.
        Vector3 fwd = relativeTo.forward; fwd.y = 0f; fwd.Normalize();
        Vector3 right = relativeTo.right;  right.y = 0f; right.Normalize();

        Vector3 world = right * input.x + fwd * input.y;
        return world.sqrMagnitude > 0.0001f ? world.normalized : Vector3.zero;
    }
}
