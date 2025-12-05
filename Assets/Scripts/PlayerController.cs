using UnityEngine;

public class PlayerController : MonoBehaviour
{
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

    private CharacterController cc;
    private float stepProgress = 0f;
    private float stepSpeed = 4f;
    private int currentLeg = 0;

    Vector3 leftFootCurrent;
    Vector3 rightFootCurrent;

    Vector3 leftFootTarget;
    Vector3 rightFootTarget;


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

    
    // 角色移动
    void UpdateMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 input = new Vector3(h, 0, v);

        // 转向
        if (input.magnitude > 0.1f)
        {
            Vector3 lookDir = input.normalized;
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.LookRotation(lookDir),
                10f * Time.deltaTime
            );
        }

        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        cc.Move(transform.forward * input.magnitude * speed * Time.deltaTime);

        // 更新步伐进度
        if (input.magnitude > 0.1f)
        {
            stepProgress += Time.deltaTime * stepSpeed;

            if (stepProgress >= 1f)
            {
                stepProgress = 0f;
                currentLeg = 1 - currentLeg;  // 换腿
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
        float stepLength = 0.3f;
        float stepHeight = 0.15f;

        // 更新脚的目标
        if (isStepping)
        {
            float forward = Mathf.Sin((step - 0.5f) * Mathf.PI) * stepLength;
            float up = Mathf.Cos((step - 0.5f) * Mathf.PI) * stepHeight;

            footTarget =
                hipPos
                + transform.forward * forward
                + transform.up * (-1f + up);
        }
        else
        {
            //支撑脚：保持落地瞬间的位置，不动
            // 初始化
            if (footCurrent == Vector3.zero)
                footTarget = hipPos + transform.up * -1f;
        }
        // 平滑移动脚
        footCurrent = Vector3.Lerp(footCurrent, footTarget, 15f * Time.deltaTime);

        // 自动生成膝盖
        Vector3 kneePos =
            Vector3.Lerp(hipPos, footCurrent, 0.5f)
            + transform.forward * 0.2f;

        // 绘制线段
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

        
        float swing = Mathf.Sin((stepProgress + (direction == 1 ? 0 : 0.5f)) * Mathf.PI * 2f) * 0.3f;// 控制摆动
        float outward = 0.4f;// 手臂展开
        float upward = 0.2f;// 手臂抬起

        // 手部位置偏移
        Vector3 restOffset =
            transform.right * outward * direction +  // 向外张开
            transform.up * upward +                   // 向上
            Vector3.down * 0.5f;                      // 向下挂着

        Vector3 hand = s + transform.forward * swing + restOffset; // 最终手部位置
        Vector3 elbow = (s + hand) * 0.5f + transform.forward * -0.15f; // 肘部偏移

        upper.SetPosition(0, s);
        upper.SetPosition(1, elbow);

        lower.SetPosition(0, elbow);
        lower.SetPosition(1, hand);
    }
}
