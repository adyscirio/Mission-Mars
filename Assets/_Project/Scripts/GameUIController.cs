using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    // ==========================================
    // UI TEXT REFERENCES
    // ==========================================

    [Header("UI Text References")]

    [SerializeField]
    private TextMeshProUGUI txtSol;

    [SerializeField]
    private TextMeshProUGUI txtEnergy;

    [SerializeField]
    private TextMeshProUGUI txtLifeSupport;

    [SerializeField]
    private TextMeshProUGUI txtShield;

    [SerializeField]
    private TextMeshProUGUI txtMissionStatus;


    // ==========================================
    // BUTTON REFERENCES
    // ==========================================

    [Header("Button References")]

    [SerializeField]
    private Button btnNextSol;


    // ==========================================
    // MISSION END PANEL
    // ==========================================

    [Header("Mission End Panel")]

    [SerializeField]
    private GameObject panelMissionEnd;

    [SerializeField]
    private TextMeshProUGUI txtFinalTitle;

    [SerializeField]
    private TextMeshProUGUI txtFinalDescription;

    [SerializeField]
    private TextMeshProUGUI txtFinalEnergy;

    [SerializeField]
    private TextMeshProUGUI txtFinalLifeSupport;

    [SerializeField]
    private TextMeshProUGUI txtFinalShield;

    [SerializeField]
    private Button btnRestartMission;


    // ==========================================
    // RESOURCE COLORS
    // ==========================================

    [Header("Resource Colors")]

    [SerializeField]
    private Color normalColor = Color.white;

    [SerializeField]
    private Color warningColor = Color.yellow;

    [SerializeField]
    private Color criticalColor = Color.red;


    // ==========================================
    // WARNING THRESHOLDS
    // ==========================================

    [Header("Warning Thresholds")]

    [SerializeField, Range(0f, 100f)]
    private float warningThreshold = 50f;

    [SerializeField, Range(0f, 100f)]
    private float criticalThreshold = 25f;


    // ==========================================
    // MANAGER REFERENCES
    // ==========================================

    private ResourceManager resourceManager;


    // ==========================================
    // INITIALIZATION
    // ==========================================

    private void Start()
    {
        // Hide the mission end panel at startup
        if (panelMissionEnd != null)
        {
            panelMissionEnd.SetActive(false);
        }

        // Get the ResourceManager instance
        resourceManager = ResourceManager.Instance;

        // Register restart button
        if (btnRestartMission != null)
        {
            btnRestartMission.onClick.AddListener(
                RestartMission
            );
        }

        if (resourceManager == null)
        {
            Debug.LogError(
                "GameUIController: ResourceManager was not found."
            );

            if (btnNextSol != null)
            {
                btnNextSol.interactable = false;
            }

            return;
        }

        // Subscribe to resource updates
        resourceManager.OnStatsUpdated += UpdateUI;

        // Subscribe to mission completion
        resourceManager.OnMissionEnded += HandleMissionEnded;

        // Register Next Sol button
        if (btnNextSol != null)
        {
            btnNextSol.onClick.AddListener(
                OnAdvanceSolClicked
            );
        }

        // Clear initial mission status
        if (txtMissionStatus != null)
        {
            txtMissionStatus.text = "";
            txtMissionStatus.color = normalColor;
        }

        // Display initial resource values
        UpdateUI();
    }


    // ==========================================
    // UPDATE UI
    // ==========================================

    private void UpdateUI()
    {
        if (resourceManager == null)
        {
            return;
        }

        // Update current sol
        if (txtSol != null)
        {
            txtSol.text =
                $"Sol: {resourceManager.CurrentSol} / " +
                $"{resourceManager.MaxSoles}";
        }

        // Update energy
        UpdateResourceText(
            txtEnergy,
            "Energy",
            resourceManager.Energy
        );

        // Update life support
        UpdateResourceText(
            txtLifeSupport,
            "Life Support",
            resourceManager.LifeSupport
        );

        // Update shield
        UpdateResourceText(
            txtShield,
            "Shield",
            resourceManager.ShieldIntegrity
        );

        // MarsEventManager controls the Next Sol button
        // while an event or result panel is open.
    }


    // ==========================================
    // UPDATE RESOURCE TEXT AND COLOR
    // ==========================================

    private void UpdateResourceText(
        TextMeshProUGUI textComponent,
        string resourceName,
        float value)
    {
        if (textComponent == null)
        {
            return;
        }

        textComponent.text =
            $"{resourceName}: {Mathf.RoundToInt(value)}%";

        if (value <= criticalThreshold)
        {
            textComponent.color = criticalColor;
        }
        else if (value <= warningThreshold)
        {
            textComponent.color = warningColor;
        }
        else
        {
            textComponent.color = normalColor;
        }
    }


    // ==========================================
    // ADVANCE SOL
    // ==========================================

    private void OnAdvanceSolClicked()
    {
        if (resourceManager == null)
        {
            Debug.LogWarning(
                "Cannot advance: ResourceManager was not found."
            );

            return;
        }

        // Prevent advancing after mission completion
        if (resourceManager.IsMissionOver)
        {
            return;
        }

        // Prevent advancing while the button is locked
        if (btnNextSol == null ||
            !btnNextSol.interactable)
        {
            return;
        }

        // Advance to the next sol
        resourceManager.AdvanceSol();
    }


    // ==========================================
    // MISSION COMPLETION
    // ==========================================

    private void HandleMissionEnded(
        bool success,
        string message)
    {
        if (resourceManager == null)
        {
            return;
        }

        // Update original mission status
        if (txtMissionStatus != null)
        {
            txtMissionStatus.text = message;

            txtMissionStatus.color =
                success ? Color.green : Color.red;
        }

        // Disable Next Sol button
        if (btnNextSol != null)
        {
            btnNextSol.interactable = false;
        }

        // Update final title
        if (txtFinalTitle != null)
        {
            txtFinalTitle.text = success
                ? "MISSION SUCCESS"
                : "MISSION FAILED";

            txtFinalTitle.color = success
                ? new Color(0.45f, 0.90f, 0.68f)
                : new Color(1f, 0.40f, 0.40f);
        }

        // Update final description
        if (txtFinalDescription != null)
        {
            if (success)
            {
                txtFinalDescription.text =
                    "Congratulations, Commander! " +
                    "Your crew has successfully survived " +
                    $"{resourceManager.MaxSoles} sols on Mars. " +
                    "The mission has been completed.";
            }
            else
            {
                txtFinalDescription.text =
                    "MISSION ALERT!\n" +
                    message + "\n" +
                    "The crew can no longer safely continue " +
                    "the mission.";
            }
        }

        // Update final energy
        UpdateResourceText(
            txtFinalEnergy,
            "Energy",
            resourceManager.Energy
        );

        // Update final life support
        UpdateResourceText(
            txtFinalLifeSupport,
            "Life Support",
            resourceManager.LifeSupport
        );

        // Update final shield
        UpdateResourceText(
            txtFinalShield,
            "Shield",
            resourceManager.ShieldIntegrity
        );

        // Show final mission panel
        if (panelMissionEnd != null)
        {
            panelMissionEnd.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "GameUIController: Mission End Panel is not assigned."
            );
        }

        Debug.Log(
            success
                ? "Mars Mission: Mission completed."
                : "Mars Mission: Mission failed."
        );
    }


    // ==========================================
    // RESTART MISSION
    // ==========================================

    private void RestartMission()
    {
        Debug.Log(
            "Mars Mission: Restarting mission."
        );

        Scene currentScene =
            SceneManager.GetActiveScene();

        if (currentScene.buildIndex < 0)
        {
            Debug.LogError(
                "Cannot restart: Add the current scene " +
                "to the Build Profiles Scene List."
            );

            return;
        }

        SceneManager.LoadScene(
            currentScene.buildIndex
        );
    }


    // ==========================================
    // CLEANUP
    // ==========================================

    private void OnDestroy()
    {
        // Remove Next Sol button listener
        if (btnNextSol != null)
        {
            btnNextSol.onClick.RemoveListener(
                OnAdvanceSolClicked
            );
        }

        // Remove Restart Mission button listener
        if (btnRestartMission != null)
        {
            btnRestartMission.onClick.RemoveListener(
                RestartMission
            );
        }

        // Unsubscribe from ResourceManager events
        if (resourceManager != null)
        {
            resourceManager.OnStatsUpdated -= UpdateUI;

            resourceManager.OnMissionEnded -=
                HandleMissionEnded;
        }
    }
}
