using System;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    // Instancia global del administrador de recursos
    public static ResourceManager Instance { get; private set; }

    [Header("Configuración de Misión")]
    [SerializeField, Min(1)]
    private int maxSoles = 10;

    public int MaxSoles => maxSoles;
    public int CurrentSol { get; private set; } = 1;

    [Header("Recursos Iniciales (0 a 100)")]
    [SerializeField, Range(0f, 100f)]
    private float energy = 80f;

    [SerializeField, Range(0f, 100f)]
    private float lifeSupport = 80f;

    [SerializeField, Range(0f, 100f)]
    private float shieldIntegrity = 80f;

    // Propiedades de lectura para otros scripts
    public float Energy => energy;
    public float LifeSupport => lifeSupport;
    public float ShieldIntegrity => shieldIntegrity;

    [Header("Consumo de Recursos por Sol")]
    [SerializeField, Min(0f)]
    private float energyConsumption = 5f;

    [SerializeField, Min(0f)]
    private float lifeSupportConsumption = 4f;

    [SerializeField, Min(0f)]
    private float shieldConsumption = 3f;

    // Eventos para actualizar la interfaz
    public event Action OnStatsUpdated;
    public event Action<bool, string> OnMissionEnded;

    // Estado actual de la misión
    private bool missionOver = false;

    public bool IsMissionOver => missionOver;

    // --------------------------------------------------
    // INICIALIZACIÓN
    // --------------------------------------------------

    private void Awake()
    {
        // Evita múltiples administradores de recursos
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
        // Notifica los valores iniciales
        NotifyStatsChanged();
    }

    // --------------------------------------------------
    // AVANZAR AL SIGUIENTE SOL
    // --------------------------------------------------

    public void AdvanceSol()
    {
        // No permite avanzar si terminó la misión
        if (missionOver)
        {
            return;
        }

        // Consumo diario de recursos
        energy -= energyConsumption;
        lifeSupport -= lifeSupportConsumption;
        shieldIntegrity -= shieldConsumption;

        // Mantiene los recursos entre 0 y 100
        ClampResources();

        // Actualiza los indicadores de la interfaz
        NotifyStatsChanged();

        // Comprueba si algún recurso llegó a cero
        if (CheckMissionFailure())
        {
            return;
        }

        // Comprueba si completamos el último sol
        if (CurrentSol >= maxSoles)
        {
            EndMission(
                true,
                $"¡Misión Exitosa! Sobreviviste {maxSoles} soles en Marte."
            );

            return;
        }

        // Comienza un nuevo sol
        CurrentSol++;

        // Actualiza el contador de soles
        NotifyStatsChanged();
    }

    // --------------------------------------------------
    // APLICAR DECISIONES DE EVENTOS
    // --------------------------------------------------

    public void ApplyEventChoice(
        float energyDelta,
        float lifeSupportDelta,
        float shieldDelta)
    {
        // No permite modificar recursos al terminar
        if (missionOver)
        {
            return;
        }

        // Aplica las consecuencias de la decisión
        energy += energyDelta;
        lifeSupport += lifeSupportDelta;
        shieldIntegrity += shieldDelta;

        // Limita los recursos entre 0 y 100
        ClampResources();

        // Actualiza la interfaz
        NotifyStatsChanged();

        // Comprueba si la decisión causó una derrota
        CheckMissionFailure();
    }

    // --------------------------------------------------
    // CONTROL DE LÍMITES
    // --------------------------------------------------

    private void ClampResources()
    {
        energy = Mathf.Clamp(energy, 0f, 100f);

        lifeSupport = Mathf.Clamp(
            lifeSupport,
            0f,
            100f
        );

        shieldIntegrity = Mathf.Clamp(
            shieldIntegrity,
            0f,
            100f
        );
    }

    // --------------------------------------------------
    // COMPROBAR DERROTA
    // --------------------------------------------------

    private bool CheckMissionFailure()
    {
        if (energy <= 0f ||
            lifeSupport <= 0f ||
            shieldIntegrity <= 0f)
        {
            EndMission(
                false,
                "¡Misión Fracasada! Los recursos de la base colapsaron."
            );

            return true;
        }

        return false;
    }

    // --------------------------------------------------
    // NOTIFICAR CAMBIOS DE RECURSOS
    // --------------------------------------------------

    private void NotifyStatsChanged()
    {
        OnStatsUpdated?.Invoke();
    }

    // --------------------------------------------------
    // FINALIZAR MISIÓN
    // --------------------------------------------------

    private void EndMission(bool success, string message)
    {
        // Evita finalizar la misión varias veces
        if (missionOver)
        {
            return;
        }

        missionOver = true;

        // Notifica victoria o derrota
        OnMissionEnded?.Invoke(success, message);
    }

    // --------------------------------------------------
    // LIMPIEZA DE INSTANCIA
    // --------------------------------------------------

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
