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

        if (!stayTimer.isFollow) isAttacking = false;

        // Follow first range
        if (stayTimer.isFollow && player != null)
        {
            UpdateFollow();
            return;
        }

        // Return second range
        if (stayTimer.isReturning)
        {
            UpdateReturn();
            return;
        }

        // Investigate third range
        if (isInvestigating)
        {
            UpdateInvestigate();
            return;
        }

        // Waiting / Idle
        UpdateWait();
    }

    // Follow 
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

    // Return
    void UpdateReturn()
    {
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

    //Investigate
    void UpdateInvestigate()
    {
        // stop after arriving
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

        // walk to player
        agent.isStopped = false;
        agent.stoppingDistance = investigateArriveDistance;

        // avoid SetDestination
        if (repathTimer > 0f) repathTimer -= Time.deltaTime;

        if ((!agent.hasPath || agent.pathStatus != NavMeshPathStatus.PathComplete) && repathTimer <= 0f)
        {
            repathTimer = repathInterval;
            agent.SetDestination(investigatePos);
        }

        // arrive check
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

    // Public thinhs
    public void GoInvestigate(Vector3 pos)
    {
        if (agent == null || stayTimer == null) return;

        if (stayTimer.isFollow) return;

        if (stayTimer.isReturning)
        {
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

    // Face to 
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
