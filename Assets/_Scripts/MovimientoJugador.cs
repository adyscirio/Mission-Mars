using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class MovimientoJugador : MonoBehaviour
{
    [Header("Física Marciana (38% gravedad terrestre)")]
    public float velocidad = 3.5f;
    public float gravedad = -3.72f;
    public float alturaSalto = 1.2f;

    private CharacterController controller;
    private Animator animator;
    private Vector3 velocidadVertical;
    private float tiempoEnElAire = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 1. Detectar suelo con pequeño margen de tolerancia para terrenos irregulares
        if (controller.isGrounded)
        {
            tiempoEnElAire = 0f;
            if (velocidadVertical.y < 0)
            {
                velocidadVertical.y = -1.5f;
            }
        }
        else
        {
            tiempoEnElAire += Time.deltaTime;
        }

        // 2. Leer teclado (WASD o Flechas)
        Vector2 input = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1;
        }

        // 3. Mover y rotar
        Vector3 movimiento = new Vector3(input.x, 0, input.y).normalized;
        controller.Move(movimiento * velocidad * Time.deltaTime);

        bool estaCaminando = movimiento != Vector3.zero;
        if (estaCaminando)
        {
            transform.forward = movimiento;
        }

        // 4. Controlar animación de pies sin hundir el modelo
        if (animator != null)
        {
            animator.SetBool("Caminando", estaCaminando);
            animator.speed = estaCaminando ? 0.85f : 0f; // Pausa de pie cuando no camina
        }

        // 5. Salto marciano con Barra Espaciadora (con margen de 0.15s)
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && tiempoEnElAire < 0.15f)
        {
            velocidadVertical.y = Mathf.Sqrt(alturaSalto * -2f * gravedad);
            tiempoEnElAire = 1f; // Evita doble salto en el aire
        }

        velocidadVertical.y += gravedad * Time.deltaTime;
        controller.Move(velocidadVertical * Time.deltaTime);
    }
}