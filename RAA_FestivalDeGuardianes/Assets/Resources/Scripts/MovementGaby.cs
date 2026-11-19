using UnityEngine;
using UnityEngine.SceneManagement;
[RequireComponent(typeof(CharacterController))]
public class MovementGaby : MonoBehaviour
{
    public float speed = 2.5f; // velocidad de paseo (m/s)
    public float acceleration = 12f; // arranque y parada suaves
    public float minStepTime = 0.3f; // un toque breve dura al menos este tiempo
    public float gravity = -9.81f;
   
    public KeyCode forwardButton = KeyCode.JoystickButton0; // botón A del mando
   
    public bool useVisorButton = true; // el botón del visor también hace avanzar
    public string goalTag = "RabbitHole";
    public string nextScene = ""; // vacío: de momento no cambiamos de escena
   
    private CharacterController controller;
   
    private Vector3 horizontalVelocity;
    private Vector3 stepDirection;
   
    private float stepTimer;
    private float verticalVelocity;
    private bool levelEnded;

    public int totalEggs = 15;
    public string eggTag = "Egg";
    private int eggsCollected;

    public bool frozen = false;
    public TMPro.TMP_Text eggsText;

    void Start(){

        controller = GetComponent<CharacterController>();

    }

     void Update() {

        if (levelEnded) return;

        // 1) entrada: stick izquierdo / flechas / WASD (ejes Horizontal y Vertical)
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        
        if (Input.GetKey(forwardButton) || (useVisorButton && Input.GetMouseButton(0))){
            input.z = 1f;
        }
        
        input = Vector3.ClampMagnitude(input, 1f);
        
        if (frozen) { 

            input = Vector3.zero; stepTimer = 0f; 
            
        }
        // 2) dirección: relativa a hacia dónde mira la cabeza, sin componente vertical
        if (input.sqrMagnitude > 0.01f && Camera.main != null){

            Vector3 forward = Camera.main.transform.forward;

            forward.y = 0f;
            forward.Normalize();

            Vector3 right = Camera.main.transform.right;

            right.y = 0f;
            right.Normalize();
            stepDirection = (forward * input.z + right * input.x).normalized;
            stepTimer = minStepTime; // se renueva mientras se mantiene la entrada
    
        } else {
            
            stepTimer -= Time.deltaTime;
        
        }
        
        // 3) velocidad horizontal con aceleración suave
        
        Vector3 target = (stepTimer > 0f) ? stepDirection * speed : Vector3.zero;
        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, target, acceleration * Time.deltaTime);
        
        // 4) gravedad: el Character Controller no la incluye
        if (controller.isGrounded && verticalVelocity < 0f){

            verticalVelocity = -2f;
        }
        
        verticalVelocity += gravity * Time.deltaTime;
        
        // 5) una única llamada a Move por fotograma
        controller.Move((horizontalVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);

    }
 
    private void OnTriggerEnter(Collider other) {

        if (levelEnded) return;

        // Recoger huevo
        if (other.CompareTag(eggTag)) {

            Collider eggCollider = other.GetComponent<Collider>();
            if (eggCollider != null)
                eggCollider.enabled = false;

            eggsCollected++;

            int remaining = GetEggsRemaining();

            if (eggsText != null)
            {
                eggsText.text = "Te quedan " + remaining + " huevos por encontrar.";
                eggsText.gameObject.SetActive(true);
            }

            Destroy(other.gameObject);

            Debug.Log("Huevos: " + eggsCollected + "/" + totalEggs);
            return;
        }

        // Llegar a la madriguera
        if (!other.CompareTag(goalTag)) return;

        if (eggsCollected < totalEggs) {
            Debug.Log("Aún faltan huevos: " + eggsCollected + "/" + totalEggs);
            return;
        }

        levelEnded = true;

        if (string.IsNullOrEmpty(nextScene)) {
            Debug.Log("I'm late! Down the rabbit hole... (nextScene está vacío)");
        } else {
            SceneManager.LoadScene(nextScene);
        }

    }

    public int GetEggsRemaining()
    {
        return totalEggs - eggsCollected;
    }

    public void UpdateEggsText(int remaining)
    {
        if (eggsText != null)
        {
            eggsText.text = "Te quedan " + remaining + " huevos por encontrar.";
            eggsText.gameObject.SetActive(true);
        }
    }
}