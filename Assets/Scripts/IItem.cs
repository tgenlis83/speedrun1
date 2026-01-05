// IItem.cs
public interface IItem
{
    string DisplayName { get; }
    void OnEquip(PlayerInventory owner);
    void OnUnequip(PlayerInventory owner);
}
