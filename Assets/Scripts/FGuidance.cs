using UnityEngine;

public class FGuidance : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("进入区域后显示的提示UI（比如世界空间UI、Canvas上的一个Panel等）")]
    public GameObject promptUI;

    [Header("Input")]
    [Tooltip("按下这个键后结束引导并继续游戏")]
    public KeyCode continueKey = KeyCode.F;

    [Header("Behavior")]
    [Tooltip("是否只触发一次")]
    public bool triggerOnce = true;

    [Tooltip("进入区域后是否暂停游戏")]
    public bool pauseOnEnter = true;

    [Tooltip("玩家Tag（默认Player）")]
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
