// DigSite.cs
using UnityEngine;

[DisallowMultipleComponent]
public sealed class DigSite : MonoBehaviour, IInteractable
{
    [Header("Dig")]
    [SerializeField, Min(0.1f)] private float digDuration = 4.0f;

    [Header("Requirements")]
    [SerializeField] private bool requiresShovel = true;

    [Header("Optional")]
    [SerializeField] private GameObject digVfx; // enable while digging if you have it

    public event System.Action OnDigStarted;
    public event System.Action OnDigCompleted;

    private float progress;
    private bool started;
    private bool completed;
    private PlayerInteractor currentDigger;

    // Optional hook for GameManager/UI
    public void SetActiveStage(GameStage stage)
    {
        // You could enable/disable hints here.
    }

    public bool CanInteract(PlayerInteractor interactor)
    {
        if (completed) return false;
        if (interactor == null || interactor.Inventory == null) return false;

        if (requiresShovel && !interactor.Inventory.HasItem<ShovelItem>())
            return false;

        return true;
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (!CanInteract(interactor)) return;

        currentDigger = interactor;

        if (!started)
        {
            started = true;
            OnDigStarted?.Invoke();
        }

        if (digVfx != null)
        {
            digVfx.SetActive(true);
            var anim = interactor.GetComponent<EntityAnimation>();
            if (anim != null) anim.SetToolState(true);
        }
    }

    private void StopDigging()
    {
        if (digVfx != null) digVfx.SetActive(false);
        var anim = currentDigger.GetComponent<EntityAnimation>();
        if (anim != null) anim.SetToolState(false);
    }

    private void Update()
    {
        if (completed) return;
        if (currentDigger == null) return;

        // Must hold interact to keep digging (good tension).
        if (!currentDigger.InteractHeld)
        {
            StopDigging();
            return;
        }

        if (requiresShovel && (currentDigger.Inventory == null || !currentDigger.Inventory.HasItem<ShovelItem>()))
        {
            StopDigging();
            return;
        }

        progress += Time.deltaTime;

        if (progress >= digDuration)
        {
            completed = true;
            StopDigging();
            OnDigCompleted?.Invoke();
        }
    }
}
