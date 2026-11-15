using System.Collections;
using UnityEngine;

public class SolarStormEffect : MonoBehaviour
{
    [Header("Referencias de Escena")]
    public Light sunLight;          // Directional Light
    public Transform habitatDome;   // Cupula

    private Color originalLightColor;
    private float originalLightIntensity;
    private bool stormActive = false;
    private GameObject shieldDome;
    private Coroutine stormRoutine;

    void Start()
    {
        if (sunLight != null)
        {
            originalLightColor = sunLight.color;
            originalLightIntensity = sunLight.intensity;
        }
    }

    public void StartSolarStorm()
    {
        if (stormActive) return;
        stormActive = true;
        if (stormRoutine != null) StopCoroutine(stormRoutine);
        stormRoutine = StartCoroutine(SolarStormPulseRoutine());
    }

    public void ActivateRadiationShield(bool fullShield)
    {
        if (shieldDome != null) Destroy(shieldDome);
        if (habitatDome == null) return;

        // Crea una esfera energética alrededor de la cúpula
        shieldDome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        shieldDome.name = "RadiationShieldFX";
        Destroy(shieldDome.GetComponent<Collider>());

        shieldDome.transform.position = habitatDome.position;
        shieldDome.transform.localScale = Vector3.one * (fullShield ? 14f : 12.5f);

        Renderer rend = shieldDome.GetComponent<Renderer>();
        if (rend != null)
        {
            // Material transparente emisivo cian (protección) o naranja (operaciones sin escudo)
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetFloat("_Surface", 1f); // Transparent
            mat.SetFloat("_Blend", 0f);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = 3000;

            Color shieldColor = fullShield
                ? new Color(0f, 0.85f, 1f, 0.22f)
                : new Color(1f, 0.45f, 0f, 0.16f);

            mat.color = shieldColor;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", shieldColor * 2.5f);
            rend.material = mat;
        }

        StartCoroutine(FinishStormAfterShield());
    }

    private IEnumerator SolarStormPulseRoutine()
    {
        Color stormColor = new Color(1f, 0.38f, 0.12f); // Naranja-rojo radiación solar
        float t = 0f;

        while (stormActive)
        {
            t += Time.deltaTime * 2.5f;
            float pulse = (Mathf.Sin(t) + 1f) * 0.5f;

            if (sunLight != null)
            {
                sunLight.color = Color.Lerp(originalLightColor, stormColor, 0.75f);
                sunLight.intensity = originalLightIntensity + pulse * 1.8f;
            }
            yield return null;
        }
    }

    private IEnumerator FinishStormAfterShield()
    {
        yield return new WaitForSeconds(3.0f);
        stormActive = false;

        if (sunLight != null)
        {
            sunLight.color = originalLightColor;
            sunLight.intensity = originalLightIntensity;
        }

        if (shieldDome != null)
        {
            Destroy(shieldDome);
        }
    }
}