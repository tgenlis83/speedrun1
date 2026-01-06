using UnityEngine;

/// <summary>
/// Top-down follow camera (no smoothing).
/// - Camera position follows target + offset instantly.
/// - Camera rotation always looks at the target.
/// - Rotation is constrained by an optional world-up.
/// </summary>
[DisallowMultipleComponent]
public sealed class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow")]
    [Tooltip("World-space offset from the target. Example: (0, 15, -10) for angled top-down.")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 15f, -10f);

    [Header("Look At")]
    [Tooltip("Point on the target to look at in local-space (e.g., (0, 1.2, 0) to look at chest height).")]
    [SerializeField] private Vector3 lookAtLocalOffset = new Vector3(0f, 0.5f, 0f);

    [Tooltip("World up used for look rotation. Usually Vector3.up.")]
    [SerializeField] private Vector3 worldUp = Vector3.up;

    [Header("Axis Locks")]
    [Tooltip("Ignore target movement on X (camera keeps its own X).")]
    [SerializeField] private bool lockX = false;

    [Tooltip("Ignore target movement on Y (camera keeps its own Y). Usually true for top-down height.")]
    [SerializeField] private bool lockY = true;

    [Tooltip("Ignore target movement on Z (camera keeps its own Z).")]
    [SerializeField] private bool lockZ = false;

    [Header("Bounds (Optional)")]
    [Tooltip("If assigned, camera will be clamped inside this collider's bounds (use a BoxCollider for your level).")]
    [SerializeField] private Collider boundsCollider;

    [Tooltip("Extra padding applied when clamping to bounds.")]
    [SerializeField] private Vector3 boundsPadding = Vector3.zero;

    [Header("Snapping (Optional)")]
    [Tooltip("If true, camera position snaps to a grid (useful for crisp pixel movement).")]
    [SerializeField] private bool snapPosition = false;

    [Tooltip("Grid size used when Snap Position is enabled. 0.01 is a common value.")]
    [Min(0.0001f)]
    [SerializeField] private float snapGridSize = 0.01f;

    [Header("Update Mode")]
    [Tooltip("LateUpdate is typical so the camera follows after movement. FixedUpdate if you move with physics.")]
    [SerializeField] private UpdateMode updateMode = UpdateMode.LateUpdate;

    public enum UpdateMode { Update, LateUpdate, FixedUpdate }

    public Transform Target
    {
        get => target;
        set => target = value;
    }

    public Vector3 Offset
    {
        get => offset;
        set => offset = value;
    }

    private void Update()
    {
        if (updateMode == UpdateMode.Update) FollowAndLookNow();
    }

    private void LateUpdate()
    {
        if (updateMode == UpdateMode.LateUpdate) FollowAndLookNow();
    }

    private void FixedUpdate()
    {
        if (updateMode == UpdateMode.FixedUpdate) FollowAndLookNow();
    }

    private void FollowAndLookNow()
    {
        if (target == null) return;

        // 1) Position
        Vector3 desiredPos = target.position + offset;

        // Axis locks: keep current axis values if locked.
        Vector3 current = transform.position;
        if (lockX) desiredPos.x = current.x;
        if (lockY) desiredPos.y = current.y;
        if (lockZ) desiredPos.z = current.z;

        // Optional bounds clamp.
        if (boundsCollider != null)
        {
            Bounds b = boundsCollider.bounds;

            // Apply padding by shrinking bounds.
            b.Expand(new Vector3(-boundsPadding.x * 2f, -boundsPadding.y * 2f, -boundsPadding.z * 2f));

            desiredPos.x = Mathf.Clamp(desiredPos.x, b.min.x, b.max.x);
            desiredPos.y = Mathf.Clamp(desiredPos.y, b.min.y, b.max.y);
            desiredPos.z = Mathf.Clamp(desiredPos.z, b.min.z, b.max.z);
        }

        // Optional snapping.
        if (snapPosition)
            desiredPos = Snap(desiredPos, snapGridSize);

        transform.position = desiredPos;

        // 2) Rotation (always look at player)
        Vector3 lookAtWorld = target.TransformPoint(lookAtLocalOffset);
        Vector3 toTarget = lookAtWorld - transform.position;

        // Avoid NaNs if camera is exactly at look point.
        if (toTarget.sqrMagnitude > 0.000001f)
            transform.rotation = Quaternion.LookRotation(toTarget, worldUp);
    }

    private static Vector3 Snap(Vector3 v, float grid)
    {
        float inv = 1f / grid;
        v.x = Mathf.Round(v.x * inv) / inv;
        v.y = Mathf.Round(v.y * inv) / inv;
        v.z = Mathf.Round(v.z * inv) / inv;
        return v;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (target == null) return;

        Gizmos.color = Color.cyan;

        Vector3 desiredPos = target.position + offset;
        Gizmos.DrawWireSphere(desiredPos, 0.25f);

        Vector3 lookAtWorld = target.TransformPoint(lookAtLocalOffset);
        Gizmos.DrawLine(desiredPos, lookAtWorld);
        Gizmos.DrawWireSphere(lookAtWorld, 0.15f);

        if (boundsCollider != null)
        {
            Gizmos.color = Color.yellow;
            Bounds b = boundsCollider.bounds;
            b.Expand(new Vector3(-boundsPadding.x * 2f, -boundsPadding.y * 2f, -boundsPadding.z * 2f));
            Gizmos.DrawWireCube(b.center, b.size);
        }
    }
#endif
}
