using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class EstacionMision : MonoBehaviour
{
    [Header("Datos Científicos del Módulo")]
    public string nombreModulo = "Invernadero Hidropónico";
    [TextArea(4, 8)]
    public string datosCientificos = "Datos de la misión...";

    [Header("Referencias de la Interfaz (UI)")]
    public GameObject avisoInteraccion;
    public GameObject panelInfo;
    public TextMeshProUGUI textoTitulo;
    public TextMeshProUGUI textoDatos;
    public TextMeshProUGUI textoProgreso;

    private bool jugadorCerca = false;
    private bool inspeccionado = false;
    private static int modulosCompletados = 0;
    private const int totalModulos = 4;
    private static EstacionMision estacionActiva;

    void Start()
    {
        modulosCompletados = 0;
        ActualizarProgreso();
    }

    void Update()
    {
        if (jugadorCerca && estacionActiva == this && Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (panelInfo != null)
            {
                bool abrir = !panelInfo.activeSelf;
                panelInfo.SetActive(abrir);

                if (abrir)
                {
                    if (textoTitulo != null) textoTitulo.text = nombreModulo;
                    if (textoDatos != null) textoDatos.text = datosCientificos;
                    if (avisoInteraccion != null) avisoInteraccion.SetActive(false);

                    if (!inspeccionado)
                    {
                        inspeccionado = true;
                        modulosCompletados++;
                        ActualizarProgreso();
                    }
                }
                else if (avisoInteraccion != null)
                {
                    avisoInteraccion.SetActive(true);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;
            estacionActiva = this;
            if (avisoInteraccion != null && (panelInfo == null || !panelInfo.activeSelf))
            {
                avisoInteraccion.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            if (estacionActiva == this)
            {
                estacionActiva = null;
            }
            if (avisoInteraccion != null) avisoInteraccion.SetActive(false);
            if (panelInfo != null) panelInfo.SetActive(false);
        }
    }

    private void ActualizarProgreso()
    {
        if (textoProgreso != null)
        {
            if (modulosCompletados >= totalModulos)
                textoProgreso.text = $"Misión Completada: {modulosCompletados}/{totalModulos} módulos analizados";
            else
                textoProgreso.text = $"Progreso de Misión: {modulosCompletados}/{totalModulos} módulos";
        }
    }
}