// PistolItem.cs
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PistolItem : AbstractCarryItem
{
    protected override void OnEquipAnim(EntityAnimation anim)
    {
        anim.SetGunState(true);
    }

    protected override void OnUnequipAnim(EntityAnimation anim)
    {
        anim.SetGunState(false);
    }

    // Future pistol logic (Shoot, Reload, etc.)
}
