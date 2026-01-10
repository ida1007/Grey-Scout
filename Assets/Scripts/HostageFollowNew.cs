using UnityEngine;

public class HostageFollowNew: MonoBehaviour
{
    [Header("Chain Follow")]
    public Transform player;
    public Transform followTarget;

    [Header("HostageFollow")]
    public bool isHostageFollowing;
    public float followDistance = 2f;
    public float followSpeed = 3f;
    public float horizontalSmooth = 12f;

    [Header("Sync Player 3C State")]
    public PlayerController player3C;

    // Animation from PlayerController

    [Header("Leg Settings")]
    public float legLength = 0.6f;
    public float kneeRotation = 0.1f;
    public float stepLength = 0.6f;
    public float stepHeight = 0.15f;

    [Header("Arm IK Settings")]
    public float upperArmLength = 0.2f;
    public float lowerArmLength = 0.2f;
    public float elbowBendAmount = 0.1f;
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

    [Header("Step Control")]
    public float stepSpeed = 4f;              
    public float walkStepMul = 1.0f;          
    public float runStepMul = 1.45f;          
    public float idleReturnSpeed = 10f;       
    public float movingThreshold = 0.08f;    

    private float stepProgress = 0f;
    private int currentLeg = 0;

    private Vector3 leftFootCurrent;
    private Vector3 rightFootCurrent;

    private Vector3 leftFootTarget;
    private Vector3 rightFootTarget;

    // Crouch visuals (same as PlayerController)
    [Header("Crouch Visuals")]
    public float crouchLeanAngle = 35f;
    public float crouchBodyDown = 0.25f;
    public float crouchLerp = 12f;

    [Header("Crouch Animation Multipliers")]
    public float crouchLegMul = 0.70f;
    public float crouchStepLenMul = 0.65f;
    public float crouchStepHeightMul = 0.60f;

    private float crouch01 = 0f;
    private Quaternion bodyBaseLocalRot;
    private Vector3 bodyBaseLocalPos;

    // runtime 
    [Header("Gravity (same as Player)")]
    public float gravity = -10f;
    private float verticalVelocity = 0f;

    private CharacterController cc;
    public Vector3 lastTargetPos;

    private Vector3 horizontalVel = Vector3.zero;

    void Awake()
    {
        cc = GetComponent<CharacterController>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player3C == null && player != null)
            player3C = player.GetComponent<PlayerController>();

    }

    void Start()
    {
        if (followTarget == null)
           followTarget = HostageManager.Instance.GetLastHostage();

        if (followTarget != null) lastTargetPos = followTarget.position;

        if (body != null)
        {
            bodyBaseLocalRot = body.localRotation;
            bodyBaseLocalPos = body.localPosition;
        }
    }

    void Update()
    {
        // player status
        bool playerCrouch = (player3C != null) && player3C.IsCrouching;
        bool playerRun = (player3C != null) && player3C.IsRunning;

        // update gravity
        Vector3 displacement = Vector3.zero;
        ApplyGravity(ref displacement);

        // before following
        if (!isHostageFollowing || player == null)
        {
            UpdateCrouchVisuals(false);
            UpdateStepBySpeed(0f, false, false);

            cc.Move(displacement);

            UpdateLegs();
            UpdateArms();

            if (followTarget != null) lastTargetPos = followTarget.position;
            return;
        }

        // move direction
        Vector3 targetMoveDir = followTarget.position - lastTargetPos;
        targetMoveDir.y = 0f;

        if (targetMoveDir.sqrMagnitude > 0.0001f)
            targetMoveDir.Normalize();

        Vector3 targetPos = followTarget.position - targetMoveDir * followDistance;
        //targetPos.y = transform.position.y;
        Vector3 toTarget = targetPos - transform.position;
        toTarget.y = 0f;

        //计算水平方向
        //Vector3 moveDir = Vector3.zero;
        //if (currentDist > followDistance)
        //{
        //    transform.position = Vector3.Lerp(
        //        transform.position,
        //        targetPos,
        //        smoothSpeed * Time.deltaTime
        //    );
        //}
        float distToTarget = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(followTarget.position.x, 0, followTarget.position.z)
            );

        Vector3 desiredHorizontalVel = Vector3.zero;
        if (distToTarget > followDistance)
        {
            Vector3 dir = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : Vector3.zero;

            float t = Mathf.InverseLerp(followDistance, followDistance + 2.0f, distToTarget);
            float spd = Mathf.Lerp(0.0f, followSpeed, t);

            desiredHorizontalVel = dir * spd;
        }

        // 平滑水平速度（避免抖动）
        horizontalVel = Vector3.Lerp(horizontalVel, desiredHorizontalVel, horizontalSmooth * Time.deltaTime);

        displacement += horizontalVel * Time.deltaTime;

        // Move Verizontal & Gravity
        cc.Move(displacement);

        // face player
        Vector3 lookDir = followTarget.position - transform.position;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(lookDir);

        // upload lastPlayerPos
        lastTargetPos = followTarget.position;

        // 4) 动画切换依据：玩家速度（优先用玩家 HorizontalSpeed；没有就用玩家位移估算）
        float playerHorizontalSpeed = 0f;

        if (player3C != null && player3C.HorizontalSpeed > 0f)
        {
            playerHorizontalSpeed = player3C.HorizontalSpeed;
        }
        else
        {
            //playerHorizontalSpeed = (currentDist > followDistance + 0.05f) ? 1f : 0f;
            playerHorizontalSpeed = (desiredHorizontalVel.magnitude > 0.01f) ? 1f : 0f;
        }

        bool playerMoving = playerHorizontalSpeed > movingThreshold;

        // chrouch visuals
        UpdateCrouchVisuals(playerCrouch);

        // Idle/Walk/Run/CrouchWalk
        UpdateStepBySpeed(playerHorizontalSpeed, playerMoving, playerRun);

        // update legs&arms
        UpdateLegs();
        UpdateArms();
    }

    //gravity
    void ApplyGravity(ref Vector3 displacement)
    {
        if (cc.isGrounded)
        {
            if (verticalVelocity < 0f) verticalVelocity = -1f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        displacement.y += verticalVelocity * Time.deltaTime;
    }

    // Crouch Body

    void UpdateCrouchVisuals(bool isCrouching)
    {
        float target = isCrouching ? 1f : 0f;
        crouch01 = Mathf.Lerp(crouch01, target, crouchLerp * Time.deltaTime);

        if (body != null)
        {
            Quaternion lean = Quaternion.Euler(crouchLeanAngle * crouch01, 0f, 0f);
            body.localRotation = Quaternion.Slerp(body.localRotation, bodyBaseLocalRot * lean, crouchLerp * Time.deltaTime);

            Vector3 downPos = bodyBaseLocalPos + Vector3.down * (crouchBodyDown * crouch01);
            body.localPosition = Vector3.Lerp(body.localPosition, downPos, crouchLerp * Time.deltaTime);
        }
    }

    // ===================== 动画：步态（切换 Walk/Run/Idle） =====================

    void UpdateStepBySpeed(float playerSpeed, bool playerMoving, bool playerRun)
    {
        // 你希望“玩家跑->人质也跑；玩家蹲->人质蹲”，这里步频根据跑步加速
        float stepMul = playerRun ? runStepMul : walkStepMul;

        // 蹲下时步频略慢一点（也可以调）
        stepMul *= Mathf.Lerp(1f, 0.75f, crouch01);

        if (playerMoving)
        {
            stepProgress += Time.deltaTime * stepSpeed * stepMul;

            if (stepProgress >= 1f)
            {
                stepProgress = 0f;
                currentLeg = 1 - currentLeg;
            }
        }
        else
        {
            // 不动 -> 慢慢回到站立相位（腿脚稳定）
            stepProgress = Mathf.Lerp(stepProgress, 0f, idleReturnSpeed * Time.deltaTime);
        }
    }

    // ===================== 动画：腿 =====================

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
        if (hip == null || upper == null || lower == null || foot == null) return;

        Vector3 hipPos = hip.position;
        float step = stepProgress;

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

    // ===================== 动画：手臂 =====================

    void UpdateArms()
    {
        UpdateSingleArm(armLeft, L_armUpper, L_armLower, -1);
        UpdateSingleArm(armRight, R_armUpper, R_armLower, 1);
    }

    void UpdateSingleArm(Transform shoulder, LineRenderer upper, LineRenderer lower, int direction)
    {
        if (shoulder == null || upper == null || lower == null) return;

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
