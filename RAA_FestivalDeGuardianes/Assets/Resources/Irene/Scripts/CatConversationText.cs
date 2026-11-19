
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DialogueEditor;
using System.Collections.Generic;

public class CatConversationText : MonoBehaviour
{
    [Header("Botones para confirmar")]
    public KeyCode mandoButton = KeyCode.JoystickButton2;
    public KeyCode key = KeyCode.E;

    [Header("Inventario")]
    public HalloweenInventory inventory;

    [Header("Orientacion del jugador")]
    public Transform playerBody;
    public Transform catLookTarget;

    [Header("Bloqueo del jugador")]
    public Behaviour playerMovement;

    [Header("Camara")]
    public Camera playerCamera;

    [Header("Conversaciones")]
    public NPCConversation catInitial;
    public NPCConversation catNoBook;
    public NPCConversation catBook;
    public NPCConversation catNoIngredients;
    public NPCConversation catFinal;

    [Header("Conversacion especial")]
    public NPCConversation catZombie;

    private Button currentButton;

    private enum DialogueType
    {
        None,
        Initial,
        NoBook,
        Book,
        NoIngredients,
        Final,
        Zombie
    }

    private DialogueType currentDialogueType = DialogueType.None;

    private bool playerInRange;
    private bool initialDialogueFinished;
    private bool hasReturnedWithBook;
    private bool finalDialogueFinished;

    private bool catConversationRunning;
    private bool movementWasEnabled;

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

        if (ConversationManager.Instance == null)
            return;

        if (ConversationManager.Instance.IsConversationActive)
            return;

        if (catConversationRunning)
            return;

        if (finalDialogueFinished)
            return;

        // Conversacion especial por el zombi.
        if (inventory.WasCaughtByZombie())
        {
            StartCatConversation(
                catZombie,
                DialogueType.Zombie
            );
            return;
        }

        // Conversacion inicial.
        if (!initialDialogueFinished)
        {
            StartCatConversation(
                catInitial,
                DialogueType.Initial
            );
            return;
        }

        // Jugador sin libro.
        if (!inventory.HasBook())
        {
            StartCatConversation(
                catNoBook,
                DialogueType.NoBook
            );
            return;
        }

        int ingredientsMissing =
            inventory.GetIngredientsIntCauldron();

        // Todos los ingredientes recogidos.
        if (inventory.getTakenIngredients() >= 3)
        {
            StartCatConversation(
                catFinal,
                DialogueType.Final
            );
            return;
        }

        // Jugador vuelve con el libro.
        if (!hasReturnedWithBook)
        {
            StartCatConversation(
                catBook,
                DialogueType.Book
            );
            return;
        }

        // Faltan ingredientes.
        StartCatConversation(
            catNoIngredients,
            DialogueType.NoIngredients
        );
    }

    private void StartCatConversation(
        NPCConversation conversation,
        DialogueType type)
    {
        if (conversation == null ||
            ConversationManager.Instance == null)
        {
            return;
        }

        if (ConversationManager.Instance.IsConversationActive)
            return;

        currentDialogueType = type;
        catConversationRunning = true;

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
        // Este script solo controla las conversaciones de la Gata.
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
            Debug.Log("Gata: no estas mirando ninguna respuesta.");
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
        // Ignorar las conversaciones de otros personajes.
        if (!catConversationRunning)
            return;

        catConversationRunning = false;
        currentButton = null;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        RestorePlayerMovement();

        DialogueType finishedDialogue = currentDialogueType;
        currentDialogueType = DialogueType.None;

        switch (finishedDialogue)
        {
            case DialogueType.Initial:

                initialDialogueFinished = true;

                inventory.UnlockObjectInteraction();

                break;

            case DialogueType.Book:

                hasReturnedWithBook = true;

                // Ahora se permite recoger ingredientes.
                inventory.UnlockIngredientInteraction();

                break;

            case DialogueType.Final:

                finalDialogueFinished = true;
                inventory.UnlockCauldronInteraction();

                break;

            case DialogueType.Zombie:

                // Ya se ha hablado con la Gata sobre el zombi.
                inventory.ResetCaughtByZombie();

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
