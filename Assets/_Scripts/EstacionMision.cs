using UnityEngine;
using UnityEngine.InputSystem;
using TMPro; // Si usan TextMeshPro para textos

public class EstacionMision : MonoBehaviour
{
    [Header("Datos de la Misión Junior")]
    public string nombreModulo = "Sistema Hidropónico";
    [TextArea(3, 6)]
    public string datoCientificoNASA = "En la EEI, la hidroponía permite cultivar vegetales usando 90% menos agua que en la Tierra.";
    
    [Header("Referencias UI")]
    public GameObject avisoInteraccion; // Ej. Texto "Presiona E"
    public GameObject panelCientifico;  // El panel con la info de la NASA
    public TMP_Text textoTitulo;
    public TMP_Text textoDato;

    private bool jugadorCerca = false;
    private bool misionCompletada = false;

    void Start()
    {
        if (avisoInteraccion != null) avisoInteraccion.SetActive(false);
        if (panelCientifico != null) panelCientifico.SetActive(false);
    }

    void Update()
    {
        if (jugadorCerca && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            AbrirPanelMision();
        }
    }

    public void AbrirPanelMision()
    {
        if (panelCientifico != null)
        {
            panelCientifico.SetActive(true);
            if (textoTitulo != null) textoTitulo.text = nombreModulo;
            if (textoDato != null) textoDato.text = datoCientificoNASA;
        }
        if (avisoInteraccion != null) avisoInteraccion.SetActive(false);
    }

    public void CerrarYCompletar()
    {
        if (panelCientifico != null) panelCientifico.SetActive(false);
        if (!misionCompletada)
        {
            misionCompletada = true;
            Debug.Log("Misión completada: " + nombreModulo);
            // Aquí puedes cambiar el color de una luz de rojo a verde
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;
            if (avisoInteraccion != null) avisoInteraccion.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            if (avisoInteraccion != null) avisoInteraccion.SetActive(false);
            if (panelCientifico != null) panelCientifico.SetActive(false);
        }
    }
}