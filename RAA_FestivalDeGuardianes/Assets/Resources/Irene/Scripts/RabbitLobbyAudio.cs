
using UnityEngine;
using System.Collections;

public class RabbitLobbyAudio : MonoBehaviour
{
    [Header("Audios")]
    public AudioSource audioSource;
    public AudioClip rabbitNotPlayed;
    public AudioClip rabbitPlayed;

    [Header("Orientacion del jugador")]
    public Transform playerBody;
    public Transform rabbitLookTarget;

    [Header("Bloqueo del jugador")]
    public Behaviour playerMovement;

    [Header("Camara")]
    public Camera playerCamera;

    private bool playerInRange;
    private bool audioRunning;
    private bool movementWasEnabled;
    private bool movementWasCaptured;

    private Coroutine audioCoroutine;

    private void OnDisable()
    {
        if (audioCoroutine != null)
        {
            StopCoroutine(audioCoroutine);
            audioCoroutine = null;
        }

        if (audioSource != null)
            audioSource.Stop();

        audioRunning = false;
        RestorePlayerMovement();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
            return;

        if (playerInRange)
            return;

        playerInRange = true;

        RotatePlayerTowardsRabbit();
        CheckAudio();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
            return;

        playerInRange = false;
    }

    private bool IsPlayer(Collider other)
    {
        return other.CompareTag("Player") ||
               other.transform.root.CompareTag("Player");
    }

    private void RotatePlayerTowardsRabbit()
    {
        if (playerBody == null || playerCamera == null)
        {
            Debug.LogWarning(
                "RabbitLobbyAudio: falta asignar Player Body o Player Camera."
            );
            return;
        }

        if (!playerCamera.transform.IsChildOf(playerBody))
        {
            Debug.LogWarning(
                "RabbitLobbyAudio: Player Camera debe estar dentro de Player Body."
            );
            return;
        }

        Transform target = rabbitLookTarget != null
            ? rabbitLookTarget
            : transform;

        Vector3 direction =
            target.position - playerCamera.transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Vector3 cameraForward = playerCamera.transform.forward;
        cameraForward.y = 0f;

        if (cameraForward.sqrMagnitude < 0.001f)
            return;

        cameraForward.Normalize();
        direction.Normalize();

        Quaternion difference =
            Quaternion.FromToRotation(cameraForward, direction);

        Quaternion newRotation =
            difference * playerBody.rotation;

        Vector3 cameraPosition = playerCamera.transform.position;

        playerBody.rotation = newRotation;

        playerBody.position +=
            cameraPosition - playerCamera.transform.position;
    }

    private void CheckAudio()
    {
        if (audioRunning)
            return;

        if (audioSource == null)
        {
            Debug.LogWarning(
                "RabbitLobbyAudio: falta asignar el AudioSource."
            );
            return;
        }

        int levelPlayed = PlayerPrefs.GetInt(
            PlayerPrefsNames.easter,
            PlayerPrefsNames.noPlayed
        );

        AudioClip clip;

        if (levelPlayed == PlayerPrefsNames.played)
        {
            clip = rabbitPlayed;
        }
        else
        {
            clip = rabbitNotPlayed;
        }

        StartRabbitAudio(clip);
    }

    private void StartRabbitAudio(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning(
                "RabbitLobbyAudio: falta asignar el audio correspondiente."
            );
            return;
        }

        if (audioSource == null || audioSource.isPlaying)
            return;

        audioRunning = true;

        if (playerMovement != null)
        {
            movementWasEnabled = playerMovement.enabled;
            movementWasCaptured = true;
            playerMovement.enabled = false;
        }

        audioCoroutine = StartCoroutine(PlayRabbitAudio(clip));
    }

    private IEnumerator PlayRabbitAudio(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();

        while (audioSource.isPlaying)
        {
            yield return null;
        }

        audioRunning = false;
        audioCoroutine = null;

        RestorePlayerMovement();

        Debug.Log("RabbitLobbyAudio: audio terminado.");
    }

    private void RestorePlayerMovement()
    {
        if (playerMovement != null && movementWasCaptured)
        {
            playerMovement.enabled = movementWasEnabled;
            movementWasCaptured = false;
        }
    }
}
