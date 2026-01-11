using UnityEngine;
using UnityEngine.AI;


public class EnemyFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform player;
    public float moveSpeed = 6f;

    [Header("Attract Setting")]
    public bool isAttacking;
    public float attackStopRange = 1.8f;
    public float attackExitRange = 2.2f;

    [Header("Return Settings")]
    public float returnStopRange = 0.3f;

    public EnemyStayTimer stayTimer; 
    public EnemyInvestigate investigate;

    private Vector3 startPos;
    private NavMeshAgent agent;

    private void Start()
    {
        startPos = transform.position;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updatePosition = true;

        agent.speed = moveSpeed;
        agent.stoppingDistance = attackStopRange;
    }
    void Update()
    {
        if (stayTimer == null || player == null || agent == null) return;

        if (investigate != null && investigate.isInvestigating)
        {
            agent.isStopped = false;            
            agent.stoppingDistance = investigate.arriveDistance;

            FaceMoveDirection(agent.velocity);
            return;       
        }

        if (!stayTimer.isFollow) isAttacking = false;

        if (stayTimer.isFollow)
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
                FaceTo(player.position);
                return;
            }

            agent.isStopped = false;
            agent.stoppingDistance = attackStopRange;
            agent.SetDestination(player.position);

            FaceMoveDirection(agent.velocity);
            return;
        }

        // return
        if (stayTimer.isReturning)
        {
            isAttacking = false; 

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
        isAttacking = false;
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
    }

    private void FaceTo(Vector3 worldPos)
    {
        Vector3 dir = worldPos - transform.position; dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);
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

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackExitRange);
    }
}
