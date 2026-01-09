using UnityEngine;
using UnityEngine.InputSystem; // ✅ Input System

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

    private Vector3 leftFootCurrent;
    private Vector3 rightFootCurrent;

    private Vector3 leftFootTarget;
    private Vector3 rightFootTarget;

    [Header("Gravity")]
    private float verticalVelocity = 0f;
    private float gravity = -10f;

    // Crouch
    [Header("Crouch Input (Input System)")]
    public InputActionReference crouchAction;

    [Header("Crouch Visuals")]
    public float crouchLeanAngle = 35f;       // 前倾角
    public float crouchBodyDown = 0.25f;      // 身体下移（body本地坐标）
    public float crouchLerp = 12f;            // 过渡速度

    [Header("Crouch Gameplay")]
    public float crouchSpeedMul = 0.55f;      // 蹲下速度倍率
    public float crouchLegMul = 0.70f;        // 腿变短倍率
    public float crouchStepLenMul = 0.65f;    // 步幅倍率
    public float crouchStepHeightMul = 0.60f; // 抬脚倍率

    [Header("CharacterController Crouch (Recommended)")]
    public bool adjustControllerOnCrouch = true;
    public float crouchHeight = 1.2f;
    public float crouchCenterY = 0.6f;

    private bool isCrouching = false;
    private float crouch01 = 0f;
    private Quaternion bodyBaseLocalRot;
    private Vector3 bodyBaseLocalPos;

    private float standHeight;
    private Vector3 standCenter;

    public bool IsCrouching => isCrouching;
    public bool IsRunning { get; private set; }
    public float HorizontalSpeed { get; private set; }

    void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    void Start()
    {
        if (body != null)
        {
            bodyBaseLocalRot = body.localRotation;
            bodyBaseLocalPos = body.localPosition;
        }

        if (cc != null)
        {
            standHeight = cc.height;
            standCenter = cc.center;
        }
    }

    void OnEnable()
    {
        //事件方式：performed=按下, canceled=松开
        if (crouchAction != null && crouchAction.action != null)
        {
            crouchAction.action.performed += OnCrouchPerformed;
            crouchAction.action.canceled += OnCrouchCanceled;
            crouchAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (crouchAction != null && crouchAction.action != null)
        {
            crouchAction.action.performed -= OnCrouchPerformed;
            crouchAction.action.canceled -= OnCrouchCanceled;
            crouchAction.action.Disable();
        }
    }

    private void OnCrouchPerformed(InputAction.CallbackContext _)
    {
        isCrouching = true;  // 按下 -> 蹲
    }

    private void OnCrouchCanceled(InputAction.CallbackContext _)
    {
        isCrouching = false; // 松开 -> 站
    }

    void Update()
    {
        UpdateMovement();
        UpdateCrouchVisuals();
        UpdateLegs();
        UpdateArms();
    }

    void UpdateMovement()
    {
        // 这里仍沿用你的老输入移动（WASD）
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 input = new Vector3(h, 0, v);

        // 相机相对移动
        Vector3 camForward = camFollow.camForward;
        Vector3 camRight = camFollow.camRight;

        Vector3 moveDir = camForward * v + camRight * h;
        moveDir.y = 0f;

        if (moveDir.sqrMagnitude > 0.0001f)
            moveDir.Normalize();

        if (moveDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(moveDir);

        // 速度：蹲下禁跑 + 更慢
        bool wantsRun = Input.GetKey(KeyCode.LeftShift);
        if (isCrouching) wantsRun = false;

        IsRunning = wantsRun;

        float baseSpeed = wantsRun ? runSpeed : walkSpeed;
        float speed = isCrouching ? baseSpeed * crouchSpeedMul : baseSpeed;

        // 重力
        if (cc.isGrounded)
            verticalVelocity = -1f;
        else
            verticalVelocity += gravity * Time.deltaTime;

        moveDir.y = verticalVelocity;
        cc.Move(moveDir * speed * Time.deltaTime);

        Vector3 vel = cc.velocity;
        vel.y = 0f;
        HorizontalSpeed = vel.magnitude;
        // 步伐（蹲下更慢）
        float stepSpeedMul = isCrouching ? 0.75f : 1f;

        if (input.magnitude > 0.1f)
        {
            stepProgress += Time.deltaTime * stepSpeed * stepSpeedMul;
            if (stepProgress >= 1f)
            {
                stepProgress = 0f;
                currentLeg = 1 - currentLeg;
            }
        }
        else
        {
            stepProgress = Mathf.Lerp(stepProgress, 0f, 10f * Time.deltaTime);
        }
    }

    void UpdateCrouchVisuals()
    {
        float target = isCrouching ? 1f : 0f;
        crouch01 = Mathf.Lerp(crouch01, target, crouchLerp * Time.deltaTime);

        if (body != null)
        {
            // 前倾
            Quaternion lean = Quaternion.Euler(crouchLeanAngle * crouch01, 0f, 0f);
            body.localRotation = Quaternion.Slerp(body.localRotation, bodyBaseLocalRot * lean, crouchLerp * Time.deltaTime);

            // 下移
            Vector3 downPos = bodyBaseLocalPos + Vector3.down * (crouchBodyDown * crouch01);
            body.localPosition = Vector3.Lerp(body.localPosition, downPos, crouchLerp * Time.deltaTime);
        }

        // 推荐：控制器也蹲下
        if (adjustControllerOnCrouch && cc != null)
        {
            float targetH = Mathf.Lerp(standHeight, crouchHeight, crouch01);
            float targetCY = Mathf.Lerp(standCenter.y, crouchCenterY, crouch01);

            cc.height = Mathf.Lerp(cc.height, targetH, crouchLerp * Time.deltaTime);

            Vector3 c = cc.center;
            c.y = Mathf.Lerp(c.y, targetCY, crouchLerp * Time.deltaTime);
            cc.center = c;
        }
    }

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

        // 蹲下时腿变短、步幅变小、抬脚变低（平滑）
        float legLen = Mathf.Lerp(legLength, legLength * crouchLegMul, crouch01);
        float sLen = Mathf.Lerp(stepLength, stepLength * crouchStepLenMul, crouch01);
        float sHeight = Mathf.Lerp(stepHeight, stepHeight * crouchStepHeightMul, crouch01);

        if (isStepping)
        {
            float forward = Mathf.Sin((step - 0.5f) * Mathf.PI) * sLen;
            float up = Mathf.Cos((step - 0.5f) * Mathf.PI) * sHeight;

            footTarget =
                hipPos
                + transform.forward * forward
                + transform.up * (-legLen + up);
        }
        else
        {
            if (footCurrent == Vector3.zero)
                footTarget = hipPos + transform.up * -legLen;
        }

        footCurrent = Vector3.Lerp(footCurrent, footTarget, 15f * Time.deltaTime);

        Vector3 kneePos =
            Vector3.Lerp(hipPos, footCurrent, 0.5f)
            + transform.forward * kneeRotation;

        upper.SetPosition(0, hipPos);
        upper.SetPosition(1, kneePos);

        lower.SetPosition(0, kneePos);
        lower.SetPosition(1, footCurrent);

        foot.SetPosition(0, footCurrent);
        foot.SetPosition(1, footCurrent + transform.forward * 0.15f);
    }

    void UpdateArms()
    {
        UpdateSingleArm(armLeft, L_armUpper, L_armLower, -1);
        UpdateSingleArm(armRight, R_armUpper, R_armLower, 1);
    }

    void UpdateSingleArm(Transform shoulder, LineRenderer upper, LineRenderer lower, int direction)
    {
        Vector3 s = shoulder.position;

        float swing = Mathf.Sin((stepProgress + (direction == 1 ? 0f : 0.5f)) * Mathf.PI * 2f) * 0.3f;

        Vector3 restOffset =
            transform.right * armOutward * direction +
            transform.up * armUpward +
            Vector3.down * armHang;

        Vector3 handTarget = s + transform.forward * swing + restOffset;

        Vector3 upperDir = (handTarget - s).normalized;
        Vector3 elbow = s + upperDir * upperArmLength;
        elbow += transform.forward * -elbowBendAmount;

        Vector3 lowerDir = (handTarget - elbow).normalized;
        Vector3 hand = elbow + lowerDir * lowerArmLength;

        upper.SetPosition(0, s);
        upper.SetPosition(1, elbow);

        lower.SetPosition(0, elbow);
        lower.SetPosition(1, hand);
    }
}
