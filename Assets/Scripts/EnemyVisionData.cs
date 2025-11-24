using UnityEngine;

[CreateAssetMenu(menuName = "AI/Enemy Vision Data", fileName = "EnemyVisionData")]
public class EnemyVisionData : MonoBehaviour
{
    [Header("基础视觉参数")]
    public float viewDistance = 12f;
    public float fovAngle = 90f;

    [Header("发现度")]
    public float detectionIncreaseSpeed = 1.0f;
    public float detectionDecreaseSpeed = 0.5f;
    public float detectionThreshold = 1.0f;

    [Header("失去目标")]
    public float loseSightTime = 1.5f;

    [Header("遮挡检测")]
    public LayerMask obstacleMask;
    public LayerMask targetMask;
}
