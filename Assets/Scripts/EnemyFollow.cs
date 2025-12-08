using UnityEngine;


public class EnemyFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform player;
    public float moveSpeed;
    public float attackStopRange;

    public EnemyStayTimer stayTimer; // 引用警戒值系统

    void Update()
    {
        if (!stayTimer.isFollow)
            return;

        float distance = (player.position - transform.position).magnitude;

        if (distance <= attackStopRange)
            return;

        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
        transform.LookAt(player);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackStopRange);
    }
}
