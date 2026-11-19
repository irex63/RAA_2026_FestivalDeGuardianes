using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class Caminar : MonoBehaviour
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

    [Header("Audios")]
    public AudioSource audioSource;
    public AudioClip audioInicio;
    public AudioClip audioEspera;
    public AudioClip audioFinal;
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
    private bool waitingForStartAudio = false;

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

        if (waitingForStartAudio)
        {
            SetRunning(false);
            Move(false);
            return;
        }

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
                StartCoroutine(StartWalkingAfterAudio());
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

            PlayAudio(audioEspera);
        }

        // El conejo está esperando
        if (isWaiting)
        {
            SetRunning(false);
            LookAtPlayer();

            timer += Time.deltaTime;

            if (timer >= repeatWaitEvery)
            {
                timer = 0f;
                PlayAudio(audioEspera);
            }

            // No reanuda hasta que el jugador esté suficientemente cerca
            if (distance <= resumeDistance)
            {
                isWaiting = false;
                timer = 0f;

                if (audioSource != null)
                    audioSource.Stop();

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

            if (audioSource != null)
            {
                audioSource.Stop();
                PlayAudio(audioFinal);
            }

            if (disappearAtEnd && audioFinal != null)
                StartCoroutine(DisappearAfterAudio());
            else if (disappearAtEnd)
                gameObject.SetActive(false);
        }
    }

    private void SetRunning(bool running)
    {
        if (animator != null)
            animator.SetBool("isRunning", running);
    }

    private void PlayAudio(AudioClip clip)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    private IEnumerator DisappearAfterAudio()
    {
        yield return new WaitForSeconds(audioFinal.length);
        gameObject.SetActive(false);
    }

    private IEnumerator StartWalkingAfterAudio()
    {
        waitingForStartAudio = true;

        SetRunning(false);
        PlayAudio(audioInicio);

        if (audioInicio != null)
            yield return new WaitForSeconds(audioInicio.length);

        yield return new WaitForSeconds(1f);

        waitingForStartAudio = false;
        SetRunning(true);
    }
}