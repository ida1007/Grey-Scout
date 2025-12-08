using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    [Header("Vision Settings")]
    public float viewDistance = 12f;
    public float fovAngle = 90f;

    [Header("Obstacle Mask")]
    public LayerMask obstacleMask;

    [Header("Eye Point")]
    public Transform eyePoint;

    [Header("Runtime Debug")]
    public bool isPlayerInFOV;
    public bool hasLineOfSight;

    public Transform player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        isPlayerInFOV = CheckFOV();
        if (isPlayerInFOV)
        {
            CheckLineOfSight();
        }
        hasLineOfSight = isPlayerInFOV && CheckLineOfSight();
    }

    private bool CheckFOV()
    {
        Vector3 dir = player.position - eyePoint.position;
        float distance = dir.magnitude;

        if (distance > viewDistance) // 超出视距
            return false;
        
        float angle = Vector3.Angle(eyePoint.forward, dir); // 是否在视野角度内
        return angle <= fovAngle * 0.5f;
    }

    private bool CheckLineOfSight()
    {
        Vector3 origin = eyePoint.position;
        Vector3 target = player.position + Vector3.up * 1.2f;
        Vector3 direction = (target - origin).normalized;
        float distance = Vector3.Distance(origin, target);
        RaycastHit hit;

        if (Physics.Raycast(origin, direction, out hit, distance))// 发射射线，获取第一个命中的对象
        {
            Transform hitObj = hit.transform;

            if (hitObj.CompareTag("Player"))
            {
                return true;
            }
            return false;
        }
        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (eyePoint == null) return;
        Gizmos.color = Color.yellow;
        Vector3 eyePos = eyePoint.position;

        Vector3 left = Quaternion.Euler(0, -fovAngle * 0.5f, 0) * eyePoint.forward;
        Vector3 right = Quaternion.Euler(0, fovAngle * 0.5f, 0) * eyePoint.forward;

        Gizmos.DrawLine(eyePos, eyePos + left * viewDistance);
        Gizmos.DrawLine(eyePos, eyePos + right * viewDistance);
        Gizmos.DrawLine(eyePos, eyePos + eyePoint.forward * viewDistance);
    }
#endif
}
