using System.Collections.Generic;
using UnityEngine;

public class HostagePlace : MonoBehaviour
{
    public GameObject pressEUI;
    private List<GameObject> places = new List<GameObject>();
    private bool playerInside = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < transform.childCount; i++) 
        { 
            places.Add(transform.GetChild(i).gameObject);
        }

        HostageManager.Instance.InitializeBoatHostages(places);
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
