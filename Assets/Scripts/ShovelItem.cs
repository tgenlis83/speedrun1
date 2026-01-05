// ShovelItem.cs
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ShovelItem : MonoBehaviour, IItem
{
    [SerializeField] private string displayName = "Shovel";

    public string DisplayName => displayName;
    [SerializeField] private Transform itemAttachPointOrigin;

    public void OnEquip(PlayerInventory owner)
    {
        // Parent to attach point for visuals.
        if (owner != null && owner.AttachPoint != null)
        {
            transform.SetParent(owner.AttachPoint, false);
            transform.localPosition = itemAttachPointOrigin.localPosition;
            transform.localRotation = itemAttachPointOrigin.localRotation;
        }
    }

    public void OnUnequip(PlayerInventory owner)
    {
        // For now just disable; later you can drop it instead.
        transform.SetParent(null);
    }
}
