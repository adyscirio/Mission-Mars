using System.Collections.Generic;
using System.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MarsEventManager : MonoBehaviour
{
    // ==========================================
    // MISSION EVENTS
    // ==========================================

    [Header("Mission Events")]

    [SerializeField]
    private List<MarsEvent> missionEvents =
        new List<MarsEvent>();


    // ==========================================
    // EVENT PANEL
    // ==========================================

    [Header("Event Panel")]

    [SerializeField]
    private GameObject panelEvent;

    [SerializeField]
    private TextMeshProUGUI txtEventTitle;

    [SerializeField]
    private TextMeshProUGUI txtEventDescription;


    // ==========================================
    // DECISION BUTTONS
    // ==========================================

    [Header("Decision Buttons")]

    [SerializeField]
    private Button btnOptionA;

    [SerializeField]
    private Button btnOptionB;

    [SerializeField]
    private TextMeshProUGUI txtOptionA;

    [SerializeField]
    private TextMeshProUGUI txtOptionB;


    // ==========================================
    // EVENT RESULT PANEL
    // ==========================================

    [Header("Event Result Panel")]

    [SerializeField]
    private GameObject panelEventResult;

    [SerializeField]
    private TextMeshProUGUI txtResultTitle;

    [SerializeField]
    private TextMeshProUGUI txtResultDescription;

    [SerializeField]
    private TextMeshProUGUI txtResourceChanges;

    [SerializeField]
    private TextMeshProUGUI txtScienceFact;

    [SerializeField]
    private Button btnContinueMission;


    // ==========================================
    // NEXT SOL BUTTON
    // ==========================================

    [Header("Next Sol Button")]

    [SerializeField]
    private Button btnNextSol;


    // ==========================================
    // INTERNAL VARIABLES
    // ==========================================

    private ResourceManager resourceManager;

    private MarsEvent currentEvent;

    private readonly HashSet<MarsEvent> completedEvents =
        new HashSet<MarsEvent>();

    private int lastProcessedSol = -1;

    private bool eventActive = false;

    private bool resultActive = false;

    private bool missionFinished = false;


    // ==========================================
    // INITIALIZATION
    // ==========================================

    private void Start()
    {
        resourceManager = ResourceManager.Instance;

        if (resourceManager == null)
        {
            UnityEngine.Debug.LogError(
                "MarsEventManager: ResourceManager was not found."
            );

            enabled = false;
            return;
        }

        // Subscribe to resource manager events
        resourceManager.OnStatsUpdated += OnStatsUpdated;
        resourceManager.OnMissionEnded += OnMissionEnded;

        // Register decision buttons
        if (btnOptionA != null)
        {
            btnOptionA.onClick.AddListener(SelectOptionA);
        }

        if (btnOptionB != null)
        {
            btnOptionB.onClick.AddListener(SelectOptionB);
        }

        // Register continue button
        if (btnContinueMission != null)
        {
            btnContinueMission.onClick.AddListener(
                ContinueMission
            );
        }

        // Hide panels at startup
        if (panelEvent != null)
        {
            panelEvent.SetActive(false);
        }

        if (panelEventResult != null)
        {
            panelEventResult.SetActive(false);
        }

        // Check events for the first sol
        CheckEventsForCurrentSol();
    }


    // ==========================================
    // RESOURCE UPDATES
    // ==========================================

    private void OnStatsUpdated()
    {
        if (resourceManager == null || missionFinished)
        {
            return;
        }

        if (lastProcessedSol != resourceManager.CurrentSol)
        {
            CheckEventsForCurrentSol();
        }

        UpdateNextSolButton();
    }


    // ==========================================
    // CHECK EVENTS FOR CURRENT SOL
    // ==========================================

    private void CheckEventsForCurrentSol()
    {
        if (resourceManager == null ||
            resourceManager.IsMissionOver ||
            eventActive ||
            resultActive)
        {
            return;
        }

        lastProcessedSol = resourceManager.CurrentSol;

        foreach (MarsEvent marsEvent in missionEvents)
        {
            if (marsEvent == null)
            {
                continue;
            }

            if (completedEvents.Contains(marsEvent))
            {
                continue;
            }

            if (marsEvent.targetSol ==
                resourceManager.CurrentSol)
            {
                ShowEvent(marsEvent);
                return;
            }
        }

        UpdateNextSolButton();
    }


    // ==========================================
    // SHOW EVENT
    // ==========================================

    private void ShowEvent(MarsEvent marsEvent)
    {
        currentEvent = marsEvent;
        eventActive = true;

        if (txtEventTitle != null)
        {
            txtEventTitle.text = marsEvent.eventTitle;
        }

        if (txtEventDescription != null)
        {
            txtEventDescription.text =
                marsEvent.eventDescription;
        }

        if (txtOptionA != null)
        {
            txtOptionA.text = marsEvent.optionATitle;
        }

        if (txtOptionB != null)
        {
            txtOptionB.text = marsEvent.optionBTitle;
        }

        if (btnOptionA != null)
        {
            btnOptionA.interactable = true;
        }

        if (btnOptionB != null)
        {
            btnOptionB.interactable = true;
        }

        if (panelEvent != null)
        {
            panelEvent.SetActive(true);
        }

        UpdateNextSolButton();

        UnityEngine.Debug.Log(
            "Event started: " + marsEvent.eventTitle
        );
    }


    // ==========================================
    // OPTION A
    // ==========================================

    private void SelectOptionA()
    {
        if (!CanSelectOption())
        {
            return;
        }

        ResolveEvent(
            currentEvent.energyChangeA,
            currentEvent.lifeSupportChangeA,
            currentEvent.shieldChangeA,
            currentEvent.resultTitleA,
            currentEvent.resultDescriptionA
        );
    }


    // ==========================================
    // OPTION B
    // ==========================================

    private void SelectOptionB()
    {
        if (!CanSelectOption())
        {
            return;
        }

        ResolveEvent(
            currentEvent.energyChangeB,
            currentEvent.lifeSupportChangeB,
            currentEvent.shieldChangeB,
            currentEvent.resultTitleB,
            currentEvent.resultDescriptionB
        );
    }


    // ==========================================
    // VALIDATE DECISION
    // ==========================================

    private bool CanSelectOption()
    {
        return eventActive &&
               !resultActive &&
               !missionFinished &&
               currentEvent != null &&
               resourceManager != null &&
               !resourceManager.IsMissionOver;
    }


    // ==========================================
    // RESOLVE EVENT
    // ==========================================

    private void ResolveEvent(
        float energyDelta,
        float lifeSupportDelta,
        float shieldDelta,
        string resultTitle,
        string resultDescription)
    {
        if (!CanSelectOption())
        {
            return;
        }

        MarsEvent resolvedEvent = currentEvent;

        // Prevent selecting the same event twice
        completedEvents.Add(resolvedEvent);

        eventActive = false;
        currentEvent = null;

        // Keep next sol locked while showing results
        resultActive = true;

        if (btnOptionA != null)
        {
            btnOptionA.interactable = false;
        }

        if (btnOptionB != null)
        {
            btnOptionB.interactable = false;
        }

        UpdateNextSolButton();

        // Apply resource changes
        resourceManager.ApplyEventChoice(
            energyDelta,
            lifeSupportDelta,
            shieldDelta
        );

        // Close decision panel
        if (panelEvent != null)
        {
            panelEvent.SetActive(false);
        }

        // Mission failure may occur after applying changes
        if (resourceManager.IsMissionOver)
        {
            return;
        }

        // Display the selected decision results
        ShowEventResult(
            resolvedEvent,
            energyDelta,
            lifeSupportDelta,
            shieldDelta,
            resultTitle,
            resultDescription
        );

        UnityEngine.Debug.Log(
            "Event resolved: " + resolvedEvent.eventTitle
        );
    }


    // ==========================================
    // SHOW EVENT RESULTS
    // ==========================================

    private void ShowEventResult(
        MarsEvent resolvedEvent,
        float energyDelta,
        float lifeSupportDelta,
        float shieldDelta,
        string resultTitle,
        string resultDescription)
    {
        if (txtResultTitle != null)
        {
            txtResultTitle.text = resultTitle;
        }

        if (txtResultDescription != null)
        {
            txtResultDescription.text = resultDescription;
        }

        if (txtResourceChanges != null)
        {
            txtResourceChanges.text =
                "Energy: " + FormatChange(energyDelta) +
                "\nLife Support: " + FormatChange(lifeSupportDelta) +
                "\nShield: " + FormatChange(shieldDelta);
        }

        if (txtScienceFact != null)
        {
            txtScienceFact.text =
                "Science Fact\n" +
                resolvedEvent.educationalExplanation;
        }

        if (btnContinueMission != null)
        {
            btnContinueMission.interactable = true;
        }

        if (panelEventResult != null)
        {
            panelEventResult.SetActive(true);
        }

        UpdateNextSolButton();
    }


    // ==========================================
    // FORMAT RESOURCE CHANGES
    // ==========================================

    private string FormatChange(float value)
    {
        if (value > 0f)
        {
            return "+" + value.ToString("0.#");
        }

        return value.ToString("0.#");
    }


    // ==========================================
    // CONTINUE MISSION
    // ==========================================

    private void ContinueMission()
    {
        if (!resultActive || missionFinished)
        {
            return;
        }

        resultActive = false;

        if (panelEventResult != null)
        {
            panelEventResult.SetActive(false);
        }

        // Check for other events in the same sol
        CheckEventsForCurrentSol();

        UpdateNextSolButton();

        UnityEngine.Debug.Log(
            "Decision results closed. Mission continues."
        );
    }


    // ==========================================
    // UPDATE NEXT SOL BUTTON
    // ==========================================

    private void UpdateNextSolButton()
    {
        if (btnNextSol == null || resourceManager == null)
        {
            return;
        }

        btnNextSol.interactable =
            !eventActive &&
            !resultActive &&
            !missionFinished &&
            !resourceManager.IsMissionOver;
    }


    // ==========================================
    // MISSION ENDED
    // ==========================================

    private void OnMissionEnded(
        bool success,
        string message)
    {
        missionFinished = true;

        eventActive = false;
        resultActive = false;
        currentEvent = null;

        if (panelEvent != null)
        {
            panelEvent.SetActive(false);
        }

        if (panelEventResult != null)
        {
            panelEventResult.SetActive(false);
        }

        if (btnOptionA != null)
        {
            btnOptionA.interactable = false;
        }

        if (btnOptionB != null)
        {
            btnOptionB.interactable = false;
        }

        if (btnContinueMission != null)
        {
            btnContinueMission.interactable = false;
        }

        UpdateNextSolButton();
    }


    // ==========================================
    // CLEANUP
    // ==========================================

    private void OnDestroy()
    {
        if (btnOptionA != null)
        {
            btnOptionA.onClick.RemoveListener(SelectOptionA);
        }

        if (btnOptionB != null)
        {
            btnOptionB.onClick.RemoveListener(SelectOptionB);
        }

        if (btnContinueMission != null)
        {
            btnContinueMission.onClick.RemoveListener(
                ContinueMission
            );
        }

        if (resourceManager != null)
        {
            resourceManager.OnStatsUpdated -= OnStatsUpdated;
            resourceManager.OnMissionEnded -= OnMissionEnded;
        }
    }
}
