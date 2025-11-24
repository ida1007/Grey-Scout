using UnityEditor.Timeline.Actions;
using UnityEngine;

public class EnemyFollow: MonoBehaviour
{
    [Header("Player")]
    public Transform player;// 玩家位置
    
    [Header("Enemy")]
    public float moveSpeed;//move speed

    [Header("Detection")]
    public float detectionRange;// 检测范围
    public float detectionAttackRange;
    public float requestStayTime;

    public float detectionTimer;
    public bool isFollowing;

    [Header("UI")]
    public Canvas headCanvas;
    public UnityEngine.UI.Image barFill;
    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);
            
       if (distance <= detectionRange)// 玩家在范围内 → 开始计时
       {
          detectionTimer = Mathf.Clamp(detectionTimer + Time.deltaTime, 0f, requestStayTime);

          if (detectionTimer >= requestStayTime)// 达到指定停留时间 → 开始追踪
          {
              isFollowing = true;
          }
       }
       else
       {
            isFollowing = false;
            detectionTimer = Mathf.Clamp(detectionTimer - Time.deltaTime, 0f, requestStayTime);
       }
        if (isFollowing)
        {
            EnemyFollowing();
        }

        UpdateDetectionUI();
    }

    public void EnemyFollowing()
    {
        float distance = Vector3.Distance(transform.position, player.position);
       
            if (distance <= detectionAttackRange)
            {
                return;
            }
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            transform.LookAt(player);
       
    }

    void OnDrawGizmosSelected()//范围调试
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionAttackRange);
    }

    void UpdateDetectionUI()
    {
        if (headCanvas == null || barFill == null)
            return;//读条显示和隐藏

        if (detectionTimer > 0)
        {
            if (!headCanvas.gameObject.activeSelf)
                headCanvas.gameObject.SetActive(true);

            barFill.fillAmount = detectionTimer / requestStayTime;
        }
        else
        {
            if (headCanvas.gameObject.activeSelf)
                headCanvas.gameObject.SetActive(false);
        }
        headCanvas.transform.LookAt(Camera.main.transform);// 让读条一直朝向摄像机
        headCanvas.transform.Rotate(0, 180, 0);
    }

}
