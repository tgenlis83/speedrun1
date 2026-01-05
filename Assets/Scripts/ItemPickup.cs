// ItemPickup.cs
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// World pickup for items. For now: shovel only.
/// Later: extend to item database, rarity, etc.
/// </summary>
[DisallowMultipleComponent]
public sealed class ItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private MonoBehaviour itemComponent; // must implement IItem
    [SerializeField] private MonoBehaviour[] componentsToDisable;
    [SerializeField] private Collider[] collidersToDisable;

    public bool CanInteract(PlayerInteractor interactor)
    {
        if (interactor == null || interactor.Inventory == null) return false;
        return itemComponent is IItem;
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (!CanInteract(interactor)) return;

        var item = itemComponent as IItem;
        interactor.Inventory.Equip(item);

        // Disable specified components
        foreach (var component in componentsToDisable)
        {
            if (component != null) component.enabled = false;
        }
        foreach (var collider in collidersToDisable)
        {
            if (collider != null) collider.enabled = false;
        }

        // Remove pickup from world (or disable).
        gameObject.SetActive(false);
    }
}
