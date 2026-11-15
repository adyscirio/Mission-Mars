using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MarsEventManager : MonoBehaviour
{
    // ==========================================
    // MISSION EVENTS
    // ==========================================

    [Header("Mission Events")]
    [SerializeField] private List<MarsEvent> missionEvents = new List<MarsEvent>();

    // ==========================================
    // EVENT PANEL
    // ==========================================

    [Header("Event Panel")]
    [SerializeField] private GameObject panelEvent;
    [SerializeField] private TextMeshProUGUI txtEventTitle;
    [SerializeField] private TextMeshProUGUI txtEventDescription;

    // ==========================================
    // DECISION BUTTONS
    // ==========================================

    [Header("Decision Buttons")]
    [SerializeField] private Button btnOptionA;
    [SerializeField] private Button btnOptionB;
    [SerializeField] private TextMeshProUGUI txtOptionA;
    [SerializeField] private TextMeshProUGUI txtOptionB;

    // ==========================================
    // EVENT RESULT PANEL
    // ==========================================

    [Header("Event Result Panel")]
    [SerializeField] private GameObject panelEventResult;
    [SerializeField] private TextMeshProUGUI txtResultTitle;
    [SerializeField] private TextMeshProUGUI txtResultDescription;
    [SerializeField] private TextMeshProUGUI txtResourceChanges;
    [SerializeField] private TextMeshProUGUI txtScienceFact;
    [SerializeField] private Button btnContinueMission;

    // ==========================================
    // NEXT SOL BUTTON
    // ==========================================

    [Header("Next Sol Button")]
    [SerializeField] private Button btnNextSol;

    // ==========================================
    // 3D GAMEPLAY AND ANIMATIONS
    // ==========================================

    [Header("3D Gameplay & Animations")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private TextMeshProUGUI txtActionPrompt;
    [SerializeField] private float interactionDistance = 3.5f;

    // ==========================================
    // 3D SCENE MODULES
    // ==========================================

    [Header("3D Scene Modules")]
    [SerializeField] private Transform domeTarget;
    [SerializeField] private Transform solarPanelsTarget;
    [SerializeField] private Transform plantationTarget;
    [SerializeField] private Transform waterTarget;
    [SerializeField] private Transform terminalTarget;
    [SerializeField] private GameObject dustStormEffect;

    // ==========================================
    // SOLAR STORM EFFECT
    // ==========================================

    [Header("Solar Storm Effect")]
    [SerializeField] private SolarStormEffect solarStormEffect;

    // ==========================================
    // MANAGER REFERENCES
    // ==========================================

    private ResourceManager resourceManager;
    private MarsEvent currentEvent;

    private readonly HashSet<MarsEvent> completedEvents =
        new HashSet<MarsEvent>();

    // ==========================================
    // EVENT STATES
    // ==========================================

    private int lastProcessedSol = -1;

    private bool eventActive = false;
    private bool awaitingPhysicalAction = false;
    private bool performingAnimation = false;
    private bool resultActive = false;
    private bool missionFinished = false;

    // ==========================================
    // PENDING EVENT DATA
    // ==========================================

    private MarsEvent pendingEvent;

    private float pendingEnergy;
    private float pendingLifeSupport;
    private float pendingShield;

    private string pendingResultTitle;
    private string pendingResultDescription;

    private Transform activeTargetModule;
    private string activePromptMessage;

    private GameObject activeBeacon;

    // Remember the selected option
    private bool lastChoseA = false;

    // ==========================================
    // INITIALIZATION
    // ==========================================

    private void Start()
    {
        resourceManager = ResourceManager.Instance;

        if (resourceManager == null)
        {
            Debug.LogError(
                "MarsEventManager: ResourceManager was not found."
            );

            enabled = false;
            return;
        }

        resourceManager.OnStatsUpdated += OnStatsUpdated;
        resourceManager.OnMissionEnded += OnMissionEnded;

        if (btnOptionA != null)
            btnOptionA.onClick.AddListener(SelectOptionA);

        if (btnOptionB != null)
            btnOptionB.onClick.AddListener(SelectOptionB);

        if (btnContinueMission != null)
            btnContinueMission.onClick.AddListener(ContinueMission);

        if (panelEvent != null)
            panelEvent.SetActive(false);

        if (panelEventResult != null)
            panelEventResult.SetActive(false);

        if (txtActionPrompt != null)
            txtActionPrompt.gameObject.SetActive(false);

        CheckEventsForCurrentSol();
    }

    // ==========================================
    // UPDATE
    // ==========================================

    private void Update()
    {
        if (!awaitingPhysicalAction ||
            performingAnimation ||
            missionFinished ||
            activeTargetModule == null ||
            playerTransform == null)
        {
            return;
        }

        float distance = Vector3.Distance(
            new Vector3(
                playerTransform.position.x,
                0f,
                playerTransform.position.z
            ),
            new Vector3(
                activeTargetModule.position.x,
                0f,
                activeTargetModule.position.z
            )
        );

        if (distance <= interactionDistance)
        {
            if (txtActionPrompt != null)
            {
                txtActionPrompt.text =
                    "[E] " + activePromptMessage;
            }

            if (Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame)
            {
                StartCoroutine(ExecutePhysicalTaskRoutine());
            }
        }
        else
        {
            if (txtActionPrompt != null)
            {
                txtActionPrompt.text =
                    "OBJECTIVE: " +
                    activePromptMessage +
                    " (" + Mathf.RoundToInt(distance) + "m)";
            }
        }
    }

    // ==========================================
    // RESOURCE UPDATES
    // ==========================================

    private void OnStatsUpdated()
    {
        if (resourceManager == null || missionFinished)
            return;

        if (lastProcessedSol != resourceManager.CurrentSol)
        {
            CheckEventsForCurrentSol();
        }

        UpdateNextSolButton();
    }

    // ==========================================
    // CHECK EVENTS
    // ==========================================

    private void CheckEventsForCurrentSol()
    {
        if (resourceManager == null ||
            resourceManager.IsMissionOver ||
            missionFinished ||
            eventActive ||
            awaitingPhysicalAction ||
            resultActive)
        {
            return;
        }

        lastProcessedSol = resourceManager.CurrentSol;

        foreach (MarsEvent marsEvent in missionEvents)
        {
            if (marsEvent == null ||
                completedEvents.Contains(marsEvent))
            {
                continue;
            }

            if (marsEvent.targetSol == resourceManager.CurrentSol)
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
            txtEventTitle.text = marsEvent.eventTitle;

        if (txtEventDescription != null)
            txtEventDescription.text = marsEvent.eventDescription;

        if (txtOptionA != null)
            txtOptionA.text = marsEvent.optionATitle;

        if (txtOptionB != null)
            txtOptionB.text = marsEvent.optionBTitle;

        if (btnOptionA != null)
            btnOptionA.interactable = true;

        if (btnOptionB != null)
            btnOptionB.interactable = true;

        if (panelEvent != null)
            panelEvent.SetActive(true);

        // Activate the solar storm on Sol 2
        if (solarStormEffect != null &&
            marsEvent.targetSol == 2)
        {
            solarStormEffect.StartSolarStorm();

            Debug.Log(
                "Mars Mission: Solar storm started."
            );
        }

        // Activate the dust storm on Sol 3
        if (dustStormEffect != null)
        {
            bool isDustStorm =
                marsEvent.targetSol == 3 ||
                (!string.IsNullOrEmpty(marsEvent.eventTitle) &&
                 marsEvent.eventTitle.ToLower().Contains("dust"));

            dustStormEffect.SetActive(isDustStorm);
        }

        UpdateNextSolButton();
    }

    // ==========================================
    // OPTION A
    // ==========================================

    private void SelectOptionA()
    {
        if (!CanSelectOption())
            return;

        QueuePhysicalAction(
            true,
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
            return;

        QueuePhysicalAction(
            false,
            currentEvent.energyChangeB,
            currentEvent.lifeSupportChangeB,
            currentEvent.shieldChangeB,
            currentEvent.resultTitleB,
            currentEvent.resultDescriptionB
        );
    }

    private bool CanSelectOption()
    {
        return eventActive &&
               !awaitingPhysicalAction &&
               !resultActive &&
               !missionFinished &&
               currentEvent != null;
    }

    // ==========================================
    // QUEUE PHYSICAL ACTION
    // ==========================================

    private void QueuePhysicalAction(
        bool choseA,
        float energyDelta,
        float lifeSupportDelta,
        float shieldDelta,
        string resultTitle,
        string resultDescription)
    {
        lastChoseA = choseA;

        pendingEvent = currentEvent;

        pendingEnergy = energyDelta;
        pendingLifeSupport = lifeSupportDelta;
        pendingShield = shieldDelta;

        pendingResultTitle = resultTitle;
        pendingResultDescription = resultDescription;

        eventActive = false;
        currentEvent = null;
        awaitingPhysicalAction = true;

        if (panelEvent != null)
            panelEvent.SetActive(false);

        AssignTargetAndInstruction(
            pendingEvent.targetSol,
            choseA
        );

        CreateObjectiveBeacon(activeTargetModule);

        if (txtActionPrompt != null)
        {
            txtActionPrompt.gameObject.SetActive(true);

            txtActionPrompt.text =
                "OBJECTIVE: " + activePromptMessage;
        }

        UpdateNextSolButton();
    }

    // ==========================================
    // ASSIGN TARGET AND INSTRUCTION
    // ==========================================

    private void AssignTargetAndInstruction(
        int sol,
        bool choseA)
    {
        switch (sol)
        {
            case 2:
                activeTargetModule =
                    choseA ? terminalTarget : domeTarget;

                activePromptMessage = choseA
                    ? "Go to the Field Terminal to activate radiation shields"
                    : "Go to the Habitat Dome to maintain operations";
                break;

            case 3:
                activeTargetModule =
                    choseA ? solarPanelsTarget : terminalTarget;

                activePromptMessage = choseA
                    ? "Walk to the Solar Panels to clean accumulated dust"
                    : "Go to the Field Terminal to switch to power-saving mode";
                break;

            case 4:
                activeTargetModule =
                    choseA ? plantationTarget : terminalTarget;

                activePromptMessage = choseA
                    ? "Walk to the Hydroponics / O2 Generator to repair oxygen flow"
                    : "Go to the Field Terminal to activate backup oxygen tanks";
                break;

            case 5:
                activeTargetModule =
                    choseA ? waterTarget : terminalTarget;

                activePromptMessage = choseA
                    ? "Walk to the Water Recycling unit to repair filtration"
                    : "Go to the Field Terminal to enable water rationing";
                break;

            case 6:
                activeTargetModule =
                    choseA ? domeTarget : terminalTarget;

                activePromptMessage = choseA
                    ? "Go to the Habitat Dome to repair exterior shielding"
                    : "Go to the Field Terminal for emergency maintenance";
                break;

            case 7:
                activeTargetModule = terminalTarget;

                activePromptMessage = choseA
                    ? "Walk to the Comms Terminal to realign the antenna"
                    : "Go to the Field Terminal to activate backup relay";
                break;

            case 8:
                activeTargetModule = domeTarget;

                activePromptMessage = choseA
                    ? "Enter the Habitat Dome to boost thermal heaters"
                    : "Go to the Habitat Dome to reinforce thermal insulation";
                break;

            case 9:
                activeTargetModule = domeTarget;

                activePromptMessage = choseA
                    ? "Go to the Habitat Dome to reinforce structural hull"
                    : "Take shelter inside the Habitat Dome immediately";
                break;

            default:
                activeTargetModule =
                    terminalTarget != null
                        ? terminalTarget
                        : domeTarget;

                activePromptMessage =
                    "Walk to the Field Terminal to complete task";
                break;
        }

        if (activeTargetModule == null)
        {
            Debug.LogWarning(
                "MarsEventManager: Target module is not assigned."
            );

            activeTargetModule = playerTransform;
        }
    }

    // ==========================================
    // CREATE OBJECTIVE BEACON
    // ==========================================

    private void CreateObjectiveBeacon(Transform target)
    {
        if (activeBeacon != null)
            Destroy(activeBeacon);

        if (target == null)
            return;

        activeBeacon =
            GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        activeBeacon.name = "ObjectiveBeacon";

        Collider beaconCollider =
            activeBeacon.GetComponent<Collider>();

        if (beaconCollider != null)
            Destroy(beaconCollider);

        activeBeacon.transform.position =
            target.position + Vector3.up * 2.5f;

        activeBeacon.transform.localScale =
            new Vector3(0.4f, 2.5f, 0.4f);

        Renderer renderer =
            activeBeacon.GetComponent<Renderer>();

        if (renderer != null)
        {
            renderer.material.color =
                new Color(0f, 0.9f, 1f, 0.45f);
        }
    }

    // ==========================================
    // EXECUTE PHYSICAL TASK
    // ==========================================

    private IEnumerator ExecutePhysicalTaskRoutine()
    {
        performingAnimation = true;

        if (txtActionPrompt != null)
        {
            txtActionPrompt.text =
                "Performing task... Please wait";
        }

        Quaternion originalRotation =
            playerTransform != null
                ? playerTransform.rotation
                : Quaternion.identity;

        GameObject taskEffect = null;

        if (playerTransform != null &&
            activeTargetModule != null)
        {
            Vector3 lookPosition = new Vector3(
                activeTargetModule.position.x,
                playerTransform.position.y,
                activeTargetModule.position.z
            );

            Vector3 direction =
                lookPosition - playerTransform.position;

            if (direction.sqrMagnitude > 0.001f)
            {
                playerTransform.LookAt(lookPosition);
            }

            Quaternion workRotation =
                playerTransform.rotation;

            taskEffect =
                GameObject.CreatePrimitive(PrimitiveType.Sphere);

            Collider effectCollider =
                taskEffect.GetComponent<Collider>();

            if (effectCollider != null)
                Destroy(effectCollider);

            taskEffect.name = "PhysicalTaskEffect";

            taskEffect.transform.position =
                activeTargetModule.position +
                Vector3.up * 1.2f;

            taskEffect.transform.localScale =
                Vector3.one * 0.25f;

            Renderer renderer =
                taskEffect.GetComponent<Renderer>();

            if (renderer != null)
                renderer.material.color = Color.cyan;

            float elapsed = 0f;
            float duration = 2f;

            while (elapsed < duration)
            {
                if (missionFinished)
                {
                    if (taskEffect != null)
                        Destroy(taskEffect);

                    if (playerTransform != null)
                        playerTransform.rotation = originalRotation;

                    yield break;
                }

                elapsed += Time.deltaTime;

                float inclination =
                    Mathf.Sin(elapsed * 8f) * 10f + 15f;

                if (playerTransform != null)
                {
                    playerTransform.rotation =
                        workRotation *
                        Quaternion.Euler(inclination, 0f, 0f);
                }

                if (taskEffect != null)
                {
                    taskEffect.transform.localScale =
                        Vector3.one *
                        (0.2f + Mathf.PingPong(
                            elapsed * 2f,
                            0.35f
                        ));
                }

                yield return null;
            }
        }

        if (playerTransform != null)
            playerTransform.rotation = originalRotation;

        if (taskEffect != null)
            Destroy(taskEffect);

        if (activeBeacon != null)
            Destroy(activeBeacon);

        if (txtActionPrompt != null)
            txtActionPrompt.gameObject.SetActive(false);

        // ======================================
        // SOLAR STORM SHIELD RESPONSE
        // ======================================

        if (solarStormEffect != null &&
            pendingEvent != null &&
            pendingEvent.targetSol == 2)
        {
            solarStormEffect.ActivateRadiationShield(
                lastChoseA
            );

            Debug.Log(
                "Mars Mission: Radiation shield decision applied."
            );
        }

        // ======================================
        // APPLY EVENT CONSEQUENCES
        // ======================================

        MarsEvent resolvedEvent = pendingEvent;

        float energyDelta = pendingEnergy;
        float lifeSupportDelta = pendingLifeSupport;
        float shieldDelta = pendingShield;

        string resultTitle = pendingResultTitle;
        string resultDescription = pendingResultDescription;

        if (resolvedEvent != null)
            completedEvents.Add(resolvedEvent);

        awaitingPhysicalAction = false;
        performingAnimation = false;
        resultActive = true;

        resourceManager.ApplyEventChoice(
            energyDelta,
            lifeSupportDelta,
            shieldDelta
        );

        if (missionFinished ||
            resourceManager.IsMissionOver)
        {
            resultActive = false;
            UpdateNextSolButton();
            yield break;
        }

        if (resolvedEvent != null)
        {
            ShowEventResult(
                resolvedEvent,
                energyDelta,
                lifeSupportDelta,
                shieldDelta,
                resultTitle,
                resultDescription
            );
        }

        UpdateNextSolButton();
    }

    // ==========================================
    // SHOW EVENT RESULT
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
            txtResultTitle.text = resultTitle;

        if (txtResultDescription != null)
            txtResultDescription.text = resultDescription;

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
                "NASA Science Fact\n" +
                resolvedEvent.educationalExplanation;
        }

        if (btnContinueMission != null)
            btnContinueMission.interactable = true;

        if (panelEventResult != null)
            panelEventResult.SetActive(true);

        UpdateNextSolButton();
    }

    // ==========================================
    // FORMAT RESOURCE CHANGE
    // ==========================================

    private string FormatChange(float value)
    {
        return value > 0f
            ? "+" + value.ToString("0.#")
            : value.ToString("0.#");
    }

    // ==========================================
    // CONTINUE MISSION
    // ==========================================

    private void ContinueMission()
    {
        if (!resultActive || missionFinished)
            return;

        resultActive = false;

        if (panelEventResult != null)
            panelEventResult.SetActive(false);

        CheckEventsForCurrentSol();
        UpdateNextSolButton();
    }

    // ==========================================
    // UPDATE NEXT SOL BUTTON
    // ==========================================

    private void UpdateNextSolButton()
    {
        if (btnNextSol == null ||
            resourceManager == null)
        {
            return;
        }

        btnNextSol.interactable =
            !eventActive &&
            !awaitingPhysicalAction &&
            !performingAnimation &&
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
        awaitingPhysicalAction = false;
        performingAnimation = false;
        resultActive = false;

        currentEvent = null;
        pendingEvent = null;

        if (activeBeacon != null)
            Destroy(activeBeacon);

        if (panelEvent != null)
            panelEvent.SetActive(false);

        if (panelEventResult != null)
            panelEventResult.SetActive(false);

        if (txtActionPrompt != null)
            txtActionPrompt.gameObject.SetActive(false);

        UpdateNextSolButton();
    }

    // ==========================================
    // CLEANUP
    // ==========================================

    private void OnDestroy()
    {
        if (btnOptionA != null)
            btnOptionA.onClick.RemoveListener(SelectOptionA);

        if (btnOptionB != null)
            btnOptionB.onClick.RemoveListener(SelectOptionB);

        if (btnContinueMission != null)
            btnContinueMission.onClick.RemoveListener(ContinueMission);

        if (resourceManager != null)
        {
            resourceManager.OnStatsUpdated -= OnStatsUpdated;
            resourceManager.OnMissionEnded -= OnMissionEnded;
        }
    }
}