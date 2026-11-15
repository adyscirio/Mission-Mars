using UnityEngine;

[CreateAssetMenu(
    fileName = "NewMarsEvent",
    menuName = "Mars Mission/Mars Event"
)]
public class MarsEvent : ScriptableObject
{
    // ==========================================
    // GENERAL EVENT INFORMATION
    // ==========================================

    [Header("Event Information")]

    [Tooltip("Title displayed when the event starts.")]
    public string eventTitle = "New Mars Event";

    [TextArea(3, 5)]
    [Tooltip("Description of the situation.")]
    public string eventDescription =
        "Describe what is happening at the Mars outpost.";

    [Min(1)]
    [Tooltip("Sol when this event will occur.")]
    public int targetSol = 1;


    // ==========================================
    // OPTION A - RESOURCE CHANGES
    // ==========================================

    [Header("Option A - Resource Changes")]

    [Tooltip("Text displayed on Option A button.")]
    public string optionATitle = "Option A";

    [Tooltip("Energy change after selecting Option A.")]
    public float energyChangeA = 0f;

    [Tooltip("Life support change after selecting Option A.")]
    public float lifeSupportChangeA = 0f;

    [Tooltip("Shield change after selecting Option A.")]
    public float shieldChangeA = 0f;


    // ==========================================
    // OPTION A - DECISION RESULTS
    // ==========================================

    [Header("Option A - Decision Results")]

    [Tooltip("Result title for Option A.")]
    public string resultTitleA =
        "Decision Results";

    [TextArea(3, 5)]
    [Tooltip("Explanation of the outcome of Option A.")]
    public string resultDescriptionA =
        "Describe the consequences of choosing Option A.";


    // ==========================================
    // OPTION B - RESOURCE CHANGES
    // ==========================================

    [Header("Option B - Resource Changes")]

    [Tooltip("Text displayed on Option B button.")]
    public string optionBTitle = "Option B";

    [Tooltip("Energy change after selecting Option B.")]
    public float energyChangeB = 0f;

    [Tooltip("Life support change after selecting Option B.")]
    public float lifeSupportChangeB = 0f;

    [Tooltip("Shield change after selecting Option B.")]
    public float shieldChangeB = 0f;


    // ==========================================
    // OPTION B - DECISION RESULTS
    // ==========================================

    [Header("Option B - Decision Results")]

    [Tooltip("Result title for Option B.")]
    public string resultTitleB =
        "Decision Results";

    [TextArea(3, 5)]
    [Tooltip("Explanation of the outcome of Option B.")]
    public string resultDescriptionB =
        "Describe the consequences of choosing Option B.";


    // ==========================================
    // SCIENTIFIC INFORMATION
    // ==========================================

    [Header("Scientific Information")]

    [TextArea(3, 6)]
    [Tooltip("Educational explanation related to this event.")]
    public string educationalExplanation = "";

    [Tooltip("Name of the scientific source.")]
    public string scientificSource = "";

    [Tooltip("URL of the scientific source.")]
    public string scientificSourceURL = "";


    // ==========================================
    // VALIDATION
    // ==========================================

    private void OnValidate()
    {
        // Prevent events from occurring before Sol 1.
        targetSol = Mathf.Max(1, targetSol);
    }
}
