using UnityEngine;

public class CamaraSeguimiento : MonoBehaviour
{
    public Transform objetivo; // Aquí arrastraremos al Astronauta
    public Vector3 distancia = new Vector3(0f, 0.08f, -0.12f);

    void LateUpdate()
    {
        if (objetivo != null)
        {
            transform.position = objetivo.position + distancia;
            transform.LookAt(objetivo.position + Vector3.up * 0.02f);
        }
    }
}