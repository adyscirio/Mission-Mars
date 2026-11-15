using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ControlTormenta : MonoBehaviour
{
    [Header("Elementos de la Tormenta")]
    public GameObject particulasTormenta;
    public TextMeshProUGUI textoAlerta;

    [Header("Configuración Atmosférica")]
    public Color colorNieblaNormal = new Color(0.76f, 0.45f, 0.32f);
    public Color colorNieblaTormenta = new Color(0.55f, 0.24f, 0.12f);
    public float densidadNormal = 0.004f;
    public float densidadTormenta = 0.032f;
    public float velocidadTransicion = 1.5f;

    [Header("Ciclo Automático (Opcional)")]
    public bool cicloAutomatico = true;
    public float segundosCalma = 35f;
    public float segundosDuracion = 15f;

    private bool tormentaActiva = false;
    private float temporizador = 0f;

    void Start()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = colorNieblaNormal;
        RenderSettings.fogDensity = densidadNormal;

        if (particulasTormenta != null) particulasTormenta.SetActive(false);
        if (textoAlerta != null) textoAlerta.gameObject.SetActive(false);
    }

    void Update()
    {
        // Activar o desactivar manualmente con la tecla T
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            CambiarEstadoTormenta(!tormentaActiva);
            temporizador = 0f;
        }

        // Ciclo automático de clima marciano
        if (cicloAutomatico)
        {
            temporizador += Time.deltaTime;
            float limite = tormentaActiva ? segundosDuracion : segundosCalma;
            if (temporizador >= limite)
            {
                CambiarEstadoTormenta(!tormentaActiva);
                temporizador = 0f;
            }
        }

        // Transición suave de la niebla en el ambiente
        Color colorObjetivo = tormentaActiva ? colorNieblaTormenta : colorNieblaNormal;
        float densidadObjetivo = tormentaActiva ? densidadTormenta : densidadNormal;

        RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, colorObjetivo, Time.deltaTime * velocidadTransicion);
        RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, densidadObjetivo, Time.deltaTime * velocidadTransicion);
    }

    public void CambiarEstadoTormenta(bool activar)
    {
        tormentaActiva = activar;

        if (particulasTormenta != null)
            particulasTormenta.SetActive(tormentaActiva);

        if (textoAlerta != null)
        {
            textoAlerta.gameObject.SetActive(tormentaActiva);
            if (tormentaActiva)
                textoAlerta.text = "ALERTA METEOROLÓGICA: Tormenta de polvo en curso - Refúgiese en la cúpula";
        }
    }
}