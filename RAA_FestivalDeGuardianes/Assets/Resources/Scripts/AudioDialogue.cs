using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
// Una línea de la conversación: un clip y quién lo dice
[System.Serializable]
public class AudioLine
{
    public AudioClip clip;
    public bool catSpeaks = true; // marcado = gato; desmarcado = Alicia
}
// Va en el GameObject Cat; su Capsule Collider (Is Trigger) es la zona
// de conversación
public class AudioDialogue : MonoBehaviour
{
    public AudioLine[] lines;
    public AudioSource catVoice; // Audio Source del Cat (3D)
    public AudioSource aliceVoice; // Audio Source del Player (2D)
    public GardenMovement playerMovement; // GardenMovement del Player
    public string nextScene = "MenuVR"; // vacío = no cambiar de escena
    private Animator animator;
    private bool conversationStarted = false;
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !conversationStarted)
        {
            Debug.Log("I've found the Cheshire Cat!");
            conversationStarted = true;
            StartCoroutine(Conversation());
        }
    }
    IEnumerator Conversation()
    {
        // 1) Alicia se detiene
        if (playerMovement != null) playerMovement.frozen = true;
        // 2) El gato saluda con su gesto y esperamos un momento
        if (animator != null) animator.SetTrigger("FirstGreeting");
        yield return new WaitForSeconds(1f);
        // 3) Una línea tras otra
        foreach (AudioLine line in lines)
        {
            AudioSource voice = line.catSpeaks ? catVoice : aliceVoice;
            if (voice == null || line.clip == null)
            {
                Debug.LogWarning("AudioDialogue: falta clip o voz");
                continue;
            }
            voice.clip = line.clip;
            voice.Play();
            yield return new WaitWhile(() => voice.isPlaying);
            yield return new WaitForSeconds(0.7f); // pausa entre frases
        }
        // 4) Final: cambiamos de escena o, si no hay, liberamos a Alicia
        if (nextScene != "")
        {
            yield return new WaitForSeconds(2f);
            SceneManager.LoadScene(nextScene);
        }
        else if (playerMovement != null)
        {
            playerMovement.frozen = false;
        }
    }
}
