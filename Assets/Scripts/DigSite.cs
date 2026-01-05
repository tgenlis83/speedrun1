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

        if (requiresShovel && !interactor.Inventory.HasItemOfType<ShovelItem>())
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

        if (digVfx != null) digVfx.SetActive(true);
    }

    private void Update()
    {
        if (completed) return;
        if (currentDigger == null) return;

        // Must hold interact to keep digging (good tension).
        if (!currentDigger.InteractHeld)
        {
            if (digVfx != null) digVfx.SetActive(false);
            return;
        }

        if (requiresShovel && (currentDigger.Inventory == null || !currentDigger.Inventory.HasItemOfType<ShovelItem>()))
        {
            if (digVfx != null) digVfx.SetActive(false);
            return;
        }

        progress += Time.deltaTime;

        if (progress >= digDuration)
        {
            completed = true;
            if (digVfx != null) digVfx.SetActive(false);
            OnDigCompleted?.Invoke();
        }
    }
}
