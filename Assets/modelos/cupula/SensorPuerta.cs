using UnityEngine;

public class SensorPuerta : MonoBehaviour
{
    [Header("Pieza de la puerta a mover")]
    public Transform piezaPuerta;
    public Vector3 desplazamientoAbierta = new Vector3(0f, 3.5f, 0f);
    public float velocidad = 3f;

    private Vector3 posicionCerrada;
    private Vector3 posicionObjetivo;

    void Start()
    {
        if (piezaPuerta != null)
        {
            posicionCerrada = piezaPuerta.localPosition;
            posicionObjetivo = posicionCerrada;
        }
    }

    void Update()
    {
        if (piezaPuerta != null)
        {
            piezaPuerta.localPosition = Vector3.Lerp(piezaPuerta.localPosition, posicionObjetivo, Time.deltaTime * velocidad);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            posicionObjetivo = posicionCerrada + desplazamientoAbierta;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            posicionObjetivo = posicionCerrada;
        }
    }
}