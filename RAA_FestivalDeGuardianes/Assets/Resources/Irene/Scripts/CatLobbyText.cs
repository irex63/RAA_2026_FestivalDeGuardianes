
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DialogueEditor;
using System.Collections.Generic;

public class CatLobbyText : MonoBehaviour
{
    [Header("Botones para confirmar")]
    public KeyCode mandoButton = KeyCode.JoystickButton2;
    public KeyCode key = KeyCode.E;

    [Header("Conversaciones")]
    public NPCConversation catNotPlayed;
    public NPCConversation catPlayed;

    [Header("Orientacion del jugador")]
    public Transform playerBody;
    public Transform catLookTarget;

    [Header("Bloqueo del jugador")]
    public Behaviour playerMovement;

    [Header("Camara")]
    public Camera playerCamera;

    private Button currentButton;

    private bool playerInRange;
    private bool catConversationRunning;
    private bool movementWasEnabled;
    private bool movementWasCaptured;

    private void OnEnable()
    {
        ConversationManager.OnConversationEnded += OnConversationEnded;
    }

    private void OnDisable()
    {
        ConversationManager.OnConversationEnded -= OnConversationEnded;

        catConversationRunning = false;
        currentButton = null;

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

    private void RotatePlayerTowardsCat()
    {
        if (playerBody == null || playerCamera == null)
        {
            Debug.LogWarning(
                "CatLobbyText: falta asignar Player Body o Player Camera."
            );
            return;
        }

        if (!playerCamera.transform.IsChildOf(playerBody))
        {
            Debug.LogWarning(
                "CatLobbyText: Player Camera debe estar dentro de Player Body."
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
        if (ConversationManager.Instance == null)
            return;

        if (ConversationManager.Instance.IsConversationActive)
            return;

        if (catConversationRunning)
            return;

        int levelPlayed = PlayerPrefs.GetInt(
            PlayerPrefsNames.hallowen,
            PlayerPrefsNames.noPlayed
        );

        NPCConversation conversation;

        if (levelPlayed == PlayerPrefsNames.played)
        {
            conversation = catPlayed;
        }
        else
        {
            conversation = catNotPlayed;
        }

        StartCatConversation(conversation);
    }

    private void StartCatConversation(NPCConversation conversation)
    {
        if (conversation == null)
        {
            Debug.LogWarning(
                "CatLobbyText: falta asignar la conversación correspondiente."
            );
            return;
        }

        if (ConversationManager.Instance == null)
            return;

        if (ConversationManager.Instance.IsConversationActive)
            return;

        catConversationRunning = true;
        currentButton = null;

        if (playerMovement != null)
        {
            movementWasEnabled = playerMovement.enabled;
            movementWasCaptured = true;
            playerMovement.enabled = false;
        }

        ConversationManager.Instance.StartConversation(conversation);
    }

    private void Update()
    {
        if (!catConversationRunning)
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
        if (playerCamera == null || EventSystem.current == null)
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
            return;

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
        if (!catConversationRunning)
            return;

        catConversationRunning = false;
        currentButton = null;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        RestorePlayerMovement();

        Debug.Log("CatLobbyText: conversación terminada.");
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
