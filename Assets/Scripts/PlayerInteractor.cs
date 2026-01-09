using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.E;

    // 当前在范围内、优先级最高的交互对象
    public InteractableBase current;

    void Update()
    {
        if (Input.GetKeyDown(interactKey) && current != null)
        {
            current.Interact(this);
        }
    }
}
