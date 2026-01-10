using UnityEngine;

public class EnemyStayTimer : MonoBehaviour
{
    [Header("Stay Timer Settings")]
    public float hearingIncreaseSpeed;
    public float visionIncreaseSpeed; // 警戒值上升速度
    public float decreaseSpeed; // 警戒值下降速度
    public float threshold; // 警戒值满值

    [Header("After Lost")]
    public float staySeconds = 3f;

    [Header("RunTime")]
    public float alertValue; // 当前警戒值
    public bool isFollow;
    public bool isWaiting;
    public bool isReturning;

    public EnemyHearing hearing;
    public EnemyVision vision;

    private float waitTimer;
    private bool hasChasedOnce;

    void Update()
    {
        float increase = 0f;
        bool heard = (hearing != null && hearing.isPlayerHeard);
        bool seen = (vision != null && vision.hasLineOfSight);

        if (heard)
            increase += hearingIncreaseSpeed * Time.deltaTime;

        if (seen)
            increase += visionIncreaseSpeed * Time.deltaTime;

        if (increase > 0f)
        {
            if (isWaiting) 
            { 
                isWaiting = false;
                waitTimer = 0f;
            }
            if (isReturning) isReturning = false;

            alertValue = Mathf.Clamp(alertValue + increase, 0f, threshold);

            //isFollow
            if (alertValue >= threshold)
            {
                isFollow = true;
                hasChasedOnce = true;
            }
                
        }
        else
        {
            alertValue = Mathf.Clamp(alertValue - decreaseSpeed * Time.deltaTime, 0f, threshold);

            if (alertValue < threshold)
                isFollow = false;
        }

        if (isWaiting)
        {
            isFollow = false;
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                isWaiting = false;
                isReturning = true;
            }
            return;
        }

        if (isReturning)
            return;

        if (hasChasedOnce && alertValue <= 0f && !isWaiting && !isReturning)
        {
            isFollow = false;
            isWaiting = true;
            waitTimer = staySeconds;
        }
    }
    // reset
    public void NotifyArrivedHome()
    {
        isFollow = false;
        isWaiting = false;
        isReturning = false;

        alertValue = 0f;
        waitTimer = 0f;
        hasChasedOnce = false;
    }
}

