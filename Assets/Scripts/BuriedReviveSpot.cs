// BuriedReviveSpot.cs
using UnityEngine;

/// <summary>
/// Interactable revive spot created when a player becomes Buried.
/// Requires a shovel to dig them out.
/// </summary>
[DisallowMultipleComponent]
public sealed class BuriedReviveSpot : MonoBehaviour, IInteractable
{
    [SerializeField, Min(0.1f)] private float digTime = 2.0f;

    private PlayerHealth buriedPlayer;
    private float progress;
    private PlayerInteractor currentDigger;

    public static BuriedReviveSpot CreateAt(Vector3 pos, PlayerHealth buried)
    {
        var go = new GameObject("BuriedReviveSpot");
        go.transform.position = pos;

        var spot = go.AddComponent<BuriedReviveSpot>();
        var sphere = go.AddComponent<SphereCollider>();
        sphere.isTrigger = true;
        sphere.radius = 1.25f;

        spot.buriedPlayer = buried;
        return spot;
    }

    public bool CanInteract(PlayerInteractor interactor)
    {
        if (buriedPlayer == null) return false;
        if (buriedPlayer.State != PlayerHealth.LifeState.Buried) return false;
        return interactor != null && interactor.Inventory != null && interactor.Inventory.HasItemOfType<ShovelItem>();
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (!CanInteract(interactor)) return;
        currentDigger = interactor;
    }

    private void Update()
    {
        if (buriedPlayer == null || buriedPlayer.State != PlayerHealth.LifeState.Buried)
            return;

        if (currentDigger == null || !currentDigger.InteractHeld)
            return;

        // Must keep shovel equipped.
        if (currentDigger.Inventory == null || !currentDigger.Inventory.HasItemOfType<ShovelItem>())
            return;

        progress += Time.deltaTime;
        if (progress >= digTime)
            buriedPlayer.Revive(0.5f);
    }
}
