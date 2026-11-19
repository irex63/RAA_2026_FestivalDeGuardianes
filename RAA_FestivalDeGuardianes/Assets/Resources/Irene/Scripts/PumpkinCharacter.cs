
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController))]
public class PumpkinCharacter : MonoBehaviour
{
    // =============================================
    // RUTA
    // =============================================

    [Header("Ruta")]

    public Transform target;
    public float speed = 2f;
    public float turnSpeed = 5f;
    public float waypointDistance = 0.3f;


    // =============================================
    // PROTAGONISTA
    // =============================================

    [Header("Player")]

    public Transform player;
    public float maxDistanceToPlayer = 6f;
    public float resumeDistance = 4f;


    // =============================================
    // SALIDA FINAL
    // =============================================

    [Header("Salida final")]

    public Vector3 exitDirection = Vector3.left;
    public float exitDistance = 50f;
    public bool disappearAtEnd = true;

    private Vector3 exitStartPosition;


    // =============================================
    // ANIMACIONES
    // =============================================

    [Header("Animacion")]

    public Animator animator;
    public string walkingParameter = "IsWalking";


    // =============================================
    // EVENTOS DE CONVERSACION
    // =============================================

    [Header("Eventos de conversacion")]

    public UnityEvent onInitialDialogue;
    public UnityEvent onPlayerTooFar;
    public UnityEvent onReachedDestination;


    // =============================================
    // ESTADOS
    // =============================================

    private enum State
    {
        Waiting,
        Following,
        FinalDialogue,
        Leaving,
        Finished
    }

    private State state = State.Waiting;

    private CharacterController controller;

    private bool isPausedForDialogue = false;

    private bool initialDialogueTriggered = false;
    private bool tooFarDialogueTriggered = false;
    private bool finalDialogueTriggered = false;


    // =============================================
    // INICIALIZACION
    // =============================================

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        exitDirection.y = 0f;

        if (exitDirection.sqrMagnitude > 0.001f)
            exitDirection.Normalize();

        SetWalking(false);
    }


    // =============================================
    // ACTUALIZACION PRINCIPAL
    // =============================================

    void Update()
    {
        // Si hay una conversacion activa,
        // el personaje permanece quieto.

        if (isPausedForDialogue)
        {
            SetWalking(false);

            // Puede seguir mirando al protagonista.
            if (player != null)
                LookAt(player.position);

            return;
        }

        switch (state)
        {
            case State.Waiting:
                UpdateWaiting();
                break;

            case State.Following:
                UpdateFollowing();
                break;

            case State.FinalDialogue:
                SetWalking(false);
                break;

            case State.Leaving:
                UpdateLeaving();
                break;

            case State.Finished:
                SetWalking(false);
                break;
        }
    }


    // =============================================
    // ESPERAR AL PROTAGONISTA
    // =============================================

    void UpdateWaiting()
    {
        SetWalking(false);

        if (player == null)
            return;

        LookAt(player.position);

        // Activar la conversacion inicial una sola vez.

        if (!initialDialogueTriggered)
        {
            initialDialogueTriggered = true;

            PauseForDialogue();

            onInitialDialogue?.Invoke();

            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        // Reanudar cuando el protagonista se acerque.

        if (distance <= resumeDistance)
        {
            tooFarDialogueTriggered = false;

            state = State.Following;
        }
    }


    // =============================================
    // SEGUIR LOS WAYPOINTS
    // =============================================

    void UpdateFollowing()
    {
        if (target == null)
        {
            SetWalking(false);
            return;
        }

        // Comprobar distancia al protagonista.

        if (player != null)
        {
            float distance = Vector3.Distance(
                transform.position,
                player.position
            );

            if (distance > maxDistanceToPlayer)
            {
                state = State.Waiting;

                SetWalking(false);

                // Activar el aviso una vez por cada episodio
                // en el que el protagonista se queda atras.

                if (!tooFarDialogueTriggered)
                {
                    tooFarDialogueTriggered = true;

                    PauseForDialogue();

                    onPlayerTooFar?.Invoke();
                }

                return;
            }
        }

        // Calcular direccion al waypoint.

        Vector3 direction =
            target.position - transform.position;

        direction.y = 0f;

        // Comprobar si hemos llegado.

        if (direction.magnitude <= waypointDistance)
        {
            AdvanceWaypoint();
            return;
        }

        direction.Normalize();

        RotateTowards(direction);

        SetWalking(true);

        controller.SimpleMove(direction * speed);
    }


    // =============================================
    // SIGUIENTE WAYPOINT
    // =============================================

    void AdvanceWaypoint()
    {
        RouteWayPoints waypoint =
            target.GetComponent<RouteWayPoints>();

        if (waypoint != null &&
            waypoint.nextWayPoint != null)
        {
            target = waypoint.nextWayPoint;
        }
        else
        {
            // Hemos llegado al destino final.

            state = State.FinalDialogue;

            SetWalking(false);

            PauseForDialogue();

            // Activar conversacion final.

            if (!finalDialogueTriggered)
            {
                finalDialogueTriggered = true;

                onReachedDestination?.Invoke();
            }
        }
    }


    // =============================================
    // CONTROL DE CONVERSACIONES
    // =============================================

    public void PauseForDialogue()
    {
        isPausedForDialogue = true;

        SetWalking(false);
    }


    public void ResumeAfterDialogue()
    {
        isPausedForDialogue = false;
    }


    // =============================================
    // COMENZAR DESPEDIDA
    // =============================================

    public void StartLeaving()
    {
        if (state != State.FinalDialogue)
            return;

        state = State.Leaving;

        isPausedForDialogue = false;

        exitStartPosition = transform.position;

        exitDirection.y = 0f;

        if (exitDirection.sqrMagnitude > 0.001f)
            exitDirection.Normalize();
    }


    // =============================================
    // SALIDA HACIA LA IZQUIERDA
    // =============================================

    void UpdateLeaving()
    {
        SetWalking(true);

        RotateTowards(exitDirection);

        controller.SimpleMove(exitDirection * speed);

        // Distancia recorrida en la direccion de salida.

        float distanceTravelled = Vector3.Dot(
            transform.position - exitStartPosition,
            exitDirection
        );

        if (distanceTravelled >= exitDistance)
        {
            state = State.Finished;

            SetWalking(false);

            if (disappearAtEnd)
            {
                gameObject.SetActive(false);
            }
        }
    }


    // =============================================
    // ROTACION
    // =============================================

    void RotateTowards(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            turnSpeed * 100f * Time.deltaTime
        );
    }


    void LookAt(Vector3 point)
    {
        Vector3 direction = point - transform.position;

        direction.y = 0f;

        RotateTowards(direction);
    }


    // =============================================
    // ANIMACIONES
    // =============================================

    void SetWalking(bool walking)
    {
        if (animator != null)
        {
            animator.SetBool(
                walkingParameter,
                walking
            );
        }
    }
}
