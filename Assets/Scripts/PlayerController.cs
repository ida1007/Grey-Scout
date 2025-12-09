using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerController : MonoBehaviour
{
    [Header("Leg Settings")]
    public float legLength = 0.4f;
    public float kneeRotation = 0.1f;
    public float stepLength = 0.4f;
    public float stepHeight = 0.15f;

    [Header("Arm IK Settings")]
    public float upperArmLength = 0.4f;
    public float lowerArmLength = 0.4f;
    public float elbowBendAmount = 0.15f;
    public float armOutward = 0.4f;
    public float armUpward = 0.2f;
    public float armHang = 0.5f;

    [Header("Body References")]
    public Transform body;
    public Transform head;

    public Transform legLeft;
    public Transform legRight;

    public Transform armLeft;
    public Transform armRight;

    [Header("LineRenderers (Legs)")]
    public LineRenderer L_legUpper;
    public LineRenderer L_legLower;
    public LineRenderer L_legFoot;

    public LineRenderer R_legUpper;
    public LineRenderer R_legLower;
    public LineRenderer R_legFoot;

    [Header("LineRenderers (Arms)")]
    public LineRenderer L_armUpper;
    public LineRenderer L_armLower;

    public LineRenderer R_armUpper;
    public LineRenderer R_armLower;

    [Header("Movement")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;

    public CameraFollow camFollow;

    private CharacterController cc;
    private float stepProgress = 0f;
    private float stepSpeed = 4f;
    private int currentLeg = 0;

    Vector3 leftFootCurrent;
    Vector3 rightFootCurrent;

    Vector3 leftFootTarget;
    Vector3 rightFootTarget;

    float verticalVelocity = 0f;
    float gravity = -10f;


    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        UpdateMovement();
        UpdateLegs();
        UpdateArms();
    }

    
    //角色移动
    void UpdateMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 input = new Vector3(h, 0, v);
        //根据摄像机角度修正移动方向
        Vector3 camForward = camFollow.camForward;
        Vector3 camRight = camFollow.camRight;
        Vector3 moveDir = camForward * v + camRight * h;
        moveDir.y = 0;
        moveDir.Normalize();
        // 转向
        if (moveDir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(moveDir);
        }

        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        // 重力
        if (cc.isGrounded)
        {
            verticalVelocity = -1f; // 保持贴地
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        moveDir.y = verticalVelocity;
        cc.Move(moveDir * speed * Time.deltaTime);

        // 步伐逻辑
        if (input.magnitude > 0.1f)
        {
            stepProgress += Time.deltaTime * stepSpeed;

            if (stepProgress >= 1f)
            {
                stepProgress = 0f;
                currentLeg = 1 - currentLeg;  //换腿
            }
        }
    }

    // 腿程序IK
    void UpdateLegs()
    {
        UpdateSingleLeg(legLeft, L_legUpper, L_legLower, L_legFoot, currentLeg == 0, ref leftFootCurrent, ref leftFootTarget);
        UpdateSingleLeg(legRight, R_legUpper, R_legLower, R_legFoot, currentLeg == 1, ref rightFootCurrent, ref rightFootTarget);
    }


    void UpdateSingleLeg(
    Transform hip,
    LineRenderer upper,
    LineRenderer lower,
    LineRenderer foot,
    bool isStepping,
    ref Vector3 footCurrent,
    ref Vector3 footTarget
)
    {
        Vector3 hipPos = hip.position;

        float step = stepProgress;

        // 更新脚的目标
        if (isStepping)
        {
            float forward = Mathf.Sin((step - 0.5f) * Mathf.PI) * stepLength;
            float up = Mathf.Cos((step - 0.5f) * Mathf.PI) * stepHeight;

            footTarget =
                hipPos
                + transform.forward * forward
                + transform.up * (-legLength + up);
        }
        else
        {
            //支撑脚：保持落地瞬间的位置，不动
            //初始化
            if (footCurrent == Vector3.zero)
                footTarget = hipPos + transform.up * -legLength;
        }
        //平滑移动脚
        footCurrent = Vector3.Lerp(footCurrent, footTarget, 15f * Time.deltaTime);

        //自动生成膝盖
        Vector3 kneePos =
            Vector3.Lerp(hipPos, footCurrent, 0.5f)
            + transform.forward * kneeRotation;

        //绘制线段
        upper.SetPosition(0, hipPos);
        upper.SetPosition(1, kneePos);

        lower.SetPosition(0, kneePos);
        lower.SetPosition(1, footCurrent);

        foot.SetPosition(0, footCurrent);
        foot.SetPosition(1, footCurrent + transform.forward * 0.15f);
    }



    // 手臂摆动
    void UpdateArms()
    {
        UpdateSingleArm(armLeft, L_armUpper, L_armLower, -1);
        UpdateSingleArm(armRight, R_armUpper, R_armLower, 1);
    }

    void UpdateSingleArm(Transform shoulder, LineRenderer upper, LineRenderer lower, int direction)
    {
        Vector3 s = shoulder.position;

        // 让摆动速度跟腿相同
        float swing = Mathf.Sin((stepProgress + (direction == 1 ? 0f : 0.5f)) * Mathf.PI * 2f) * 0.3f;

        // 基础偏移，可调节
        Vector3 restOffset =
            transform.right * armOutward * direction +  // 左右展开
            transform.up * armUpward +                 // 抬起一些
            Vector3.down * armHang;                    // 下垂

        Vector3 handTarget = s + transform.forward * swing + restOffset;

        //计算肘部位置
        Vector3 upperDir = (handTarget - s).normalized;
        Vector3 elbow = s + upperDir * upperArmLength;

        //可调弯曲程度
        elbow += transform.forward * -elbowBendAmount;

        //手的位置离肘部
        Vector3 lowerDir = (handTarget - elbow).normalized;
        Vector3 hand = elbow + lowerDir * lowerArmLength;

        //上臂与前臂
        upper.SetPosition(0, s);
        upper.SetPosition(1, elbow);

        lower.SetPosition(0, elbow);
        lower.SetPosition(1, hand);
    }

}
