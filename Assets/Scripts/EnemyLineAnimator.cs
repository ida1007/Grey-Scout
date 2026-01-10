using UnityEngine;
using UnityEngine.AI;

public class EnemyLineAnimator : MonoBehaviour
{
    [Header("Refs")]
    public NavMeshAgent agent;
    public EnemyFollow follow;

    [Header("Leg Settings")]
    public float legLength = 0.6f;
    public float kneeRotation = 0.1f;
    public float stepLength = 0.6f;
    public float stepHeight = 0.15f;
    public float stepSpeed = 7f;
    public float moveThreshold = 0.15f;

    [Header("Body (Optional)")]
    public Transform body;
    public float bodyBobAmount = 0.05f;     // 上下轻微起伏
    public float bodyBobSpeed = 6f;
    private Vector3 bodyBaseLocalPos;

    [Header("Arms Pose (Static)")]
    public Transform armLeft;
    public Transform armRight;

    public Vector3 leftHandOffset = new Vector3(0.35f, 0.22f, 0.05f);
    public Vector3 leftElbowOffset = new Vector3(0.38f, 0.13f, 0.00f);

    public Vector3 rightHandOffset = new Vector3(0.72f, -0.3f, -0.22f);
    public Vector3 rightElbowOffset = new Vector3(0.19f, -1.06f, 1.55f);

    public float upperArmLength = 0.25f;
    public float lowerArmLength = 0.3f;

    public LineRenderer L_armUpper;
    public LineRenderer L_armLower;

    public LineRenderer R_armUpper;
    public LineRenderer R_armLower;

    [Header("Attack (Right Arm Swing)")]
    public bool freezeLegsOnAttack = true;   // leg frozen
    public float attackHz = 0.5f; // 每秒几次（建议 1.2~2）
    [Range(0.05f, 0.6f)] public float windupPortion = 0.5f; // 蓄力占比
    [Range(0.05f, 0.6f)] public float strikePortion = 0.22f; // 刺击占比（越小越快）
    [Range(0.5f, 3f)] public float attackMotionScale = 2.0f;  // 总体放大倍率（你要更大就调这个）

    [Header("Attack Once Pose (Right Arm)")]
    public Vector3 windupHandDelta = new Vector3(-0.09f, -0.2f, -0.4f); // 蓄力：外展
    public Vector3 windupElbowDelta = new Vector3(-0.56f, 0.5f, -0.7f);

    public Vector3 strikeHandDelta = new Vector3(0.35f, 0.05f, 2f); // 向前刺
    public Vector3 strikeElbowDelta = new Vector3(0.15f, 0f, 1.5f);

    [Header("Right Spear")]
    public Transform rightSpear;                 // Spear_L
    public Vector3 rightSpearLocalOffset = new Vector3(0f, 0f, 0f); // 微调位置
    public Vector3 rightSpearLocalEuler = new Vector3(10f, 0f, 0f);  // 微调旋转
    public float rightSpearLength = 0.5f;

    [Header("Right Spear Attack Rotation")]
    public Vector3 spearWindupEuler = new Vector3(-25f, 0f, 10f);   // 蓄力时往后/上翻一点
    public Vector3 spearStrikeEuler = new Vector3(15f, 0f, -5f);    // 刺击时往前压一点
    public float spearEulerScale = 1f;                               // 旋转幅度放大

    [Header("Leg Transforms")]
    public Transform legLeft;
    public Transform legRight;
    public LineRenderer L_legUpper;
    public LineRenderer L_legLower;
    public LineRenderer L_legFoot;
    public LineRenderer R_legUpper;
    public LineRenderer R_legLower;
    public LineRenderer R_legFoot;

    private float stepProgress;
    private int currentLeg; // 0=left, 1=right

    private Vector3 leftFootCurrent;
    private Vector3 rightFootCurrent;
    private Vector3 leftFootTarget;
    private Vector3 rightFootTarget;

    private Vector3 cachedRightElbowWS;
    private Vector3 cachedRightHandWS;
    private bool hasCachedRightArm;

    private Quaternion cachedRightSpearExtraRot = Quaternion.identity;
    private bool hasCachedRightSpearRot;

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (follow == null) follow = GetComponent<EnemyFollow>();
    }

    void Start()
    {
        if (body != null) bodyBaseLocalPos = body.localPosition;
    }

    void Update()
    {
        if (agent == null) return;

        hasCachedRightArm = false;
        hasCachedRightSpearRot = false;

        bool isAttacking = (follow != null && follow.isAttacking);

        // NavMeshAgent
        Vector3 vel = agent.velocity;
        vel.y = 0f;
        float speed = vel.magnitude;

        bool isMoving = speed > moveThreshold && !agent.isStopped;

        bool shouldFreezeLegs = isAttacking && freezeLegsOnAttack && agent.isStopped;

        if (shouldFreezeLegs)
        {
            stepProgress = Mathf.Lerp(stepProgress, 0f, 10f * Time.deltaTime);
            UpdateLegs();
            UpdateBody(false);
        }
        else
        {
            UpdateStep(isMoving, speed);
            UpdateLegs();
            UpdateBody(isMoving);
        }

        if (isAttacking)
        {
            // 左手固定（提灯）
            DrawFixedArm(armLeft, L_armUpper, L_armLower, leftElbowOffset, leftHandOffset, -1);

            // 右手挥臂（长矛攻击）
            DrawTwoStageStrikeRightArm();
            UpdateRightSpearFromArm();
        }
        else
        {
            // 非攻击：双手固定姿态
            UpdateFixedArms();
            UpdateRightSpearFromArm();
        }
    }

    void UpdateStep(bool isMoving, float speed)
    {
        if (isMoving)
        {
            // 速度越大，迈步越快（你也可以固定不随速度变化）
            float speedMul = Mathf.Clamp(speed / Mathf.Max(0.01f, agent.speed), 0.6f, 1.4f);

            stepProgress += Time.deltaTime * stepSpeed * speedMul;
            if (stepProgress >= 1f)
            {
                stepProgress = 0f;
                currentLeg = 1 - currentLeg;
            }
        }
        else
        {
            // 停下时让步伐缓慢回到 0，脚稳住
            stepProgress = Mathf.Lerp(stepProgress, 0f, 10f * Time.deltaTime);
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
        if (hip == null || upper == null || lower == null || foot == null) return;

        Vector3 hipPos = hip.position;
        float step = stepProgress;

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
            // 非迈步腿：保持在“正下方”
            if (footCurrent == Vector3.zero)
                footTarget = hipPos + transform.up * -legLength;
            else
                footTarget = hipPos + transform.up * -legLength;
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

    void UpdateBody(bool isMoving)
    {
        if (body == null) return;

        if (isMoving)
        {
            float bob = Mathf.Sin(Time.time * bodyBobSpeed) * bodyBobAmount;
            Vector3 target = bodyBaseLocalPos + Vector3.up * bob;
            body.localPosition = Vector3.Lerp(body.localPosition, target, 12f * Time.deltaTime);
        }
        else
        {
            body.localPosition = Vector3.Lerp(body.localPosition, bodyBaseLocalPos, 12f * Time.deltaTime);
        }
    }

    void UpdateFixedArms()
    {
        // 左手：提灯（向前、向上）
        DrawFixedArm(
            armLeft,
            L_armUpper,
            L_armLower,
            leftElbowOffset,
            leftHandOffset,
            -1 // 左手
        );

        // 右手：长矛（微弯、略抬）
        DrawFixedArm(
            armRight,
            R_armUpper,
            R_armLower,
            rightElbowOffset,
            rightHandOffset,
            1,
            true// 右手
        );
    }

    void DrawFixedArm(
    Transform shoulder,
    LineRenderer upper,
    LineRenderer lower,
    Vector3 elbowOffset,
    Vector3 handOffset,
    int side,
    bool cacheAsRightArm = false
)
    {
        if (shoulder == null || upper == null || lower == null) return;

        Vector3 s = shoulder.position;

        Vector3 elbow =
            s + transform.forward * elbowOffset.x
              + transform.up * elbowOffset.y
              + transform.right * elbowOffset.z * side;

        Vector3 hand =
            s + transform.forward * handOffset.x
              + transform.up * handOffset.y
              + transform.right * handOffset.z * side;

        Vector3 upperDir = (elbow - s).normalized;
        elbow = s + upperDir * upperArmLength;

        Vector3 lowerDir = (hand - elbow).normalized;
        hand = elbow + lowerDir * lowerArmLength;

        upper.SetPosition(0, s);
        upper.SetPosition(1, elbow);

        lower.SetPosition(0, elbow);
        lower.SetPosition(1, hand);

        if (cacheAsRightArm)
        {
            cachedRightElbowWS = elbow;
            cachedRightHandWS = hand;
            hasCachedRightArm = true;
        }
    }


    void DrawTwoStageStrikeRightArm()
    {
        if (armRight == null || R_armUpper == null || R_armLower == null) return;

        Vector3 s = armRight.position;

        Vector3 baseElbow =
            s + transform.forward * rightElbowOffset.x
              + transform.up * rightElbowOffset.y
              + transform.right * rightElbowOffset.z;

        Vector3 baseHand =
            s + transform.forward * rightHandOffset.x
              + transform.up * rightHandOffset.y
              + transform.right * rightHandOffset.z;

        float phase = Mathf.Repeat(Time.time * attackHz, 1f);

        float wEnd = Mathf.Clamp01(windupPortion);
        float sEnd = Mathf.Clamp01(wEnd + strikePortion);

        // 0..1 平滑
        static float Ease(float t) => t * t * (3f - 2f * t);

        Vector3 elbow = baseElbow;
        Vector3 hand = baseHand;

        if (phase < wEnd)
        {
            // Windup：从 base -> base + windup
            float t = Ease(phase / Mathf.Max(0.0001f, wEnd));
            elbow = Vector3.Lerp(baseElbow, baseElbow + windupElbowDelta * attackMotionScale, t);
            hand = Vector3.Lerp(baseHand, baseHand + windupHandDelta * attackMotionScale, t);
        }
        else if (phase < sEnd)
        {
            // Strike：从 windup -> strike（快速向前刺）
            float t = Ease((phase - wEnd) / Mathf.Max(0.0001f, (sEnd - wEnd)));
            Vector3 eWind = baseElbow + windupElbowDelta * attackMotionScale;
            Vector3 hWind = baseHand + windupHandDelta * attackMotionScale;
            Vector3 eStr = baseElbow + strikeElbowDelta * attackMotionScale;
            Vector3 hStr = baseHand + strikeHandDelta * attackMotionScale;
            elbow = Vector3.Lerp(eWind, eStr, t);
            hand = Vector3.Lerp(hWind, hStr, t);
        }
        else
        {
            // Return：从 strike -> base（回位）
            float t = Ease((phase - sEnd) / Mathf.Max(0.0001f, (1f - sEnd)));
            Vector3 eStr = baseElbow + strikeElbowDelta * attackMotionScale;
            Vector3 hStr = baseHand + strikeHandDelta * attackMotionScale;
            elbow = Vector3.Lerp(eStr, baseElbow, t);
            hand = Vector3.Lerp(hStr, baseHand, t);
        }

        //  Spear rotation animation (cached)
        Vector3 spearEuler = Vector3.zero;

        if (phase < wEnd)
        {
            float t = Ease(phase / Mathf.Max(0.0001f, wEnd));
            spearEuler = Vector3.Lerp(Vector3.zero, spearWindupEuler * spearEulerScale, t);
        }
        else if (phase < sEnd)
        {
            float t = Ease((phase - wEnd) / Mathf.Max(0.0001f, (sEnd - wEnd)));
            Vector3 eWind = spearWindupEuler * spearEulerScale;
            Vector3 eStr = spearStrikeEuler * spearEulerScale;
            spearEuler = Vector3.Lerp(eWind, eStr, t);
        }
        else
        {
            float t = Ease((phase - sEnd) / Mathf.Max(0.0001f, (1f - sEnd)));
            Vector3 eStr = spearStrikeEuler * spearEulerScale;
            spearEuler = Vector3.Lerp(eStr, Vector3.zero, t);
        }

        cachedRightSpearExtraRot = Quaternion.Euler(spearEuler);
        hasCachedRightSpearRot = true;

        // 限制骨长
        Vector3 upperDir = (elbow - s).normalized;
        elbow = s + upperDir * upperArmLength;

        Vector3 lowerDir = (hand - elbow).normalized;
        hand = elbow + lowerDir * lowerArmLength;

        R_armUpper.SetPosition(0, s);
        R_armUpper.SetPosition(1, elbow);

        R_armLower.SetPosition(0, elbow);
        R_armLower.SetPosition(1, hand);

        cachedRightElbowWS = elbow;
        cachedRightHandWS = hand;
        hasCachedRightArm = true;
    }
    void UpdateRightSpearFromArm()
    {
        if (rightSpear == null) return;
        if (!hasCachedRightArm) return;

        Vector3 elbow = cachedRightElbowWS;
        Vector3 hand = cachedRightHandWS;

        Vector3 dir = hand - elbow;
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();

        // 位置：跟随手 + 向前推一点（握把位置）
        rightSpear.position = hand + dir * (rightSpearLength * 0.2f);

        // 朝向：矛的 forward(+Z) 指向手的方向
        rightSpear.rotation = Quaternion.LookRotation(dir, transform.up);

        // 微调
        rightSpear.position += rightSpear.TransformDirection(rightSpearLocalOffset);
        rightSpear.rotation *= Quaternion.Euler(rightSpearLocalEuler);

        if (hasCachedRightSpearRot)
            rightSpear.rotation *= cachedRightSpearExtraRot;
    }
}
