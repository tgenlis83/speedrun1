// GameManager.cs
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class GameManager : MonoBehaviour
{
    [Header("Stage References")]
    [SerializeField] private DigSite digSite;
    [SerializeField] private Coffin coffin;
    [SerializeField] private ExitZone exitZone;
    [SerializeField] private ZombieSpawner zombieSpawner;
    [SerializeField] private StageAnnouncer stageIndicator;

    [Header("Stage Tuning")]
    [SerializeField] private float locateZombieSpawnMultiplier = 0.5f;
    [SerializeField] private float digZombieSpawnMultiplier = 1.0f;
    [SerializeField] private float moveZombieSpawnMultiplier = 1.5f;

    public GameStage CurrentStage { get; private set; } = GameStage.Locate;

    public delegate void StageChanged(GameStage newStage);
    public event StageChanged OnStageChanged;

    private void Awake()
    {
        if (digSite != null) digSite.OnDigStarted += HandleDigStarted;
        if (digSite != null) digSite.OnDigCompleted += HandleDigCompleted;
        if (exitZone != null) exitZone.OnCoffinExited += HandleCoffinExited;
    }

    private void Start()
    {
        // Initial state
        if (coffin != null) coffin.SetHidden(true);
        SetStage(GameStage.Locate);
    }

    private void SetStage(GameStage stage)
    {
        CurrentStage = stage;

        // Stage behaviors
        if (zombieSpawner != null)
        {
            float mult = stage switch
            {
                GameStage.Locate => locateZombieSpawnMultiplier,
                GameStage.Dig => digZombieSpawnMultiplier,
                GameStage.Move => moveZombieSpawnMultiplier,
                _ => 0f
            };
            zombieSpawner.SetIntensity(mult);
        }

        // Optional: tell the DigSite which stage it belongs to (useful for UI text, etc.)
        if (digSite != null) digSite.SetActiveStage(stage);

        OnStageChanged?.Invoke(stage);
        Debug.Log($"[GameManager] Stage => {stage}");
        switch (stage)
        {
            case GameStage.Locate:
                stageIndicator?.SetText("Locate the dig site");
                break;
            case GameStage.Dig:
                stageIndicator?.SetText("Dip it up!");
                break;
            case GameStage.Move:
                stageIndicator?.SetText("Steal the Coffin");
                break;
            default:
                stageIndicator?.SetText("TODO");
                break;
        }
    }

    private void HandleDigStarted()
    {
        if (CurrentStage == GameStage.Locate)
            SetStage(GameStage.Dig);
    }

    private void HandleDigCompleted()
    {
        if (coffin != null) coffin.SetHidden(false);
        SetStage(GameStage.Move);
    }

    private void HandleCoffinExited(Coffin exitedCoffin)
    {
        if (CurrentStage != GameStage.Move) return;

        // Later: transition to Drive stage
        SetStage(GameStage.VictoryShop);
        Debug.Log("[GameManager] Coffin escaped! (Drive/Shop later)");
    }
}
