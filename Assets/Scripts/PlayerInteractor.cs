using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.E;

    // promise first interact
    public InteractableBase current;

    void Update()
    {
        if (Input.GetKeyDown(interactKey) && current != null)
        {
            current.Interact(this);
        }
    }
}
