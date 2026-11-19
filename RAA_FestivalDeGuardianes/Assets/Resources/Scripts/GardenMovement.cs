using UnityEngine;
// Movimiento de Alicia en el jardín: igual que Movement (bosque), pero sin
// madriguera y con un interruptor para congelarla durante las conversaciones.
[RequireComponent(typeof(CharacterController))]
public class GardenMovement : MonoBehaviour
{
    public float speed = 2.5f; // velocidad de paseo (m/s)
    public float acceleration = 12f; // arranque y parada suaves
    public float minStepTime = 0.3f; // un toque breve dura al menos esto
    public float gravity = -9.81f;
    public KeyCode forwardButton = KeyCode.JoystickButton0; // botón A
    public bool useVisorButton = true; // el botón del visor también avanza
                                       // Los diálogos lo activan para detener a Alicia
    public bool frozen = false;
    private CharacterController controller;
    private Vector3 horizontalVelocity;
    private Vector3 stepDirection;
    private float stepTimer;
    private float verticalVelocity;
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // 1) Entrada: stick, flechas, WASD, botón A o botón del visor.
        // Si Alicia está congelada, no hay entrada.
        Vector3 input = Vector3.zero;
        if (!frozen)
        {
            input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f,
            Input.GetAxisRaw("Vertical"));
            if (Input.GetKey(forwardButton) ||
            (useVisorButton && Input.GetMouseButton(0)))
            {
                input.z = 1f;
            }
            input = Vector3.ClampMagnitude(input, 1f);
        }// 2) Dirección: relativa a hacia dónde mira la cabeza
        if (input.sqrMagnitude > 0.01f && Camera.main != null)
        {
            Vector3 forward = Camera.main.transform.forward;
            forward.y = 0f;
            forward.Normalize();
            Vector3 right = Camera.main.transform.right;
            right.y = 0f;
            right.Normalize();
            stepDirection = (forward * input.z + right * input.x).normalized;
            stepTimer = minStepTime;
        }
        else
        {
            stepTimer -= Time.deltaTime;
        }
        if (frozen) stepTimer = 0f; // al congelarla, deja de avanzar ya
                                    // 3) Velocidad horizontal con aceleración suave
        Vector3 target = (stepTimer > 0f) ? stepDirection * speed : Vector3.zero;
        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, target,
        acceleration * Time.deltaTime);
        // 4) Gravedad: se aplica siempre, aunque Alicia esté congelada
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
        verticalVelocity += gravity * Time.deltaTime;
        // 5) Una única llamada a Move por fotograma
        controller.Move((horizontalVelocity + Vector3.up * verticalVelocity)
        * Time.deltaTime);
    }
}
