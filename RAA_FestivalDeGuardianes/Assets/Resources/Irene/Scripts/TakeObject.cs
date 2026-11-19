
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TakeObject : MonoBehaviour
{
    [Header("Botones")]
    public KeyCode mandoButton = KeyCode.JoystickButton2;
    public KeyCode key = KeyCode.E;

    [Header("Inventario")]
    public HalloweenInventory inventory;

    [Header("Configuracion")]
    [SerializeField] private int totalIngredients = 3;

    [Header("Cambio de escena")]
    [SerializeField] private string nextSceneName;

    [Header("Texto")]
    [SerializeField] private TextMeshProUGUI helpText;

    private GameObject current;

    private void Start()
    {
        ShowText("");
    }

    public void StartLook(GameObject goal)
    {
        if (goal == null || inventory == null)
        {
            StopLooking();
            return;
        }

        // Misma comprobacion que HighLightObject.
        if (!inventory.CanInteractWithObjects())
        {
            StopLooking();
            return;
        }

        // IMPORTANTE:
        // El tag está en el objeto padre, igual que en HighLightObject.
        Transform parent = goal.transform;

        bool isBook = parent.CompareTag("Book");
        bool isCauldron = parent.CompareTag("Cauldron");

        // Antes de recoger el libro, solo aparece su texto.
        if (!inventory.HasBook())
        {
            if (isBook)
            {
                Interact(goal, "Pulsa círculo\npara recoger el libro");
            }
            else
            {
                StopLooking();
            }

            return;
        }

        // Después de recoger el libro, no mostrar su texto.
        if (!inventory.CanCollectIngredients())
        {
            StopLooking();
            return;
        }

        int taken = inventory.getTakenIngredients();

        // El libro ya no necesita interacción.
        if (isBook)
        {
            StopLooking();
            return;
        }

        // Caldero: exactamente la misma condición que su luz.
        if (isCauldron)
        {
            if (inventory.CanUseCauldron() &&
                taken >= totalIngredients)
            {
                Interact(goal, "Pulsa círculo\npara dejar los ingredientes");
            }
            else
            {
                StopLooking();
            }

            return;
        }

        // Ingredientes: exactamente la misma condición que su luz.
        if (taken < totalIngredients)
        {
            Interact(goal, "Pulsa círculo\npara recoger ingrediente");
        }
        else
        {
            StopLooking();
        }
    }

    private void Interact(GameObject goal, string text)
    {
        current = goal;
        ShowText(text);
    }

    public void StopLooking()
    {
        current = null;
        ShowText("");
    }

    public void Take(GameObject goal)
    {
        if (goal == null || inventory == null)
            return;

        if (!inventory.CanInteractWithObjects())
            return;

        // El caldero no se recoge como un objeto.
        if (goal.CompareTag("Cauldron"))
            return;

        if (goal.CompareTag("Book"))
        {
            if (inventory.HasBook())
                return;

            inventory.TakeBook();

            Debug.Log("Libro recogido.");
        }
        else
        {
            if (!inventory.HasBook() ||
                !inventory.CanCollectIngredients())
                return;

            if (inventory.getTakenIngredients() >= totalIngredients)
                return;

            inventory.AddToIngredients(1);

            Debug.Log(
                "Ingredientes recogidos: " +
                inventory.getTakenIngredients()
            );
        }

        if (current == goal)
            StopLooking();

        goal.SetActive(false);
    }

    private void LeaveIngredients()
    {
        // Debe haber terminado la conversación final con la Gata.
        if (!inventory.CanUseCauldron())
            return;

        int taken = inventory.getTakenIngredients();

        // Necesitamos los tres ingredientes.
        if (taken < totalIngredients)
            return;

        // Depositar todos los ingredientes.
        inventory.LeaveIngredient(taken);
        inventory.RestartIngredients();

        Debug.Log("Ingredientes depositados: " + taken);

        StopLooking();

        // Cambiar inmediatamente a la siguiente escena.
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError(
                "No has asignado la siguiente escena en TakeObject."
            );
        }
    }

    private void Update()
    {
        if (current == null || inventory == null)
            return;

        if (!inventory.CanInteractWithObjects())
        {
            StopLooking();
            return;
        }

        if (Input.GetKeyDown(mandoButton) ||
            Input.GetKeyDown(key))
        {
            if (current.CompareTag("Cauldron"))
            {
                // Comprobar las mismas condiciones que al mostrar el texto.
                if (inventory.CanUseCauldron() &&
                    inventory.getTakenIngredients() >= totalIngredients)
                {
                    LeaveIngredients();
                }
            }
            else
            {
                Take(current);
            }
        }
    }

    private void ShowText(string text)
    {
        if (helpText != null)
            helpText.text = text;
    }
}
