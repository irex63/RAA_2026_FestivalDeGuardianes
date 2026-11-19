
using System.Collections;
using UnityEngine;

public class CatConversationAudio : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Audios de la Gata")]
    public AudioClip catInitialAudio;
    public AudioClip catNoBookAudio;
    public AudioClip catZombieAudio;
    public AudioClip catBookAudio;
    public AudioClip catNoIngredientsAudio;
    public AudioClip catFinalAudio;

    [Header("Inventario")]
    public HalloweenInventory inventory;

    [Header("Orientacion del jugador")]
    public Transform playerBody;
    public Transform catLookTarget;

    [Header("Bloqueo del jugador")]
    public Behaviour playerMovement;

    [Header("Camara")]
    public Camera playerCamera;

    private enum AudioType
    {
        None,
        Initial,
        NoBook,
        Book,
        NoIngredients,
        Final,
        Zombie
    }

    private AudioType currentAudioType = AudioType.None;

    private bool playerInRange;
    private bool initialAudioFinished;
    private bool hasReturnedWithBook;
    private bool finalAudioFinished;

    private bool catAudioRunning;
    private bool movementWasEnabled;

    private Coroutine audioRoutine;

    private void OnDisable()
    {
        StopAllCoroutines();

        audioRoutine = null;

        if (audioSource != null)
            audioSource.Stop();

        catAudioRunning = false;
        currentAudioType = AudioType.None;

        RestorePlayerMovement();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
            return;

        if (playerInRange)
            return;

        playerInRange = true;

        RotatePlayerTowardsCat();

        CheckConversation();
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

    // Girar el XR Origin usando la orientacion real de la camara.

    private void RotatePlayerTowardsCat()
    {
        if (playerBody == null || playerCamera == null)
        {
            Debug.LogWarning(
                "Gata: falta asignar Player Body o Player Camera."
            );

            return;
        }

        if (!playerCamera.transform.IsChildOf(playerBody))
        {
            Debug.LogWarning(
                "Gata: Player Camera debe estar dentro de Player Body."
            );

            return;
        }

        Transform target = catLookTarget != null
            ? catLookTarget
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

    private void CheckConversation()
    {
        if (inventory == null)
        {
            Debug.LogWarning("Falta asignar HalloweenInventory.");
            return;
        }

        if (catAudioRunning)
            return;

        if (finalAudioFinished)
            return;

        // Conversacion especial por el zombi.
        if (inventory.WasCaughtByZombie())
        {
            StartCatAudio(
                catZombieAudio,
                AudioType.Zombie
            );

            return;
        }

        // Conversacion inicial.
        if (!initialAudioFinished)
        {
            StartCatAudio(
                catInitialAudio,
                AudioType.Initial
            );

            return;
        }

        // Jugador sin libro.
        if (!inventory.HasBook())
        {
            StartCatAudio(
                catNoBookAudio,
                AudioType.NoBook
            );

            return;
        }

        // Todos los ingredientes recogidos.
        if (inventory.getTakenIngredients() >= 3)
        {
            StartCatAudio(
                catFinalAudio,
                AudioType.Final
            );

            return;
        }

        // Jugador vuelve con el libro.
        if (!hasReturnedWithBook)
        {
            StartCatAudio(
                catBookAudio,
                AudioType.Book
            );

            return;
        }

        // Faltan ingredientes.
        StartCatAudio(
            catNoIngredientsAudio,
            AudioType.NoIngredients
        );
    }

    private void StartCatAudio(AudioClip clip, AudioType type)
    {
        if (audioSource == null)
        {
            Debug.LogError("Gata: falta asignar el AudioSource.");
            return;
        }

        if (clip == null)
        {
            Debug.LogError(
                "Gata: falta asignar el AudioClip correspondiente: " + type
            );

            return;
        }

        if (catAudioRunning)
            return;

        currentAudioType = type;
        catAudioRunning = true;

        if (playerMovement != null)
        {
            movementWasEnabled = playerMovement.enabled;
            playerMovement.enabled = false;
        }

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.loop = false;
        audioSource.Play();

        audioRoutine = StartCoroutine(WaitForAudio());
    }

    private IEnumerator WaitForAudio()
    {
        while (audioSource != null && audioSource.isPlaying)
        {
            yield return null;
        }

        audioRoutine = null;

        OnAudioEnded();
    }

    private void OnAudioEnded()
    {
        if (!catAudioRunning)
            return;

        catAudioRunning = false;

        RestorePlayerMovement();

        AudioType finishedAudio = currentAudioType;
        currentAudioType = AudioType.None;

        switch (finishedAudio)
        {
            case AudioType.Initial:

                initialAudioFinished = true;

                inventory.UnlockObjectInteraction();

                Debug.Log("Gata: conversacion inicial terminada.");

                break;

            case AudioType.Book:

                hasReturnedWithBook = true;

                inventory.UnlockIngredientInteraction();

                Debug.Log("Gata: conversacion del libro terminada.");

                break;

            case AudioType.Final:

                finalAudioFinished = true;

                inventory.UnlockCauldronInteraction();

                Debug.Log("Gata: conversacion final terminada.");

                break;

            case AudioType.Zombie:

                inventory.ResetCaughtByZombie();

                Debug.Log("Gata: conversacion del zombi terminada.");

                break;
        }
    }

    private void RestorePlayerMovement()
    {
        if (playerMovement != null)
            playerMovement.enabled = movementWasEnabled;
    }
}
