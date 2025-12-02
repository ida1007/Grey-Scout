using UnityEngine;

public class EnemyHearing : MonoBehaviour
{
    [Header("Hearing Settings")]
    public Transform player;
    public float hearingRange;

    public bool isPlayerHeard;

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        isPlayerHeard = distance <= hearingRange;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
    }
}
