using UnityEngine;

public class HostageFollow : MonoBehaviour
{
    public Transform player;
    [Header("Hostage")]
    public bool isHostageFollowing; 
    public float followDistance;
    public float smoothSpeed;

    private Vector3 lastPlayerPos;

    void Start()
    {
        lastPlayerPos = player.position;
    }

    void Update()
    {
        if (isHostageFollowing)
        {
            Vector3 playerMoveDir = player.position - lastPlayerPos;//计算玩家移动方向
            playerMoveDir.y = 0;

            if (playerMoveDir.magnitude > 0.01f) //检测玩家是否移动
            {
                playerMoveDir.Normalize();
            }

            Vector3 targetPos = player.position - playerMoveDir * followDistance;
            targetPos.y = transform.position.y;
            float currentDist = Vector3.Distance(transform.position, player.position);

            if (currentDist > followDistance) //控制最小距离 平滑移动
            {
                transform.position = Vector3.Lerp(
                    transform.position,
                    targetPos,
                    smoothSpeed * Time.deltaTime
                );
            }
            Vector3 lookDir = player.position - transform.position; //面朝玩家
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(lookDir);
        }
        lastPlayerPos = player.position;
    }
}
