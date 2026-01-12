using UnityEngine;

public class InteractZone : MonoBehaviour
{
    public InteractableBase interactable;   
    public GameObject promptUI;            

    void Reset()
    {
        if (interactable == null)
            interactable = GetComponentInParent<InteractableBase>();
    }

    void Awake()
    {
        if (promptUI != null) promptUI.SetActive(false);
        if (interactable == null)
            interactable = GetComponentInParent<InteractableBase>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var pi = other.GetComponentInParent<PlayerInteractor>();
        if (pi == null) return;

        pi.current = interactable;
        if (promptUI != null) promptUI.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var pi = other.GetComponentInParent<PlayerInteractor>();
        if (pi == null) return;

        if (pi.current == interactable)
            pi.current = null;

        if (promptUI != null) promptUI.SetActive(false);
    }
}
