using UnityEngine;

public class EnemyStayTimer : MonoBehaviour
{
    [Header("Stay Timer Settings")]
    public float hearingIncreaseSpeed;
    public float visionIncreaseSpeed;// 警戒值上升速度
    public float decreaseSpeed;// 警戒值下降速度
    public float threshold;// 警戒值满值

    public float alertValue;// 当前警戒值
    public bool isFollow; 

    public EnemyHearing hearing;
    public EnemyVision vision;

    void Update()
    {
        float increase = 0f;

        if (hearing != null && hearing.isPlayerHeard)
            increase += hearingIncreaseSpeed * Time.deltaTime;

        if (vision != null && vision.hasLineOfSight)
            increase += visionIncreaseSpeed * Time.deltaTime;

        if (increase > 0f)
        {
            alertValue = Mathf.Clamp(alertValue + increase, 0f, threshold);
        }
        else
        {
            
            alertValue = Mathf.Clamp(alertValue - decreaseSpeed * Time.deltaTime, 0f, threshold);
        }

        isFollow = alertValue >= threshold;
    }
}
