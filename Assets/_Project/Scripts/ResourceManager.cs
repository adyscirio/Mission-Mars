using System;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    // Global instance of the resource manager
    public static ResourceManager Instance { get; private set; }

    [Header("Mission Configuration")]
    [SerializeField, Min(1)]
    private int maxSoles = 8;

    public int MaxSoles => maxSoles;
    public int CurrentSol { get; private set; } = 1;

    [Header("Initial Resources (0 to 100)")]
    [SerializeField, Range(0f, 100f)]
    private float energy = 100f;

    [SerializeField, Range(0f, 100f)]
    private float lifeSupport = 100f;

    [SerializeField, Range(0f, 100f)]
    private float shieldIntegrity = 100f;

    // Read-only properties for other scripts
    public float Energy => energy;
    public float LifeSupport => lifeSupport;
    public float ShieldIntegrity => shieldIntegrity;
    public float Shield => shieldIntegrity; // Alias for GameUIController compatibility

    [Header("Resource Consumption per Sol")]
    [SerializeField, Min(0f)]
    private float energyConsumption = 5f;

    [SerializeField, Min(0f)]
    private float lifeSupportConsumption = 4f;

    [SerializeField, Min(0f)]
    private float shieldConsumption = 3f;

    // Events to update the UI
    public event Action OnStatsUpdated;
    public event Action<bool, string> OnMissionEnded;

    // Current mission state
    private bool missionOver = false;

    public bool IsMissionOver => missionOver;

    // --------------------------------------------------
    // INITIALIZATION
    // --------------------------------------------------

    private void Awake()
    {
        // Prevent duplicate resource managers
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        maxSoles = Mathf.Max(1, maxSoles);
        ClampResources();
    }

    private void Start()
    {
        // Notify initial values
        NotifyStatsChanged();
    }

    // --------------------------------------------------
    // ADVANCE TO NEXT SOL
    // --------------------------------------------------

    public void AdvanceSol()
    {
        // Do not advance if the mission has ended
        if (missionOver)
        {
            return;
        }

        // Daily resource consumption
        energy -= energyConsumption;
        lifeSupport -= lifeSupportConsumption;
        shieldIntegrity -= shieldConsumption;

        // Keep resources between 0 and 100
        ClampResources();

        // Update UI indicators
        NotifyStatsChanged();

        // Check if any resource hit zero
        if (CheckMissionFailure())
        {
            return;
        }

        // Check if we completed the final sol (Sol 8)
        if (CurrentSol >= maxSoles)
        {
            EndMission(
                true,
                $"MISSION COMPLETE! Crew survived {maxSoles} sols on Mars."
            );
            return;
        }

        // Start a new sol
        CurrentSol++;

        // Update sol counter
        NotifyStatsChanged();
    }

    // --------------------------------------------------
    // APPLY EVENT DECISIONS
    // --------------------------------------------------

    public void ApplyEventChoice(
        float energyDelta,
        float lifeSupportDelta,
        float shieldDelta)
    {
        // Do not modify resources once finished
        if (missionOver)
        {
            return;
        }

        // Apply decision consequences
        energy += energyDelta;
        lifeSupport += lifeSupportDelta;
        shieldIntegrity += shieldDelta;

        // Clamp resources between 0 and 100
        ClampResources();

        // Update the UI
        NotifyStatsChanged();

        // Check if the decision caused failure
        CheckMissionFailure();
    }

    // --------------------------------------------------
    // CLAMP LIMITS
    // --------------------------------------------------

    private void ClampResources()
    {
        energy = Mathf.Clamp(energy, 0f, 100f);
        lifeSupport = Mathf.Clamp(lifeSupport, 0f, 100f);
        shieldIntegrity = Mathf.Clamp(shieldIntegrity, 0f, 100f);
    }

    // --------------------------------------------------
    // CHECK FAILURE
    // --------------------------------------------------

    private bool CheckMissionFailure()
    {
        if (energy <= 0f ||
            lifeSupport <= 0f ||
            shieldIntegrity <= 0f)
        {
            EndMission(
                false,
                "MISSION FAILED! Base resources collapsed."
            );
            return true;
        }

        return false;
    }

    // --------------------------------------------------
    // NOTIFY RESOURCE CHANGES
    // --------------------------------------------------

    private void NotifyStatsChanged()
    {
        OnStatsUpdated?.Invoke();
    }

    // --------------------------------------------------
    // END MISSION
    // --------------------------------------------------

    private void EndMission(bool success, string message)
    {
        // Prevent ending the mission multiple times
        if (missionOver)
        {
            return;
        }

        missionOver = true;

        // Notify victory or defeat
        OnMissionEnded?.Invoke(success, message);
    }

    // --------------------------------------------------
    // INSTANCE CLEANUP
    // --------------------------------------------------

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}