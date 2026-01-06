// IItem.cs
public interface IItem : IInteractable
{
    string DisplayName { get; }
    void OnEquip(PlayerInventory owner);
    void OnUnequip(PlayerInventory owner, bool isSwapping = false);
}
