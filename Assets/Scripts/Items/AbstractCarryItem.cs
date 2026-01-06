using UnityEngine;

[DisallowMultipleComponent]
public abstract class AbstractCarryItem : MonoBehaviour, IItem
{
    [SerializeField] protected string displayName = "Item";
    [SerializeField] protected Transform gripOffset;
    [Header("On Equip Logic")]
    [SerializeField] protected Collider[] collidersToDisable;
    [SerializeField] protected MonoBehaviour[] componentsToDisable;

    public string DisplayName => displayName;

    // --- IInteractable ---
    public virtual bool CanInteract(PlayerInteractor interactor) 
        => interactor != null && interactor.Inventory != null;

    public virtual void Interact(PlayerInteractor interactor)
    {
        if (!CanInteract(interactor)) return;

        var anim = interactor.GetComponent<EntityAnimation>();
        if (anim != null) anim.TriggerPick();

        interactor.Inventory.Equip(this);
    }

    public virtual void OnUse(PlayerInventory owner)
    {
        // Default: do nothing
    }

    // --- IItem ---
    public virtual void OnEquip(PlayerInventory owner)
    {
        // 1. Parent to hand
        if (owner.AttachPoint != null)
        {
            transform.SetParent(owner.AttachPoint);
            
            if (gripOffset != null)
            {
                transform.localPosition = gripOffset.localPosition;
                transform.localRotation = gripOffset.localRotation;
            }
            else
            {
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
        }
        else
        {
            transform.SetParent(owner.transform);
        }

        // 2. Disable physics/colliders
        var rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true; 
        
        if (collidersToDisable != null)
        {
            foreach (var col in collidersToDisable)
                if (col != null) col.enabled = false;
        }

        if (componentsToDisable != null)
        {
            foreach (var comp in componentsToDisable)
                if (comp != null) comp.enabled = false;
        }

        // 3. Animation hook
        var anim = owner.GetComponent<EntityAnimation>();
        if (anim != null) OnEquipAnim(anim);
    }

    public virtual void OnUnequip(PlayerInventory owner, bool isSwapping = false)
    {
        // 1. Unparent
        transform.SetParent(null);
        
        // 2. Enable physics
        var rb = GetComponent<Rigidbody>();
        if (rb != null) 
        {
            rb.isKinematic = false;
        }
        
        if (collidersToDisable != null)
        {
            foreach (var col in collidersToDisable)
                if (col != null) col.enabled = true;
        }

        if (componentsToDisable != null)
        {
            foreach (var comp in componentsToDisable)
                if (comp != null) comp.enabled = true;
        }

        // 3. Animation hook & throw
        var anim = owner.GetComponent<EntityAnimation>();
        if (anim != null) 
        {
            OnUnequipAnim(anim);
            if (!isSwapping) anim.TriggerThrow();
        }
    }

    protected abstract void OnEquipAnim(EntityAnimation anim);
    protected abstract void OnUnequipAnim(EntityAnimation anim);
}
