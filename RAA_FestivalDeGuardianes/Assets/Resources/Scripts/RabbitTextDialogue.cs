using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DialogueEditor;

public class RabbitTextDialogue : MonoBehaviour
{
    public string npcTag = "Rabbit";
    public NPCConversation conversation;
    public string nextScene = "";

    private MovementGaby movement;

    public CaminarTexto rabbitMovement;

    private bool followRabbit = false;

    private bool started = false;
    private bool stickReleased = true;

    private void Awake()
    {
        movement = GetComponent<MovementGaby>();

        if (movement == null)
        {
            Debug.LogError("RabbitTextDialogue: falta Movement.");
        }
    }

    private void OnEnable()
    {
        ConversationManager.OnConversationEnded += OnConversationEnded;
    }

    private void OnDisable()
    {
        ConversationManager.OnConversationEnded -= OnConversationEnded;
    }

    private void Update()
    {
        if (ConversationManager.Instance == null ||
            !ConversationManager.Instance.IsConversationActive)
            return;

        float vertical = Input.GetAxis("Vertical");

        if (Mathf.Abs(vertical) < 0.2f)
        {
            stickReleased = true;
        }
        else if (stickReleased && Mathf.Abs(vertical) > 0.5f)
        {
            stickReleased = false;

            if (vertical > 0f)
                ConversationManager.Instance.SelectPreviousOption();
            else
                ConversationManager.Instance.SelectNextOption();
        }

        if (Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.JoystickButton2))
        {
            ConversationManager.Instance.PressSelectedOption();
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        Debug.Log("He detectado un contacto con: " + other.name);
        
        if (started || !other.CompareTag(npcTag) || followRabbit)
            return;

        if (movement == null || conversation == null ||
            ConversationManager.Instance == null)
        {
            Debug.LogError("RabbitTextDialogue: revisa Movement, Conversation y ConversationManager.");
            return;
        }

        started = true;
        movement.frozen = true;

        ConversationManager.Instance.StartConversation(conversation);
    }

    private void OnConversationEnded()
    {
        if (followRabbit)
        {
            if (rabbitMovement != null)
                rabbitMovement.enabled = true;

            if (movement != null)
                movement.frozen = false;
        }
        else
        {
            if (!string.IsNullOrEmpty(nextScene))
                StartCoroutine(GoNext());
            else
                StartCoroutine(Unfreeze());
        }
    }

    private IEnumerator GoNext()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(nextScene);
    }

    private IEnumerator Unfreeze()
    {
        yield return new WaitForSeconds(0.5f);

        if (movement != null)
            movement.frozen = false;
    }

    private void OnTriggerExit(Collider other)
{
    if (other.CompareTag(npcTag))
    {
        started = false;
    }
}
public void AcceptTrial()
{
    followRabbit = true;
    Debug.Log("¡Alicia ha aceptado la prueba!");
}

}