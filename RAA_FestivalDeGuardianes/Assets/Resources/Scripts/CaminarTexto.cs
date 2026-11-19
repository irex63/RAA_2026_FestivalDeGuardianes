using UnityEngine;
using System.Collections;
using DialogueEditor;

[RequireComponent(typeof(CharacterController))]
public class CaminarTexto : MonoBehaviour
{
    [Header("Ruta")]
    public Transform target;
    public float speed = 2f;
    public float turnSpeed = 5f;
    public float gravity = -9.81f;

    [Header("Jugador")]
    public Transform player;
    public float startDistance = 50f;
    public float waitDistance = 7f;
    public float resumeDistance = 4f;

   [Header("Conversaciones")]
    public NPCConversation waitConversation;
    public NPCConversation endConversation;

    public float repeatWaitEvery = 6f;

    [Header("Final")]
    public bool disappearAtEnd = false;

    private CharacterController controller;
    private Animator animator;

    private float verticalVelocity = 0f;
    private float timer = 0f;

    private bool started = false;
    private bool isWaiting = false;
    private bool finished = false;
    private bool conversationActive = false;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        if (player == null)
        {
            GameObject pl = GameObject.FindGameObjectWithTag("Player");

            if (pl != null)
                player = pl.transform;
        }

        SetRunning(false);
    }

    private void Update()
    {
        if (finished || player == null || target == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        // Esperar a que el jugador se acerque para comenzar
        if (!started)
        {
            LookAtPlayer();

            if (distance <= startDistance)
            {
                started = true;
                SetRunning(true);

            }

            Move(false);
            return;
        }

        // El jugador se ha alejado demasiado
        if (!isWaiting && distance > waitDistance)
        {
            isWaiting = true;
            timer = 0f;

            SetRunning(false);
            LookAtPlayer();

            if (waitConversation != null &&
                ConversationManager.Instance != null &&
                !ConversationManager.Instance.IsConversationActive)
            {
                conversationActive = true;
                ConversationManager.Instance.StartConversation(waitConversation);
            }
        }

        // El conejo está esperando
        if (isWaiting)
        {
            SetRunning(false);
            LookAtPlayer();

            if (ConversationManager.Instance != null &&
                ConversationManager.Instance.IsConversationActive)
            {
                Move(false);
                return;
            }

            conversationActive = false;

            if (distance <= resumeDistance)
            {
                isWaiting = false;
                timer = 0f;
                SetRunning(true);
            }
            else
            {
                Move(false);
                return;
            }
        }

        // Movimiento hacia el siguiente waypoint
        LookAtTarget();
        SetRunning(true);
        Move(true);
    }

    private void Move(bool forward)
    {
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 movement = Vector3.up * verticalVelocity;

        if (forward)
            movement += transform.forward * speed;

        controller.Move(movement * Time.deltaTime);
    }

    private void LookAtTarget()
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion look = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                look,
                turnSpeed * Time.deltaTime
            );
        }
    }

    private void LookAtPlayer()
    {
        if (player == null)
            return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("WayPoint"))
            return;

        if (target == null || other.gameObject != target.gameObject)
            return;

        WayPoint wp = other.GetComponent<WayPoint>();

        if (wp != null && wp.nextWayPoint != null)
        {
            target = wp.nextWayPoint;
        }
        else
        {
            finished = true;
            SetRunning(false);

            if (endConversation != null &&
                ConversationManager.Instance != null &&
                !ConversationManager.Instance.IsConversationActive)
            {
                LookAtPlayer();
                ConversationManager.Instance.StartConversation(endConversation);
            }
        }
    }

    private void SetRunning(bool running)
    {
        if (animator != null)
            animator.SetBool("isRunning", running);
    }

}