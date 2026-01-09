using UnityEngine;

public class InteractZone : MonoBehaviour
{
    public InteractableBase interactable;   // 拖要交互的物体脚本
    public GameObject promptUI;             // “E”提示的UI物体（世界空间UI或小图标）

    void Reset()
    {
        // 自动找：常见做法是 Trigger 是子物体，Interactable 在父物体
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
