using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class MarsWelcomeExplorer : MonoBehaviour
{
    [System.Serializable]
    public class OutpostModuleInfo
    {
        public string moduleName;
        public Transform moduleTransform;
        [TextArea(2, 4)]
        public string description;
    }

    [Header("UI de Bienvenida y Exploración")]
    [SerializeField] private TextMeshProUGUI txtModuleInspector;
    [SerializeField] private Button btnNextSol;
    [SerializeField] private GameObject panelEvent;
    [SerializeField] private GameObject panelEventResult;

    [Header("Módulos de la Base")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float inspectRadius = 8f;
    [SerializeField] private OutpostModuleInfo[] modules;

    private int inspectedCount = 0;
    private bool[] inspectedFlags;
    private bool missionStarted = false;
    private bool sol1Completed = false;
    private bool blueBoxRetired = false;

    private Image cardBackground;
    private Outline cardBorder;
    private RectTransform bgRect;
    private RectTransform textRect;

    // Barra de progreso de misión generada automáticamente en el HUD
    private Image progressBarFill;
    private TextMeshProUGUI txtProgressPercent;
    private GameObject progressBarContainer;

    // Marcadores 3D flotantes sobre cada módulo en Sol 1
    private GameObject[] moduleMarkers;
    private Renderer[] markerRenderers;

    private void Start()
    {
        inspectedFlags = new bool[modules.Length];
        CreateModuleMarkers();
        SetupKidFriendlyCardStyle();
        CreateMissionProgressBar();
        UpdateProgressBar(0f, "SOL 1 RECON");

        // Desactivar Next Sol en Sol 1 hasta completar el escaneo
        if (btnNextSol != null)
        {
            btnNextSol.interactable = false;
            btnNextSol.onClick.AddListener(OnNextSolClicked);
        }

        ShowStartScreen();
    }

    private void CreateModuleMarkers()
    {
        if (modules == null) return;
        moduleMarkers = new GameObject[modules.Length];
        markerRenderers = new Renderer[modules.Length];

        for (int i = 0; i < modules.Length; i++)
        {
            if (modules[i].moduleTransform == null) continue;

            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = $"Marker_{modules[i].moduleName}";
            Destroy(marker.GetComponent<Collider>());

            float alturaExtra = modules[i].moduleTransform.name.ToLower().Contains("cupula") ? 6.5f : 3.2f;
            marker.transform.position = modules[i].moduleTransform.position + Vector3.up * alturaExtra;
            marker.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
            marker.transform.rotation = Quaternion.Euler(45f, 0f, 45f);

            Renderer rend = marker.GetComponent<Renderer>();
            rend.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            rend.material.color = new Color(0.0f, 0.92f, 1.0f, 1f);
            rend.material.EnableKeyword("_EMISSION");
            rend.material.SetColor("_EmissionColor", new Color(0.0f, 0.92f, 1.0f) * 2.5f);

            moduleMarkers[i] = marker;
            markerRenderers[i] = rend;
        }
    }

    private void CreateMissionProgressBar()
    {
        if (txtModuleInspector == null) return;
        Transform canvasRoot = txtModuleInspector.transform.parent;

        progressBarContainer = new GameObject("MissionProgressBar_HUD", typeof(RectTransform), typeof(Image), typeof(Outline));
        progressBarContainer.transform.SetParent(canvasRoot, false);

        RectTransform containerRect = progressBarContainer.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 1f);
        containerRect.anchorMax = new Vector2(0.5f, 1f);
        containerRect.pivot = new Vector2(0.5f, 1f);
        containerRect.anchoredPosition = new Vector2(0f, -18f);
        containerRect.sizeDelta = new Vector2(420f, 34f);

        Image bgImg = progressBarContainer.GetComponent<Image>();
        bgImg.color = new Color(0.05f, 0.08f, 0.20f, 0.90f);

        Outline outline = progressBarContainer.GetComponent<Outline>();
        outline.effectColor = new Color(0.0f, 0.92f, 1.0f, 0.95f);
        outline.effectDistance = new Vector2(2f, -2f);

        GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillObj.transform.SetParent(progressBarContainer.transform, false);
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = new Vector2(4f, 4f);
        fillRect.offsetMax = new Vector2(-4f, -4f);

        progressBarFill = fillObj.GetComponent<Image>();
        progressBarFill.color = new Color(0.18f, 0.88f, 0.45f, 1f);
        progressBarFill.type = Image.Type.Filled;
        progressBarFill.fillMethod = Image.FillMethod.Horizontal;
        progressBarFill.fillAmount = 0f;

        GameObject labelObj = new GameObject("Txt_Progress", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObj.transform.SetParent(progressBarContainer.transform, false);
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        txtProgressPercent = labelObj.GetComponent<TextMeshProUGUI>();
        txtProgressPercent.font = txtModuleInspector.font;
        txtProgressPercent.fontSize = 17f;
        txtProgressPercent.fontStyle = FontStyles.Bold;
        txtProgressPercent.alignment = TextAlignmentOptions.Center;
        txtProgressPercent.color = Color.white;
        txtProgressPercent.raycastTarget = false;
    }

    public void UpdateProgressBar(float normalizedValue, string phaseLabel, bool isEmergencyAlert = false)
    {
        normalizedValue = Mathf.Clamp01(normalizedValue);
        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = Mathf.Max(normalizedValue, isEmergencyAlert ? 0.12f : 0f);

            if (normalizedValue >= 1f)
                progressBarFill.color = new Color(1.0f, 0.82f, 0.10f, 1f);   // Dorado: 100% Completado
            else if (normalizedValue >= 0.50f)
                progressBarFill.color = new Color(0.0f, 0.88f, 1.0f, 1f);    // Cian: Diagnóstico / Avanzado
            else if (normalizedValue > 0f)
                progressBarFill.color = new Color(1.0f, 0.60f, 0.05f, 1f);   // Naranja: En progreso / En camino
            else
                progressBarFill.color = isEmergencyAlert
                    ? new Color(1.0f, 0.22f, 0.22f, 1f)                      // Rojo: Alerta de crisis 0%
                    : new Color(0.18f, 0.88f, 0.45f, 1f);                    // Verde: Inicio Sol 1
        }

        if (txtProgressPercent != null)
        {
            int pct = Mathf.RoundToInt(normalizedValue * 100f);
            txtProgressPercent.text = $"{phaseLabel}: {pct}%";
        }
    }

    private void SetupKidFriendlyCardStyle()
    {
        if (txtModuleInspector == null) return;

        txtModuleInspector.gameObject.SetActive(true);
        textRect = txtModuleInspector.rectTransform;
        txtModuleInspector.alignment = TextAlignmentOptions.Center;
        txtModuleInspector.raycastTarget = false;

        Transform existingBg = textRect.parent.Find("KidCard_Background");
        GameObject bgObj = existingBg != null
            ? existingBg.gameObject
            : new GameObject("KidCard_Background", typeof(RectTransform), typeof(Image), typeof(Outline));

        bgObj.transform.SetParent(textRect.parent, false);
        bgObj.transform.SetSiblingIndex(textRect.GetSiblingIndex());

        bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = textRect.anchorMin;
        bgRect.anchorMax = textRect.anchorMax;
        bgRect.pivot = textRect.pivot;

        cardBackground = bgObj.GetComponent<Image>();
        cardBackground.color = new Color(0.05f, 0.09f, 0.24f, 0.94f);
        cardBackground.raycastTarget = false;

        cardBorder = bgObj.GetComponent<Outline>();
        cardBorder.effectColor = new Color(0.0f, 0.92f, 1.0f, 1.0f);
        cardBorder.effectDistance = new Vector2(3f, -3f);
    }

    private void ShowStartScreen()
    {
        if (txtModuleInspector == null) return;

        txtModuleInspector.gameObject.SetActive(true);
        if (cardBackground != null) cardBackground.gameObject.SetActive(true);

        Vector2 centerPos = new Vector2(0f, -40f);
        textRect.anchoredPosition = centerPos;
        textRect.sizeDelta = new Vector2(880f, 230f);
        if (bgRect != null)
        {
            bgRect.anchoredPosition = centerPos;
            bgRect.sizeDelta = new Vector2(920f, 245f);
        }

        txtModuleInspector.fontSize = 22f;
        txtModuleInspector.text =
            "<size=135%><color=#FFD700><b>WELCOME TO MISSION-MARS, CADET!</b></color></size>\n\n" +
            "<color=#00F5FF><b>[W]  [A]  [S]  [D]</b></color> or <color=#00F5FF><b>Arrow Keys</b></color> : Move Astronaut\n" +
            "<color=#00F5FF><b>[Q] / [E]</b></color> or <color=#00F5FF><b>Mouse Drag</b></color> : Rotate Camera 360°   •   <color=#00F5FF><b>[SPACE]</b></color> : Mars Jump (-3.72 m/s²)\n" +
            "<color=#7CFF6B><b>[F]</b></color> : Scan Base Modules   •   <color=#7CFF6B><b>[R]</b></color> : Repair / Emergency Action\n\n" +
            "<size=120%><color=#FFD700><b>► CLICK TO CONTINUE ◄</b></color></size>";
    }

    private void MoveCardToBottomHud()
    {
        if (textRect == null) return;

        txtModuleInspector.gameObject.SetActive(true);
        if (cardBackground != null) cardBackground.gameObject.SetActive(true);

        Vector2 bottomPos = new Vector2(0f, -360f);
        textRect.anchoredPosition = bottomPos;
        textRect.sizeDelta = new Vector2(880f, 125f);
        if (bgRect != null)
        {
            bgRect.anchoredPosition = bottomPos;
            bgRect.sizeDelta = new Vector2(920f, 138f);
        }

        txtModuleInspector.fontSize = 21f;
        txtModuleInspector.text =
            "<size=115%><color=#FFD700><b>SOL 1 OBJECTIVE: SCAN ALL 5 BASE MODULES!</b></color></size>\n" +
            "<color=#FFFFFF>Walk near each glowing marker and press <color=#7CFF6B><b>[F]</b></color> to reach 100% Mission Progress!</color>";
    }

    private void OnNextSolClicked()
    {
        // Al avanzar de Sol 1 a Sol 2, retirar permanentemente el recuadro azul y los marcadores de Sol 1
        blueBoxRetired = true;
        if (cardBackground != null) cardBackground.gameObject.SetActive(false);
        if (txtModuleInspector != null) txtModuleInspector.text = "";

        if (moduleMarkers != null)
        {
            foreach (var m in moduleMarkers)
                if (m != null) m.SetActive(false);
        }
    }

    private void Update()
    {
        if (blueBoxRetired) return;
        if (playerTransform == null || modules == null || Keyboard.current == null) return;

        bool eventoAbierto = (panelEvent != null && panelEvent.activeInHierarchy) ||
                             (panelEventResult != null && panelEventResult.activeInHierarchy);
        if (eventoAbierto)
        {
            if (cardBackground != null) cardBackground.gameObject.SetActive(false);
            if (txtModuleInspector != null) txtModuleInspector.gameObject.SetActive(false);
            return;
        }

        // Asegurar que el texto siempre esté activo mientras el recuadro azul de Sol 1 siga en pantalla
        if (!txtModuleInspector.gameObject.activeSelf)
            txtModuleInspector.gameObject.SetActive(true);
        if (cardBackground != null && !cardBackground.gameObject.activeSelf)
            cardBackground.gameObject.SetActive(true);

        // Animar los marcadores flotantes sobre cada módulo en Sol 1
        if (moduleMarkers != null)
        {
            for (int i = 0; i < moduleMarkers.Length; i++)
            {
                if (moduleMarkers[i] == null || modules[i].moduleTransform == null) continue;
                moduleMarkers[i].transform.Rotate(Vector3.up * 90f * Time.deltaTime, Space.World);
                float alturaBase = modules[i].moduleTransform.name.ToLower().Contains("cupula") ? 6.5f : 3.2f;
                float bob = Mathf.Sin(Time.time * 3f + i) * 0.25f;
                moduleMarkers[i].transform.position = modules[i].moduleTransform.position + Vector3.up * (alturaBase + bob);
            }
        }

        if (!missionStarted)
        {
            bool clickedToStart = (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
                                  Keyboard.current.enterKey.wasPressedThisFrame ||
                                  Keyboard.current.spaceKey.wasPressedThisFrame;

            if (clickedToStart)
            {
                missionStarted = true;
                MoveCardToBottomHud();
            }
            return;
        }

        // Si ya completó los 5 módulos en Sol 1, mantener el cartel dorado MISSION COMPLETE
        if (sol1Completed) return;

        OutpostModuleInfo nearest = null;
        int nearestIndex = -1;
        float minDist = inspectRadius;

        for (int i = 0; i < modules.Length; i++)
        {
            if (modules[i].moduleTransform == null) continue;
            float dist = Vector3.Distance(playerTransform.position, modules[i].moduleTransform.position);
            if (dist <= minDist)
            {
                minDist = dist;
                nearest = modules[i];
                nearestIndex = i;
            }
        }

        if (nearest != null)
        {
            if (txtModuleInspector != null && !Keyboard.current.fKey.wasPressedThisFrame)
            {
                if (!txtModuleInspector.text.Contains(nearest.moduleName.ToUpper()))
                {
                    txtModuleInspector.text =
                        $"<size=120%><color=#00F5FF><b>SCANNER READY: {nearest.moduleName.ToUpper()}</b></color></size>\n" +
                        $"<color=#FFFFFF>Press <color=#7CFF6B><b>[F]</b></color> to inspect this module!</color>";
                }
            }

            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                if (!inspectedFlags[nearestIndex])
                {
                    inspectedFlags[nearestIndex] = true;
                    inspectedCount++;

                    // Cambiar marcador de ese módulo de cian a verde completado
                    if (markerRenderers != null && markerRenderers[nearestIndex] != null)
                    {
                        markerRenderers[nearestIndex].material.color = new Color(0.2f, 1.0f, 0.45f, 1f);
                        markerRenderers[nearestIndex].material.SetColor("_EmissionColor", new Color(0.2f, 1.0f, 0.45f) * 2.5f);
                    }

                    float ratio = (float)inspectedCount / modules.Length;
                    UpdateProgressBar(ratio, "SOL 1 RECON");
                }

                if (inspectedCount >= modules.Length)
                {
                    sol1Completed = true;

                    // Desbloquear Next Sol
                    if (btnNextSol != null)
                        btnNextSol.interactable = true;

                    // Cambiar tarjeta a estilo dorado MISSION COMPLETE
                    if (cardBackground != null)
                        cardBackground.color = new Color(0.18f, 0.14f, 0.03f, 0.95f);
                    if (cardBorder != null)
                        cardBorder.effectColor = new Color(1.0f, 0.84f, 0.0f, 1.0f);

                    if (txtModuleInspector != null)
                    {
                        txtModuleInspector.text =
                            "<size=130%><color=#FFD700><b>★ SOL 1 MISSION COMPLETE! (100%) ★</b></color></size>\n" +
                            "<color=#FFFFFF>All outpost systems scanned! Press <color=#7CFF6B><b>[NEXT SOL]</b></color> to begin planetary survival!</color>";
                    }
                }
                else
                {
                    if (cardBackground != null)
                        cardBackground.color = new Color(0.05f, 0.22f, 0.28f, 0.92f);

                    if (txtModuleInspector != null)
                    {
                        txtModuleInspector.text =
                            $"<size=118%><color=#7CFF6B><b>{nearest.moduleName.ToUpper()}</b></color>  <color=#FFD700>({inspectedCount}/{modules.Length} SCANNED)</color></size>\n" +
                            $"<color=#FFFFFF>{nearest.description}</color>";
                    }
                }
            }
        }
    }
}