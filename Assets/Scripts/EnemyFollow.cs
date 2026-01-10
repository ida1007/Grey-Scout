using UnityEngine;
using UnityEngine.AI;


public class EnemyFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform player;
    public float moveSpeed;
    public float attackStopRange = 1.8f;

    [Header("Return Settings")]
    public float returnStopRange = 0.3f;

    public EnemyStayTimer stayTimer; // 引用警戒值系统

    private Vector3 startPos;
    private NavMeshAgent agent;

    private void Start()
    {
        startPos = transform.position;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        agent = GetComponent<NavMeshAgent>();

        // 用你自己的 LookRotation（只转Y），所以关掉Agent自动旋转
        agent.updateRotation = false;

        // 让 Agent 自己更新位置（默认true）
        agent.updatePosition = true;

        // 让停止距离等于攻击停下距离（也可以分开）
        agent.stoppingDistance = attackStopRange;
    }
    void Update()
    {
        if (stayTimer == null || player == null || agent == null) return;

        if (stayTimer.isFollow)
        {
            //return;
            agent.isStopped = false;
            agent.stoppingDistance = attackStopRange;
            agent.SetDestination(player.position);

            FaceMoveDirection(agent.velocity);
            return;
        }

        // return
        if (stayTimer.isReturning)
        {
            agent.isStopped = false;
            agent.stoppingDistance = returnStopRange;
            agent.SetDestination(startPos);

            // 到家：判定更稳一点（用平面距离）
            if (FlatDistance(transform.position, startPos) <= returnStopRange)
            {
                agent.isStopped = true;
                stayTimer.NotifyArrivedHome();
                return;
            }

            FaceMoveDirection(agent.velocity);
            return;
        }

        // waiting
        agent.isStopped = true;
        FaceMoveDirection(agent.velocity);

    }
    private void FaceMoveDirection(Vector3 velocity)
    {
        Vector3 dir = velocity;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }
        else
        {
            // 如果 velocity 很小（刚停下），也可以选择朝向玩家/朝向家
            // 不需要就留空
        }
    }

    private float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f; b.y = 0f;
        return Vector3.Distance(a, b);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackStopRange);
    }
}
