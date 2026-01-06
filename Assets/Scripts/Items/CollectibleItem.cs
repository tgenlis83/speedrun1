// CollectibleItem.cs
using UnityEngine;

/// <summary>
/// Pickup for collectible items - items that simply disappear when picked up.
/// </summary>
[DisallowMultipleComponent]
public sealed class CollectibleItem : MonoBehaviour, IItem
{
    [SerializeField] private string displayName = "Collectible";

    public string DisplayName => displayName;

    public bool CanInteract(PlayerInteractor interactor)
    {
        return interactor != null && interactor.Inventory != null;
    }

    public void OnUse(PlayerInventory owner)
    {
        // Default: do nothing
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (CanInteract(interactor))
        {
            interactor.Inventory.Collect(this);
        }
    }

    public void OnEquip(PlayerInventory owner)
    {
        gameObject.SetActive(false);
    }

    public void OnUnequip(PlayerInventory owner, bool isSwapping = false)
    {
        gameObject.SetActive(true);
        transform.SetParent(null);
    }
}
