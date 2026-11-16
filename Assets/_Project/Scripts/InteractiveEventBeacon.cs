using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class InteractiveEventBeacon : MonoBehaviour
{
    [Header("Referencias de HUD y Escena")]
    [SerializeField] private MarsWelcomeExplorer missionHud;
    [SerializeField] private Button btnNextSol;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private TextMeshProUGUI txtActionPrompt;
    [SerializeField] private GameObject panelEvent;
    [SerializeField] private GameObject panelEventResult;

    [Header("Módulos de la Base (Arrastra los 5)")]
    [SerializeField] private Transform cupula;
    [SerializeField] private Transform panelSolar;
    [SerializeField] private Transform plantacion;
    [SerializeField] private Transform agua;
    [SerializeField] private Transform terminalDeCampo;

    [Header("Ajustes de Interacción")]
    [SerializeField] private float activationDistance = 7.5f;

    private Transform currentTarget;
    private GameObject beaconVisual;
    private GameObject beaconRing;
    private GameObject playerPointer;

    private float initialDistanceToTarget = 1f;
    private bool isBeaconActive = false;
    private bool solEventResolved = false;
    private int lastTrackedSol = -1;
    private string currentEventName = "OUTPOST ANOMALY";
    private ResourceManager resourceManager;

    private void Start()
    {
        resourceManager = Object.FindFirstObjectByType<ResourceManager>();
        CreateVisualIndicators();
        ClearBeacon();
    }

    private void CreateVisualIndicators()
    {
        // 1. Rayo vertical dorado sobre el módulo afectado
        beaconVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beaconVisual.name = "Emergency_Beacon_Beam";
        Destroy(beaconVisual.GetComponent<Collider>());
        beaconVisual.transform.localScale = new Vector3(0.55f, 6.5f, 0.55f);

        Renderer beaconRend = beaconVisual.GetComponent<Renderer>();
        beaconRend.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        beaconRend.material.color = new Color(1f, 0.78f, 0.0f, 0.85f);
        beaconRend.material.EnableKeyword("_EMISSION");
        beaconRend.material.SetColor("_EmissionColor", new Color(1f, 0.6f, 0.0f) * 3.0f);

        // 2. Anillo luminoso en el suelo alrededor del módulo
        beaconRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beaconRing.name = "Emergency_Ground_Ring";
        Destroy(beaconRing.GetComponent<Collider>());
        beaconRing.transform.localScale = new Vector3(5.0f, 0.08f, 5.0f);

        Renderer ringRend = beaconRing.GetComponent<Renderer>();
        ringRend.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        ringRend.material.color = new Color(1f, 0.25f, 0.15f, 0.65f);
        ringRend.material.EnableKeyword("_EMISSION");
        ringRend.material.SetColor("_EmissionColor", new Color(1f, 0.2f, 0.1f) * 2.2f);

        // 3. Flecha guía flotante sobre el casco del astronauta
        playerPointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        playerPointer.name = "Astronaut_Objective_Pointer";
        Destroy(playerPointer.GetComponent<Collider>());
        playerPointer.transform.localScale = new Vector3(0.26f, 0.26f, 0.95f);

        Renderer ptrRend = playerPointer.GetComponent<Renderer>();
        ptrRend.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        ptrRend.material.color = new Color(0.0f, 0.95f, 1.0f, 1f);
        ptrRend.material.EnableKeyword("_EMISSION");
        ptrRend.material.SetColor("_EmissionColor", new Color(0.0f, 0.95f, 1.0f) * 3.0f);
    }

    private Transform GetModuleForEventTitle(string title)
    {
        string lower = title.ToLower();

        if (lower.Contains("solar") || lower.Contains("dust") || lower.Contains("power") || lower.Contains("panel"))
            return panelSolar != null ? panelSolar : cupula;

        if (lower.Contains("oxygen") || lower.Contains("plant") || lower.Contains("co2") || lower.Contains("crop"))
            return plantacion != null ? plantacion : cupula;

        if (lower.Contains("water") || lower.Contains("recycl") || lower.Contains("ice"))
            return agua != null ? agua : cupula;

        if (lower.Contains("comm") || lower.Contains("equip") || lower.Contains("meteor") || lower.Contains("signal"))
            return terminalDeCampo != null ? terminalDeCampo : cupula;

        return cupula;
    }

    public void ClearBeacon()
    {
        isBeaconActive = false;
        currentTarget = null;
        if (beaconVisual != null) beaconVisual.SetActive(false);
        if (beaconRing != null) beaconRing.SetActive(false);
        if (playerPointer != null) playerPointer.SetActive(false);
    }

    private void Update()
    {
        if (resourceManager == null)
            resourceManager = Object.FindFirstObjectByType<ResourceManager>();

        int currentSol = resourceManager != null ? resourceManager.CurrentSol : 1;

        // Sol 1 es solo exploración
        if (currentSol <= 1)
        {
            lastTrackedSol = currentSol;
            return;
        }

        // Detectar cambio de Sol (Sol 2 -> Sol 3 -> Sol 4...) y reiniciar estado limpio
        if (currentSol != lastTrackedSol)
        {
            lastTrackedSol = currentSol;
            solEventResolved = false;
            ClearBeacon();
        }

        // 1. Interceptar el evento que genera MarsEventManager en el Sol actual
        if (panelEvent != null && panelEvent.activeSelf && !isBeaconActive && !solEventResolved)
        {
            currentEventName = "OUTPOST ANOMALY";
            TextMeshProUGUI[] textosPanel = panelEvent.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in textosPanel)
            {
                if (tmp.gameObject.name.ToLower().Contains("title"))
                {
                    currentEventName = tmp.text;
                    break;
                }
            }

            currentTarget = GetModuleForEventTitle(currentEventName);
            if (currentTarget != null)
            {
                panelEvent.SetActive(false);
                isBeaconActive = true;
                initialDistanceToTarget = Mathf.Max(
                    activationDistance + 1f,
                    Vector3.Distance(playerTransform.position, currentTarget.position)
                );

                if (btnNextSol != null) btnNextSol.interactable = false;
                if (missionHud != null) missionHud.UpdateProgressBar(0f, $"SOL {currentSol} ALERT", true);

                if (beaconVisual != null) beaconVisual.SetActive(true);
                if (beaconRing != null) beaconRing.SetActive(true);
                if (playerPointer != null) playerPointer.SetActive(true);

                if (txtActionPrompt != null)
                {
                    txtActionPrompt.gameObject.SetActive(true);
                    txtActionPrompt.text =
                        $"<size=125%><color=#FF4D4D><b>SOL {currentSol} ALERT: {currentEventName.ToUpper()}!</b></color></size>\n" +
                        $"<color=#FFD700>Follow the cyan pointer to the glowing beacon at <b>{currentTarget.name}</b>!</color>";
                }
            }
        }

        // 2. Cuando el jugador elige una opción y cierra los paneles, marcar 100% y habilitar Next Sol
        bool panelsClosed = (panelEvent == null || !panelEvent.activeSelf) &&
                            (panelEventResult == null || !panelEventResult.activeSelf);

        if (solEventResolved && panelsClosed && !isBeaconActive)
        {
            if (missionHud != null) missionHud.UpdateProgressBar(1f, $"SOL {currentSol} COMPLETE");
            if (btnNextSol != null && !btnNextSol.interactable) btnNextSol.interactable = true;
            if (txtActionPrompt != null)
            {
                txtActionPrompt.gameObject.SetActive(true);
                txtActionPrompt.text =
                    $"<size=125%><color=#FFD700><b>★ SOL {currentSol} MISSION COMPLETE! (100%) ★</b></color></size>\n" +
                    "<color=#7CFF6B>Press <b>[NEXT SOL]</b> to continue the Mars expedition!</color>";
            }
            return;
        }

        if (!isBeaconActive || currentTarget == null || playerTransform == null) return;

        // 3. Animar rayo dorado, anillo rojo en el suelo y flecha cian
        float pulse = 1.0f + Mathf.Sin(Time.time * 6f) * 0.22f;
        beaconVisual.transform.position = currentTarget.position + Vector3.up * 5.2f;
        beaconVisual.transform.localScale = new Vector3(0.55f * pulse, 6.5f, 0.55f * pulse);

        beaconRing.transform.position = new Vector3(currentTarget.position.x, playerTransform.position.y + 0.1f, currentTarget.position.z);
        beaconRing.transform.localScale = new Vector3(5.0f * pulse, 0.08f, 5.0f * pulse);

        Vector3 flatTarget = new Vector3(currentTarget.position.x, playerTransform.position.y + 2.4f, currentTarget.position.z);
        playerPointer.transform.position = playerTransform.position + Vector3.up * 2.4f;
        playerPointer.transform.LookAt(flatTarget);

        // 4. Progreso por cercanía (0% -> 60%) y activación con un toque de [R]
        float dist = Vector3.Distance(playerTransform.position, currentTarget.position);
        if (dist > activationDistance)
        {
            float travelProgress = Mathf.Clamp01(1f - ((dist - activationDistance) / (initialDistanceToTarget - activationDistance)));
            if (missionHud != null)
                missionHud.UpdateProgressBar(travelProgress * 0.60f, "APPROACHING TARGET");
        }
        else
        {
            if (missionHud != null)
                missionHud.UpdateProgressBar(0.75f, "READY TO DIAGNOSE");

            if (txtActionPrompt != null)
            {
                txtActionPrompt.text =
                    $"<size=125%><color=#7CFF6B><b>MODULE REACHED!</b></color></size>\n" +
                    $"<color=#FFFFFF>Press <color=#FFD700><b>[R]</b></color> to inspect and solve <b>{currentEventName}</b>!</color>";
            }

            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                solEventResolved = true;
                ClearBeacon();
                if (txtActionPrompt != null) txtActionPrompt.gameObject.SetActive(false);
                if (panelEvent != null) panelEvent.SetActive(true);
            }
        }
    }
}