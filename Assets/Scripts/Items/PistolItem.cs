// PistolItem.cs
using Unity.VisualScripting;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PistolItem : AbstractCarryItem
{
    [SerializeField] private ParticleSystem[] particleSystems;
    [SerializeField, Range(1f, 100f)] private float damage = 23f;

    private Transform shootDir;
    private void Awake()
    {
        if (particleSystems != null)
        {
            foreach (var ps in particleSystems)
            {
                if (ps != null) ps.Stop();
            }
        }
    }

    protected override void OnEquipAnim(EntityAnimation anim)
    {
        anim.SetGunState(true);
        shootDir = transform.GetComponentInParent<Rigidbody>().transform;
    }

    protected override void OnUnequipAnim(EntityAnimation anim)
    {
        anim.SetGunState(false);
    }

    private EntityHealth healthCache;
    public override void OnUse(PlayerInventory owner)
    {
        // find the owner's entity animation component
        EntityAnimation anim = owner.GetComponent<EntityAnimation>();
        if (anim != null) anim.TriggerShoot();
        foreach (var ps in particleSystems)
        {
            ps.Play();
        }
        // Raycast to damage entities
        if (shootDir != null)
        {
            Vector3 rayOrigin = shootDir.position + shootDir.forward * 0.5f + Vector3.up * 1f;
            if (Physics.Raycast(rayOrigin, shootDir.forward, out RaycastHit hit))
            {
                if (hit.collider.TryGetComponent(out healthCache))
                {
                    healthCache.TakeDamage(damage);
                }
            }
        }
    }

    // Future pistol logic (Shoot, Reload, etc.)
}
