
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DialogueEditor;
using System.Collections;
using System.Collections.Generic;

public class PumpkinCharacterText : MonoBehaviour
{
    [Header("Botones para confirmar")]
    public KeyCode mandoButton = KeyCode.JoystickButton2;
    public KeyCode key = KeyCode.E;

    [Header("Delay conversacion inicial")]
    public float initialDialogueDelay = 2f;

    [Header("Referencias")]
    public PumpkinCharacter pumpkinCharacter;

    [Tooltip("Componente que controla el movimiento del jugador")]
    public Behaviour playerMovement;

    [Tooltip("Camara principal del jugador VR")]
    public Camera playerCamera;

    [Header("Conversaciones")]
    public NPCConversation initialConversation;
    public NPCConversation waitingConversation;
    public NPCConversation finalConversation;

    private Button currentButton;

    private enum DialogueType
    {
        None,
        Initial,
        Waiting,
        Final
    }

    private DialogueType currentDialogue = DialogueType.None;

    private bool movementWasEnabled;
    private bool pumpkinConversationRunning;

    private void OnEnable()
    {
        ConversationManager.OnConversationEnded += OnConversationEnded;
    }

    private void OnDisable()
    {
        ConversationManager.OnConversationEnded -= OnConversationEnded;

        pumpkinConversationRunning = false;
        currentButton = null;

        RestorePlayerMovement();
    }

    // Iniciar conversaciones.

    public void StartInitialDialogue()
    {
        StartCoroutine(StartInitialDialogueWithDelay());
    }

    private IEnumerator StartInitialDialogueWithDelay()
    {
        yield return new WaitForSeconds(initialDialogueDelay);

        StartPumpkinConversation(
            initialConversation,
            DialogueType.Initial
        );
    }

    public void StartWaitingDialogue()
    {
        StartPumpkinConversation(
            waitingConversation,
            DialogueType.Waiting
        );
    }

    public void StartFinalDialogue()
    {
        StartPumpkinConversation(
            finalConversation,
            DialogueType.Final
        );
    }

    private void StartPumpkinConversation(
        NPCConversation conversation,
        DialogueType type)
    {
        if (conversation == null ||
            ConversationManager.Instance == null)
        {
            Debug.LogError(
                "Calabaza: falta una conversacion o ConversationManager."
            );
            return;
        }

        if (ConversationManager.Instance.IsConversationActive)
            return;

        currentDialogue = type;
        pumpkinConversationRunning = true;

        currentButton = null;

        if (playerMovement != null)
        {
            movementWasEnabled = playerMovement.enabled;
            playerMovement.enabled = false;
        }

        ConversationManager.Instance.StartConversation(conversation);
    }

    private void Update()
    {
        // Este script solo controla las conversaciones de la Calabaza.
        if (!pumpkinConversationRunning)
            return;

        if (ConversationManager.Instance == null ||
            !ConversationManager.Instance.IsConversationActive)
            return;

        LookAtOption();

        if (Input.GetKeyDown(mandoButton) ||
            Input.GetKeyDown(key))
        {
            ConfirmOption();
        }
    }

    private void LookAtOption()
    {
        if (playerCamera == null ||
            EventSystem.current == null)
            return;

        PointerEventData pointer =
            new PointerEventData(EventSystem.current);

        pointer.position = new Vector2(
            Screen.width / 2f,
            Screen.height / 2f
        );

        List<RaycastResult> results =
            new List<RaycastResult>();

        EventSystem.current.RaycastAll(pointer, results);

        foreach (RaycastResult result in results)
        {
            Button button =
                result.gameObject.GetComponentInParent<Button>();

            if (button != null &&
                button.interactable &&
                button.isActiveAndEnabled &&
                button.gameObject.activeInHierarchy)
            {
                currentButton = button;

                EventSystem.current.SetSelectedGameObject(
                    button.gameObject
                );

                return;
            }
        }

        currentButton = null;
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void ConfirmOption()
    {
        if (currentButton == null)
        {
            Debug.Log(
                "Calabaza: no estas mirando ninguna respuesta."
            );
            return;
        }

        Button buttonToConfirm = currentButton;

        currentButton = null;

        if (buttonToConfirm == null ||
            !buttonToConfirm.interactable ||
            !buttonToConfirm.isActiveAndEnabled ||
            !buttonToConfirm.gameObject.activeInHierarchy)
            return;

        buttonToConfirm.onClick.Invoke();
    }

    private void OnConversationEnded()
    {
        // Ignorar las conversaciones de la Gata.
        if (!pumpkinConversationRunning)
            return;

        pumpkinConversationRunning = false;
        currentButton = null;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        DialogueType finishedDialogue = currentDialogue;
        currentDialogue = DialogueType.None;

        switch (finishedDialogue)
        {
            case DialogueType.Initial:
            case DialogueType.Waiting:

                if (pumpkinCharacter != null)
                {
                    pumpkinCharacter.ResumeAfterDialogue();
                }

                RestorePlayerMovement();

                break;

            case DialogueType.Final:

                if (pumpkinCharacter != null)
                {
                    pumpkinCharacter.StartLeaving();
                }

                RestorePlayerMovement();

                break;
        }
    }

    private void RestorePlayerMovement()
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = movementWasEnabled;
        }
    }
}
