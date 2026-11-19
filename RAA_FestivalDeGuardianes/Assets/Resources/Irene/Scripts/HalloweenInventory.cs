
using UnityEngine;

public class HalloweenInventory : MonoBehaviour
{
    private bool hasBook;
    private int ingredientsTaken;
    private int ingredientsInCauldron = 3;

    // Se desbloquea al terminar la primera conversación con la Gata.
    private bool canInteractWithObjects = false;

    private bool canCollectIngredients = false;

    public void UnlockIngredientInteraction()
    {
        canCollectIngredients = true;
        Debug.Log("Ya puedes recoger ingredientes.");
    }

    public bool CanCollectIngredients()
    {
        return canCollectIngredients;
    }

    public void UnlockObjectInteraction()
    {
        canInteractWithObjects = true;
        Debug.Log("Ya puedes interactuar con los objetos.");
    }

    public bool CanInteractWithObjects()
    {
        return canInteractWithObjects;
    }

    public void RestartBook()
    {
        hasBook = false;
    }

    public void RestartIngredients()
    {
        ingredientsTaken = 0;
    }

    public void RestartIngredientsInCauldron()
    {
        ingredientsInCauldron = 3;
    }

    public void LeaveIngredient(int ingredients)
    {
        ingredientsInCauldron -= ingredients;
        ingredientsInCauldron = Mathf.Max(0, ingredientsInCauldron);
    }

    public int GetIngredientsIntCauldron()
    {
        return ingredientsInCauldron;
    }

    public void AddToIngredients(int taken)
    {
        ingredientsTaken += taken;
    }

    public bool HasBook()
    {
        return hasBook;
    }

    public int getTakenIngredients()
    {
        return ingredientsTaken;
    }

    public void TakeBook()
    {
        hasBook = true;
    }

    private bool caughtByZombie = false;

    public void SetCaughtByZombie()
    {
        caughtByZombie = true;
    }

    public bool WasCaughtByZombie()
    {
        return caughtByZombie;
    }

    public void ResetCaughtByZombie()
    {
        caughtByZombie = false;
    }

    private bool canUseCauldron = false;

    public void UnlockCauldronInteraction()
    {
        canUseCauldron = true;
    }

    public bool CanUseCauldron()
    {
        return canUseCauldron;
    }
}
