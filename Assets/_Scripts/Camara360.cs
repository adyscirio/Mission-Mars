using UnityEngine;
using UnityEngine.InputSystem;

public class Camara360 : MonoBehaviour
{
    public Transform objetivo; // Arrastra aquí a Astronauta_Caminando
    public float distancia = 4.5f;
    public float altura = 1.8f;
    public float sensibilidadRaton = 3f;
    public float velocidadTeclado = 110f;

    private float rotacionX = 0f;
    private float rotacionY = 15f;

    void LateUpdate()
    {
        if (objetivo == null) return;

        float giroX = 0f;
        float giroY = 0f;

        // 1. Leer teclado con el nuevo Input System (Q / E o teclas J / L)
        if (Keyboard.current != null)
        {
            if (Keyboard.current.qKey.isPressed || Keyboard.current.jKey.isPressed) giroX -= 1f;
            if (Keyboard.current.eKey.isPressed || Keyboard.current.lKey.isPressed) giroX += 1f;
        }

        rotacionX += giroX * velocidadTeclado * Time.deltaTime;

        // 2. Leer arrastre del ratón / trackpad manteniendo clic izquierdo o derecho
        if (Mouse.current != null && (Mouse.current.leftButton.isPressed || Mouse.current.rightButton.isPressed))
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            rotacionX += delta.x * (sensibilidadRaton * 0.1f);
            giroY = delta.y * (sensibilidadRaton * 0.1f);
            rotacionY = Mathf.Clamp(rotacionY - giroY, -10f, 75f);
        }

        Quaternion rotacion = Quaternion.Euler(rotacionY, rotacionX, 0);
        Vector3 posicion = objetivo.position + Vector3.up * altura - (rotacion * Vector3.forward * distancia);

        transform.position = posicion;
        transform.LookAt(objetivo.position + Vector3.up * (altura * 0.8f));
    }
}