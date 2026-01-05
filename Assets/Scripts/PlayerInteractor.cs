// PlayerInteractor.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public sealed class PlayerInteractor : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerInventory inventory;

    [Header("Input System")]
    [SerializeField] private InputActionReference interactAction;

    [Header("Scan")]
    [SerializeField, Min(0.25f)] private float interactRadius = 1.6f;
    [SerializeField] private LayerMask interactMask = ~0;

    public PlayerInventory Inventory => inventory;
    public bool InteractHeld { get; private set; }

    private readonly Collider[] hits = new Collider[16];

    private void Reset()
    {
        inventory = GetComponent<PlayerInventory>();
    }

    private void OnEnable()
    {
        if (interactAction != null) interactAction.action.Enable();
    }

    private void OnDisable()
    {
        if (interactAction != null) interactAction.action.Disable();
    }

    private void Update()
    {
        InteractHeld = interactAction != null && interactAction.action.IsPressed();

        if (interactAction != null && interactAction.action.WasPressedThisFrame())
        {
            IInteractable best = FindBestInteractable();
            if (best != null && best.CanInteract(this))
                best.Interact(this);
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
