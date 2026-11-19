using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class WayPointCharacterGaby : MonoBehaviour
{
    [Header("Ruta")]
    public Transform target;
    public float speed = 2f;
    public float turnSpeed = 5f;
    public float gravity = -9.81f;

    [Header("Distancias al jugador")]
    public float startDistance = 50f;
    public float maxDistance = 7f;
    public float resumeDistance = 4f;

    [Header("Jugador")]
    public MovementGaby playerMovement;

    [Header("Audios del conejo")]
    public AudioSource audioSource;
    public AudioClip audioInicio;
    public AudioClip audioEspera;
    public AudioClip audioFinal;

    [Header("Repetición del audio de espera")]
    public float repeatWaitEvery = 6f;

    private enum State
    {
        WaitingStart,
        Running,
        WaitingPlayer,
        Finished
    }

    private State state = State.WaitingStart;

    private CharacterController controller;
    private Animator animator;
    private bool hasRunParam = false;

    private Transform player;
    private float verticalVelocity = 0f;
    private float timer = 0f;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        if (animator != null)
        {
            foreach (AnimatorControllerParameter p in animator.parameters)
            {
                if (p.name == "isRunning")
                    hasRunParam = true;
            }
        }

        GameObject pl = GameObject.FindGameObjectWithTag("Player");

        if (pl != null)
            player = pl.transform;

        SetRunning(false);

        if (player == null)
            Debug.LogError("WayPointCharacterGaby: no se encuentra el Player.");
    }

    private void Update()
    {
        if (player == null)
            return;

        float dist = Vector3.Distance(transform.position, player.position);
        bool moveForward = false;

        switch (state)
        {
            case State.WaitingStart:

                LookAt(player.position);

                if (dist <= startDistance)
                {
                    PlayAudio(audioInicio);
                    state = State.Running;
                    SetRunning(true);
                }

                break;

            case State.Running:

                if (target != null)
                {
                    LookAt(target.position);
                    moveForward = true;
                }

                if (dist > maxDistance)
                {
                    state = State.WaitingPlayer;
                    SetRunning(false);
                    timer = 0f;

                    PlayAudio(audioEspera);
                }

                break;

            case State.WaitingPlayer:

                LookAt(player.position);

                timer += Time.deltaTime;

                if (timer >= repeatWaitEvery)
                {
                    timer = 0f;
                    PlayAudio(audioEspera);
                }

                if (dist <= resumeDistance)
                {
                    state = State.Running;
                    SetRunning(true);

                    if (audioSource != null)
                        audioSource.Stop();
                }

                break;

            case State.Finished:

                LookAt(player.position);

                break;
        }

        Move(moveForward);
    }

    // ---------- MOVIMIENTO ----------

    private void Move(bool forward)
    {
        if (controller == null)
            return;

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 movement = Vector3.up * verticalVelocity;

        if (forward)
            movement += transform.forward * speed;

        controller.Move(movement * Time.deltaTime);
    }

    private void LookAt(Vector3 point)
    {
        Vector3 direction = point - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion rotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotation,
                turnSpeed * Time.deltaTime
            );
        }
    }

    // ---------- WAYPOINTS ----------

    private void OnTriggerEnter(Collider other)
    {
        if (state != State.Running)
            return;

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
            state = State.Finished;

            SetRunning(false);

            PlayAudio(audioFinal);
        }
    }

    // ---------- ANIMACIÓN ----------

    private void SetRunning(bool running)
    {
        if (animator != null && hasRunParam)
            animator.SetBool("isRunning", running);
    }

    // ---------- AUDIO ----------

    private void PlayAudio(AudioClip clip)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }
}