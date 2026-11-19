
using UnityEngine;

public class HighLightObject : MonoBehaviour
{
    [SerializeField] private float turnOnIntensity = 2f;
    [SerializeField] private int totalIngredients = 3;

    public HalloweenInventory inventory;

    public void TurnOn(Light light)
    {
        if (light == null || inventory == null)
            return;

        if (!inventory.CanInteractWithObjects())
        {
            ChangeIntensity(light, 0f);
            return;
        }

        Transform parent = light.transform.parent;

        if (parent == null)
            return;

        bool isBook = parent.CompareTag("Book");
        bool isCauldron = parent.CompareTag("Cauldron");

        // Antes de recoger el libro, solo se ilumina el libro.
        if (!inventory.HasBook())
        {
            ChangeIntensity(
                light,
                isBook ? turnOnIntensity : 0f
            );

            return;
        }

        // El libro ya esta recogido, pero falta hablar con la Gata.
        if (!inventory.CanCollectIngredients())
        {
            ChangeIntensity(light, 0f);
            return;
        }

        int taken = inventory.getTakenIngredients();

        // El libro ya no necesita iluminarse.
        if (isBook)
        {
            ChangeIntensity(light, 0f);
            return;
        }

        // El caldero solo se ilumina después de hablar con la Gata
        // al terminar la conversación final.
        if (isCauldron)
        {
            ChangeIntensity(
                light,
                inventory.CanUseCauldron() &&
                taken >= totalIngredients
                    ? turnOnIntensity
                    : 0f
            );

            return;
        }

        // Los ingredientes solo se iluminan si quedan por recoger.
        ChangeIntensity(
            light, taken < totalIngredients
                ? turnOnIntensity
                : 0f
        );
    }

    private void ChangeIntensity(Light light, float newIntensity)
    {
        if (light != null)
            light.intensity = newIntensity;
    }

    public void TurnOff(Light light)
    {
        if (light != null)
            ChangeIntensity(light, 0f);
    }
}
