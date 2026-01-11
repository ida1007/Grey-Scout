using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HostagePlace : MonoBehaviour
{
    [Header("UI")]
    public GameObject pressEUI;
    public TMP_Text countText;

    [Header("Win Sequence")]
    public WinSequenceController winSequenceController;
    public MonoBehaviour playerController;

    private List<GameObject> places = new List<GameObject>();
    private bool playerInside = false;
    private bool hasWon = false;

    void Start()
    {
        
        for (int i = 0; i < transform.childCount; i++)
        {
            places.Add(transform.GetChild(i).gameObject);
        }

        HostageManager.Instance.InitializeBoatHostages(places);

        if (pressEUI) pressEUI.SetActive(false);

        UpdateCountUI();
    }

    void Update()
    {
        if (!playerInside) return;
        if (hasWon) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            // win
            if (HostageManager.Instance.IsBoatFull)
            {
                Win();
            }
            else
            {
                HostageManager.Instance.MoveHostage();
                UpdateCountUI();
                // here can change the UI panel z.B. Lets go! usw
            }
        }
    }
    void UpdateCountUI()
    {
        if (countText == null) return;

        int current = HostageManager.Instance.CurrentBoatNum;
        int total = HostageManager.Instance.BoatCapacity;

        countText.text = $"On Boat: {current} / {total}";
    }
    private void Win()
    {
        hasWon = true;
        Debug.Log("YOU WIN! Boat departed.");

        winSequenceController.Play();
        playerController.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            if (pressEUI) pressEUI.SetActive(true);
            UpdateCountUI();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            if (pressEUI) pressEUI.SetActive(false);
        }
    }
}
