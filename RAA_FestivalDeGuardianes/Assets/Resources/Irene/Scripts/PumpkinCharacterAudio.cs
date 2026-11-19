
using System.Collections;
using UnityEngine;

public class PumpkinCharacterAudio : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;

    public AudioClip initialAudio;
    public AudioClip waitingAudio;
    public AudioClip finalAudio;

    [Header("Delay del audio inicial")]
    public float initialDialogueDelay = 2f;

    [Header("Referencias")]
    public PumpkinCharacter pumpkinCharacter;

    [Tooltip("Componente que controla el movimiento del jugador")]
    public Behaviour playerMovement;

    private enum AudioType
    {
        None,
        Initial,
        Waiting,
        Final
    }

    private AudioType currentAudio = AudioType.None;

    private bool movementWasEnabled;
    private bool audioRunning;

    private Coroutine initialDelayRoutine;
    private Coroutine audioRoutine;

    private void OnDisable()
    {
        StopAllCoroutines();

        initialDelayRoutine = null;
        audioRoutine = null;

        if (audioSource != null)
            audioSource.Stop();

        audioRunning = false;
        currentAudio = AudioType.None;

        RestorePlayerMovement();
    }

    // AUDIO INICIAL

    public void StartInitialDialogue()
    {
        if (audioRunning || initialDelayRoutine != null)
            return;

        initialDelayRoutine =
            StartCoroutine(StartInitialAudioWithDelay());
    }

    private IEnumerator StartInitialAudioWithDelay()
    {
        yield return new WaitForSeconds(initialDialogueDelay);

        initialDelayRoutine = null;

        StartPumpkinAudio(
            initialAudio,
            AudioType.Initial
        );
    }

    // AUDIO CUANDO EL JUGADOR SE ALEJA

    public void StartWaitingDialogue()
    {
        StartPumpkinAudio(
            waitingAudio,
            AudioType.Waiting
        );
    }

    // AUDIO FINAL

    public void StartFinalDialogue()
    {
        StartPumpkinAudio(
            finalAudio,
            AudioType.Final
        );
    }

    private void StartPumpkinAudio(AudioClip clip, AudioType type)
    {
        if (audioRunning)
            return;

        if (audioSource == null)
        {
            Debug.LogError("PumpkinAudio: falta asignar el AudioSource.");
            return;
        }

        if (clip == null)
        {
            Debug.LogError("PumpkinAudio: falta asignar el AudioClip.");
            return;
        }

        currentAudio = type;
        audioRunning = true;

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
        if (!audioRunning)
            return;

        audioRunning = false;

        AudioType finishedAudio = currentAudio;
        currentAudio = AudioType.None;

        switch (finishedAudio)
        {
            case AudioType.Initial:
            case AudioType.Waiting:

                if (pumpkinCharacter != null)
                    pumpkinCharacter.ResumeAfterDialogue();

                break;

            case AudioType.Final:

                if (pumpkinCharacter != null)
                    pumpkinCharacter.StartLeaving();

                break;
        }

        RestorePlayerMovement();
    }

    private void RestorePlayerMovement()
    {
        if (playerMovement != null)
            playerMovement.enabled = movementWasEnabled;
    }
}
