using UnityEngine;

public class HostagePlace : MonoBehaviour
{
    public GameObject pressEUI;
    private bool playerInside = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerInside) return;
        if (Input.GetKeyDown(KeyCode.E))
        {

            HostageManager.Instance.MoveHostage();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            pressEUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            pressEUI.SetActive(false);
        }
    }
}
