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
    [SerializeField] private PlayerInventory inventory;

    [Header("Input System")]
    [SerializeField] private InputActionReference moveAction;

    [Header("Reference Frame (Optional)")]
    [Tooltip("If set, movement is relative to this transform (e.g., main camera).")]
    [SerializeField] private Transform relativeTo;

    private Vector2 cachedInput;
    private Camera mainCamera;

    private void Reset()
    {
        movement = GetComponent<EntityMovement>();
        inventory = GetComponent<PlayerInventory>();
    }

    private void Awake()
    {
        if (movement == null)
            movement = GetComponent<EntityMovement>();
        
        if (inventory == null)
            inventory = GetComponent<PlayerInventory>();

        if (movement != null)
            movement.RelativeTo = relativeTo;
            
        mainCamera = Camera.main;
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

        HandleAiming();
    }

    private void HandleAiming()
    {
        if (movement == null) return;

        bool isPistolEquipped = inventory != null && inventory.CurrentCarryItem is PistolItem;

        if (isPistolEquipped && mainCamera != null && Mouse.current != null)
        {
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(mouseScreenPos);

            // Plane at player's height to intersect with mouse ray
            Plane plane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));
            if (plane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                movement.SetLookAtPosition(hitPoint);
                return;
            }
        }

        movement.SetLookAtPosition(null);
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
