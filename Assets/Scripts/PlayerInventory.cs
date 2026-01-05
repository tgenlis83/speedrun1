// PlayerInventory.cs
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerInventory : MonoBehaviour
{
    public IItem CurrentItem { get; private set; }

    [SerializeField] private Transform itemAttachPoint;

    public bool HasItem => CurrentItem != null;
    public Transform AttachPoint => itemAttachPoint;

    public void Equip(IItem item)
    {
        if (item == null) return;

        if (CurrentItem != null)
            CurrentItem.OnUnequip(this);

        CurrentItem = item;
        CurrentItem.OnEquip(this);
    }

    public void Clear()
    {
        if (CurrentItem != null)
            CurrentItem.OnUnequip(this);

        CurrentItem = null;
    }

    public bool HasItemOfType<T>() where T : class, IItem
        => CurrentItem is T;
}
