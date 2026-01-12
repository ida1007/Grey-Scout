using UnityEngine;

public class FGuidance : MonoBehaviour
{
    [Header("UI")]
    public GameObject promptUI;

    [Header("Input")]
    public KeyCode continueKey = KeyCode.F;

    [Header("Behavior")]
    public bool triggerOnce = true;
    public bool pauseOnEnter = true;

    public string playerTag = "Player";

    private bool playerInside = false;
    private bool finished = false;

    void Awake()
    {
        if (promptUI) promptUI.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (finished) return;
        if (!other.CompareTag(playerTag)) return;

        playerInside = true;

        if (promptUI) promptUI.SetActive(true);

        if (pauseOnEnter)
            Time.timeScale = 0f; 
    }

    void OnTriggerExit(Collider other)
    {
        if (finished) return;
        if (!other.CompareTag(playerTag)) return;

        playerInside = false;

    }

    void Update()
    {
        if (finished) return;
        if (!playerInside) return;

        if (Input.GetKeyDown(continueKey))
        {
            FinishGuidance();
        }
    }

    public void FinishGuidance()
    {
        if (finished) return;
        finished = true;

        if (promptUI) promptUI.SetActive(false);

        Time.timeScale = 1f; 

        if (triggerOnce)
        {
            enabled = false;
        }
        else
        {
            playerInside = false;
        }
    }

    void OnDisable()
    {
        if (Time.timeScale == 0f && finished)
            Time.timeScale = 1f;
    }
}
