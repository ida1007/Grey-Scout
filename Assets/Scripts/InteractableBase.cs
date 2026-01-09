using UnityEngine;

public abstract class InteractableBase : MonoBehaviour
{
    [Header("UI Prompt")]
    public string promptText = "Press E";

    public abstract void Interact(PlayerInteractor player);
}
