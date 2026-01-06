// PlayerInventory.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerInventory : MonoBehaviour
{
    public event Action<IItem> OnItemDropped;

    [SerializeField] private Transform itemAttachPoint;
    
    private readonly List<IItem> collectibles = new List<IItem>();

    public IItem CurrentCarryItem { get; private set; }
    public Transform AttachPoint => itemAttachPoint;
    public bool HasCarryItem => CurrentCarryItem != null;

    /// <summary>
    /// Equips a carry item. If an item is already held, it is swapped out.
    /// </summary>
    public void Equip(IItem item)
    {
        if (item == null) return;

        // If we are already holding something, drop it first (SWAP)
        if (CurrentCarryItem != null)
        {
            // DropHeldItem() calls OnUnequip(false) usually, we need true here.
            // So we manually unequip to trigger swap.
            DropHeldItem(isSwapping: true);
        }

        CurrentCarryItem = item;
        CurrentCarryItem.OnEquip(this);
    }

    /// <summary>
    /// Adds a collectible item to the inventory.
    /// </summary>
    public void Collect(IItem item)
    {
        if (item == null) return;
        collectibles.Add(item);
        item.OnEquip(this); // Collectibles might play a sound or effect on equip
    }

    /// <summary>
    /// Drops the currently held item, if any.
    /// </summary>
    public void DropHeldItem(bool isSwapping = false)
    {
        if (CurrentCarryItem == null) return;

        var item = CurrentCarryItem;
        CurrentCarryItem = null;

        item.OnUnequip(this, isSwapping);
        
        // Notify listeners (like the CarryItemPickup) that this item was dropped
        OnItemDropped?.Invoke(item);
    }

    /// <summary>
    /// Uses the currently held item, if any.
    /// </summary>
    public void UseHeldItem()
    {
        if (CurrentCarryItem == null) return;

        CurrentCarryItem.OnUse(this);
    }

    public bool HasCollectible<T>() where T : class, IItem
    {
        foreach (var item in collectibles)
        {
            if (item is T) return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if the player has a specific type of item (carry or collectible).
    /// Can optionally consume (remove) the item(s) if the required amount is found.
    /// </summary>
    public bool HasItem<T>(int amount = 1, bool consume = false) where T : class, IItem
    {
        int count = 0;
        bool hasCarry = CurrentCarryItem is T;
        if (hasCarry) count++;

        // Count in collectibles
        int collectibleCount = 0;
        foreach (var item in collectibles)
        {
            if (item is T) collectibleCount++;
        }
        count += collectibleCount;

        if (count >= amount)
        {
            if (consume)
            {
                int remainingToRemove = amount;

                // 1. Remove from collectibles first (prefer keeping the item in hand if possible, or maybe opposite? 
                // Usually consumables are collectibles. If holding a key, use it. Logic: remove from list first for stability)
                if (collectibleCount > 0)
                {
                    for (int i = collectibles.Count - 1; i >= 0 && remainingToRemove > 0; i--)
                    {
                        if (collectibles[i] is T item)
                        {
                            item.OnUnequip(this);
                            collectibles.RemoveAt(i);
                            remainingToRemove--;
                        }
                    }
                }

                // 2. If still need to remove, and holding the item, remove it
                if (remainingToRemove > 0 && hasCarry)
                {
                    // To "consume" a carry item, we unequip it but do NOT fire OnItemDropped.
                    // This prevents the world pickup from reappearing.
                    var item = CurrentCarryItem;
                    CurrentCarryItem = null;
                    item.OnUnequip(this);
                    remainingToRemove--;
                    
                    // We might not be able to clean up the CarryItemPickup listener easily here 
                    // without an OnItemConsumed event, but for now this fits the requirement to "delete".
                }
            }
            return true;
        }

        return false;
    }
}
