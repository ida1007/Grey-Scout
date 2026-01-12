using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target; //玩家位置

    [Header("Setting")]
    public float height; //摄像机高度
    public float distance; //摄像机与player距离
    public float followSpeed; //移动平滑
    public float rotateSpeed; //旋转平滑

    [Header("Ratation Limits")]
    public float minMoveX;
    public float maxMoveX;
    public float minMoveY;
    public float maxMoveY;

    [Header("Mouse Setting")]
    public float mouseSensitivityX; 
    public float mouseSensitivityY; 

    [HideInInspector] public Vector3 camForward;
    [HideInInspector] public Vector3 camRight;

    private float moveX;
    private float moveY;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    /*public Vector3 offset = new Vector3(0, 2, -5);
    public float smooth = 5f;
    */
    private void LateUpdate()
    {
        if (target == null) return;

        /*Vector3 targetPos = target.position + offset;
        transform.position = new Vector3(targetPos.x,targetPos.y,targetPos.z);

        transform.LookAt(target.position);
        */
        moveX += Input.GetAxis("Mouse X") * mouseSensitivityX * Time.deltaTime;
        moveY -= Input.GetAxis("Mouse Y") * mouseSensitivityY * Time.deltaTime;
        //moveX = Mathf.Clamp(moveX,minMoveX,maxMoveX);
        moveY = Mathf.Clamp(moveY, minMoveY, maxMoveY);

        Quaternion rotation = Quaternion.Euler(moveY, moveX, 0);

        //Player orientation synchronisation
        Vector3 forward = rotation * Vector3.forward;
        forward.y = 0f;
        //target.forward = forward;

        //Adjust rotation for player
        camForward = forward.normalized;
        camRight = (rotation * Vector3.right).normalized;
        camRight.y = 0;

        //camera position
        Vector3 offset = rotation * new Vector3(0,height,-distance);
        Vector3 targetPos = target.position + offset;

        //cam move
        transform.position = Vector3.Lerp(transform.position,targetPos, followSpeed * Time.deltaTime);

        //cam look at Player
        Quaternion lookRot = Quaternion.LookRotation(target.position + Vector3.up - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, rotateSpeed * Time.deltaTime);
    }
}
