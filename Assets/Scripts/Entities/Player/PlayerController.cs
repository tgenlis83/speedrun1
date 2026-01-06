// PlayerController.cs
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Reads WASD via the New Input System and feeds PlayerMovement.
/// Input is read in Update and applied in FixedUpdate for stable Rigidbody motion.
/// </summary>
[DisallowMultipleComponent]
public sealed class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EntityMovement movement;

    [Header("Input System")]
    [SerializeField] private InputActionReference moveAction;

    [Header("Reference Frame (Optional)")]
    [Tooltip("If set, movement is relative to this transform (e.g., main camera).")]
    [SerializeField] private Transform relativeTo;

    private Vector2 cachedInput;

    private void Reset()
    {
        movement = GetComponent<EntityMovement>();
    }

    private void Awake()
    {
        if (movement == null)
            movement = GetComponent<EntityMovement>();

        if (movement != null)
            movement.RelativeTo = relativeTo;
    }

    private void OnEnable()
    {
        if (moveAction != null)
            moveAction.action.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null)
            moveAction.action.Disable();
    }

    private void Update()
    {
        if (movement != null)
            movement.RelativeTo = relativeTo;

        if (moveAction != null)
        {
            cachedInput = moveAction.action.ReadValue<Vector2>();
            cachedInput = Vector2.ClampMagnitude(cachedInput, 1f);
        }
    }

    private void FixedUpdate()
    {
        if (movement == null)
        {
            return;
        }
        movement.SetMoveInput(cachedInput);
    }
}
