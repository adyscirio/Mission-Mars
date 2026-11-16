using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class MovimientoJugador : MonoBehaviour
{
    [Header("Física Marciana (38% gravedad terrestre)")]
    public float velocidad = 3.5f;
    public float gravedad = -3.72f;
    public float alturaSalto = 1.2f;

    [Header("Control de Cámara 360° (Teclas Q/E y Mouse)")]
    public float velocidadGiroTeclado = 110f;
    public float sensibilidadMouse = 0.35f;
    public float distanciaCamara = 6.5f;
    public float alturaFoco = 1.6f;
    public float minPitch = 5f;
    public float maxPitch = 65f;
    public Transform objetivoInicial;

    private CharacterController controller;
    private Animator animator;
    private Vector3 velocidadVertical;
    private float tiempoEnElAire = 0f;

    private float yaw = 0f;
    private float pitch = 18f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (objetivoInicial != null)
        {
            Vector3 direccionBase = objetivoInicial.position - transform.position;
            direccionBase.y = 0f;
            if (direccionBase != Vector3.zero)
                transform.forward = direccionBase.normalized;
        }

        yaw = transform.eulerAngles.y;
        pitch = 18f;
    }

    void Update()
    {
        // 1. Detectar suelo
        if (controller.isGrounded)
        {
            tiempoEnElAire = 0f;
            if (velocidadVertical.y < 0)
                velocidadVertical.y = -1.5f;
        }
        else
        {
            tiempoEnElAire += Time.deltaTime;
        }

        // 2. Leer movimiento (WASD o Flechas) y rotación 360° de cámara (Q / E)
        Vector2 input = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1;

            // Rotación 360° continua con Q (izquierda) y E (derecha)
            if (Keyboard.current.qKey.isPressed) yaw -= velocidadGiroTeclado * Time.deltaTime;
            if (Keyboard.current.eKey.isPressed) yaw += velocidadGiroTeclado * Time.deltaTime;
        }

        // 3. Mover y orientar astronauta respecto a la cámara
        Vector3 adelanteCam = Camera.main != null ? Camera.main.transform.forward : transform.forward;
        Vector3 derechaCam = Camera.main != null ? Camera.main.transform.right : transform.right;
        adelanteCam.y = 0f;
        derechaCam.y = 0f;

        Vector3 movimiento = (adelanteCam.normalized * input.y + derechaCam.normalized * input.x).normalized;
        controller.Move(movimiento * velocidad * Time.deltaTime);

        bool estaCaminando = movimiento != Vector3.zero;
        if (estaCaminando)
            transform.forward = movimiento;

        // 4. Controlar animación
        if (animator != null)
        {
            animator.SetBool("Caminando", estaCaminando);
            animator.speed = estaCaminando ? 0.85f : 0f;
        }

        // 5. Salto marciano con Space
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && tiempoEnElAire < 0.15f)
        {
            velocidadVertical.y = Mathf.Sqrt(alturaSalto * -2f * gravedad);
            tiempoEnElAire = 1f;
        }

        velocidadVertical.y += gravedad * Time.deltaTime;
        controller.Move(velocidadVertical * Time.deltaTime);
    }

    void LateUpdate()
    {
        if (Camera.main == null) return;

        // Rotar 360° arrastrando con clic izquierdo (si no está sobre un botón UI) o clic derecho
        if (Mouse.current != null)
        {
            bool sobreUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            bool arrastrandoMouse = (Mouse.current.leftButton.isPressed && !sobreUI) || Mouse.current.rightButton.isPressed;

            if (arrastrandoMouse)
            {
                Vector2 delta = Mouse.current.delta.ReadValue();
                yaw += delta.x * sensibilidadMouse;
                pitch -= delta.y * sensibilidadMouse;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }
        }

        Quaternion rotacion = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 foco = transform.position + Vector3.up * alturaFoco;

        Camera.main.transform.position = foco - (rotacion * Vector3.forward * distanciaCamara);
        Camera.main.transform.LookAt(foco);
    }
}