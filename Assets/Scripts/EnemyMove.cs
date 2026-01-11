using UnityEngine;
using UnityEngine.AI;

public class EnemyMove : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;
    public EnemyStayTimer stayTimer;
    public NavMeshAgent agent;

    [Header("Move Speed")]
    public float moveSpeed = 6f;

    [Header("Attack Setting")]
    public bool isAttacking;
    public float attackStopRange = 1.8f;
    public float attackExitRange = 2.2f;

    [Header("Return Settings")]
    public float returnStopRange = 0.3f;

    [Header("Investigate Settings")]
    public float investigateArriveDistance = 1.2f;
    public float investigateStopSeconds = 1.0f;
    public float repathInterval = 0.25f;

    [Header("Runtime (Read Only)")]
    public bool isInvestigating;
    public Vector3 investigatePos;

    private Vector3 startPos;
    private float investigateStopTimer;
    private float repathTimer;

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        startPos = transform.position;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updatePosition = true;
            agent.speed = moveSpeed;
            agent.stoppingDistance = attackStopRange;
        }
    }

    void Update()
    {
        if (stayTimer == null || agent == null) return;

        // 非追击时：攻击态清掉
        if (!stayTimer.isFollow) isAttacking = false;

        // 1) Follow（追击）最高优先级
        if (stayTimer.isFollow && player != null)
        {
            UpdateFollow();
            return;
        }

        // 2) Return（回家）第二优先级
        if (stayTimer.isReturning)
        {
            UpdateReturn();
            return;
        }

        // 3) Investigate（调查）第三优先级（注意：waiting 的时候 stayTimer 会 return，所以这里不要用 isWaiting）
        if (isInvestigating)
        {
            UpdateInvestigate();
            return;
        }

        // 4) Waiting / Idle
        UpdateWait();
    }

    // ===================== Follow =====================
    void UpdateFollow()
    {
        float dist = FlatDistance(transform.position, player.position);

        if (!isAttacking)
        {
            if (dist <= attackExitRange) isAttacking = true;
        }
        else
        {
            if (dist > attackExitRange) isAttacking = false;
        }

        // enter attack range
        if (dist <= attackStopRange)
        {
            agent.isStopped = true;
            agent.ResetPath();
            FaceTo(player.position);
            return;
        }

        agent.isStopped = false;
        agent.stoppingDistance = attackStopRange;
        agent.SetDestination(player.position);

        FaceMoveDirection(agent.velocity);
    }

    // ===================== Return =====================
    void UpdateReturn()
    {
        // 回家时：停止调查
        isInvestigating = false;
        investigateStopTimer = 0f;
        repathTimer = 0f;

        isAttacking = false;

        agent.isStopped = false;
        agent.stoppingDistance = returnStopRange;
        agent.SetDestination(startPos);

        if (FlatDistance(transform.position, startPos) <= returnStopRange)
        {
            agent.isStopped = true;
            agent.ResetPath();
            stayTimer.NotifyArrivedHome();
            return;
        }

        FaceMoveDirection(agent.velocity);
    }

    // ===================== Investigate =====================
    void UpdateInvestigate()
    {
        // 如果 stayTimer 在 waiting 状态（它内部会 return），你希望 Duck 能打断 waiting 的话：
        // 你已经在 ApplyDuckAlertJump 里把 isWaiting/isReturning 清掉了，所以这里不用额外处理。

        // 到达后停留
        if (investigateStopTimer > 0f)
        {
            investigateStopTimer -= Time.deltaTime;
            agent.isStopped = true;
            FaceTo(investigatePos);

            if (investigateStopTimer <= 0f)
            {
                StopInvestigate(resetPath: true);
            }
            return;
        }

        // 正常走向目标
        agent.isStopped = false;
        agent.stoppingDistance = investigateArriveDistance;

        // 避免频繁 SetDestination
        if (repathTimer > 0f) repathTimer -= Time.deltaTime;

        if ((!agent.hasPath || agent.pathStatus != NavMeshPathStatus.PathComplete) && repathTimer <= 0f)
        {
            repathTimer = repathInterval;
            agent.SetDestination(investigatePos);
        }

        // 到达判定（pathPending 结束后再看 remainingDistance）
        if (!agent.pathPending && agent.hasPath && agent.remainingDistance <= investigateArriveDistance)
        {
            agent.ResetPath();
            agent.isStopped = true;
            investigateStopTimer = investigateStopSeconds;
        }

        FaceMoveDirection(agent.velocity);
    }

    void UpdateWait()
    {
        isAttacking = false;

        agent.isStopped = true;
        agent.ResetPath();
        FaceMoveDirection(agent.velocity);
    }

    // ===================== Public API =====================
    public void GoInvestigate(Vector3 pos)
    {
        if (agent == null || stayTimer == null) return;

        // 追击中不接管（你也可以改成允许追击时更新 investigatePos，但不会生效因为 follow 优先级更高）
        if (stayTimer.isFollow) return;

        // Duck 会在 ApplyDuckAlertJump 里清掉 waiting/returning
        // 这里做得更稳：再清一次回家状态下的路径
        if (stayTimer.isReturning)
        {
            // 允许 Duck 打断回家：你 StayTimer 里已经 isReturning=false 了
            agent.ResetPath();
        }

        investigatePos = pos;
        isInvestigating = true;
        investigateStopTimer = 0f;
        repathTimer = 0f;

        agent.isStopped = false;
        agent.ResetPath();
        agent.stoppingDistance = investigateArriveDistance;
        agent.SetDestination(investigatePos);
    }

    public void StopInvestigate(bool resetPath)
    {
        isInvestigating = false;
        investigateStopTimer = 0f;
        repathTimer = 0f;

        if (agent != null && resetPath)
            agent.ResetPath();
    }

    // ===================== Helpers =====================
    private void FaceMoveDirection(Vector3 velocity)
    {
        Vector3 dir = velocity;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    private void FaceTo(Vector3 worldPos)
    {
        Vector3 dir = worldPos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    private float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f; b.y = 0f;
        return Vector3.Distance(a, b);
    }
}
