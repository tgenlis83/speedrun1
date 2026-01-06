// PlayerInteractor.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public sealed class PlayerInteractor : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private EntityMovement movement;

    [Header("Input System")]
    [SerializeField] private InputActionReference interactInput;
    [SerializeField] private InputActionReference useInput;
    [SerializeField] private InputActionReference dropInput;

    [Header("Scan")]
    [SerializeField, Min(0.25f)] private float interactRadius = 1.0f;
    [SerializeField] private LayerMask interactMask = ~0;

    public PlayerInventory Inventory => inventory;
    public bool InteractHeld { get; private set; }

    private readonly Collider[] hits = new Collider[16];

    private void Reset()
    {
        inventory = GetComponent<PlayerInventory>();
        movement = GetComponent<EntityMovement>();
    }

    private void OnEnable()
    {
        if (interactInput != null) interactInput.action.Enable();
        if (dropInput != null) dropInput.action.Enable();
        if (useInput != null) useInput.action.Enable();
    }

    private void OnDisable()
    {
        if (interactInput != null) interactInput.action.Disable();
        if (dropInput != null) dropInput.action.Disable();
        if (useInput != null) useInput.action.Disable();
    }

    private void Update()
    {
        HandleInteraction();
        HandleDropping();
        HandleUse();
    }

    private void HandleInteraction()
    {
        InteractHeld = interactInput != null && interactInput.action.IsPressed();

        if (interactInput != null && interactInput.action.WasPressedThisFrame())
        {
            IInteractable best = FindBestInteractable();
            if (best != null && best.CanInteract(this))
            {
                if (movement != null && movement.IsDragging)
                {
                    if (!(best is Coffin)) return;
                }

                best.Interact(this);
            }
        }
    }

    private void HandleUse()
    {
        if (movement != null && movement.IsDragging) return;

        if (useInput != null && useInput.action.WasPressedThisFrame())
        {
            if (inventory != null && inventory.HasCarryItem)
            {
                inventory.UseHeldItem();
            }
        }
    }

    private void HandleDropping()
    {
        if (movement != null && movement.IsDragging) return;

        if (dropInput != null && dropInput.action.WasPressedThisFrame())
        {
            if (inventory != null && inventory.HasCarryItem)
            {
                inventory.DropHeldItem();
            }
        }
    }

    private IInteractable FindBestInteractable()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, interactRadius, hits, interactMask, QueryTriggerInteraction.Collide);

        IInteractable best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            var col = hits[i];
            if (col == null) continue;

            // Look for IInteractable on collider or parents.
            var comps = ListPool<MonoBehaviour>.Get();
            col.GetComponentsInParent(true, comps);
            foreach (var mb in comps)
            {
                if (mb is IInteractable interactable)
                {
                    float d = (col.transform.position - transform.position).sqrMagnitude;
                    if (d < bestDist)
                    {
                        bestDist = d;
                        best = interactable;
                    }
                }
            }
            ListPool<MonoBehaviour>.Release(comps);
        }

        return best;
    }

    // Tiny pooled list helper to avoid allocs.
    private static class ListPool<T>
    {
        private static readonly Stack<List<T>> pool = new Stack<List<T>>();

        public static List<T> Get()
        {
            if (pool.Count > 0) { var l = pool.Pop(); l.Clear(); return l; }
            return new List<T>(8);
        }

        public static void Release(List<T> list)
        {
            if (list == null) return;
            list.Clear();
            pool.Push(list);
        }
    }
}
