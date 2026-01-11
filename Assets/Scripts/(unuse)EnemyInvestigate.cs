using UnityEngine;
using UnityEngine.AI;

public class EnemyInvestigate : MonoBehaviour
{
    [Header("Refs")]
    public NavMeshAgent agent;
    public EnemyStayTimer stayTimer;

    [Header("Investigate Settings")]
    public float arriveDistance = 1.2f;     
    public float stopSeconds = 1.0f;        
    public float repathInterval = 0.25f;    

    [Header("Runtime")]
    public bool isInvestigating;
    public Vector3 investigatePos;

    private float stopTimer;
    private float repathTimer;

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (!isInvestigating) return;
        if (agent == null) return;

        // 不抢主状态机控制（追击/等待/回家时，停止调查）
        if (stayTimer != null)
        {
            if (stayTimer.isFollow || stayTimer.isWaiting || stayTimer.isReturning)
            {
                StopInvestigate(resetPath: true);
                return;
            }
        }

        // 到达后停留
        if (stopTimer > 0f)
        {
            stopTimer -= Time.deltaTime;
            if (stopTimer <= 0f)
            {
                StopInvestigate(resetPath: true);
            }
            return;
        }

        // 避免频繁 SetDestination
        if (repathTimer > 0f) repathTimer -= Time.deltaTime;

        // 到达判断：agent.remainingDistance 只有在 path 计算完后可靠
        if (!agent.pathPending)
        {
            if (agent.hasPath && agent.remainingDistance <= arriveDistance)
            {
                // 到达：停留观察
                agent.ResetPath();
                stopTimer = stopSeconds;
                return;
            }
        }

        if ((!agent.hasPath || agent.pathStatus != NavMeshPathStatus.PathComplete) && repathTimer <= 0f)
        {
            repathTimer = repathInterval;
            agent.isStopped = false;
            agent.SetDestination(investigatePos);
        }
    }

    /// <summary>
    /// 被 Duck 触发：去调查声音位置
    /// </summary>
    public void GoInvestigate(Vector3 pos)
    {
        if (agent == null) return;

        if (stayTimer != null && (stayTimer.isFollow || stayTimer.isWaiting || stayTimer.isReturning))
            return;

        investigatePos = pos;
        isInvestigating = true;
        stopTimer = 0f;
        repathTimer = 0f;

        agent.ResetPath();                     
        agent.isStopped = false;
        agent.stoppingDistance = arriveDistance; 
        agent.SetDestination(investigatePos);
    }


    public void StopInvestigate(bool resetPath)
    {
        isInvestigating = false;
        stopTimer = 0f;
        repathTimer = 0f;

        if (agent != null && resetPath)
            agent.ResetPath();
    }
}
