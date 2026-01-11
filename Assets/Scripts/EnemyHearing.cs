using UnityEngine;

public class EnemyHearing : MonoBehaviour
{
    [Header("Dynamic Hearing Range")]
    public float minHearingRange = 3f; 
    public float maxHearingRange = 9.0f; 
    public float hearingLerp = 10f;

    [Header("Duck Hearing (Fixed Range)")]
    public float duckRange = 12f;  
    public float duckHeardMemory = 1.2f; 

    [Header("Refs")]
    public Transform player;
    public PlayerController playerCtrl;
    public EnemyStayTimer stayTimer;
    public EnemyMove mover;

    [Header("Runtime")]
    public bool isPlayerHeard;
    public Vector3 lastHeardPos;

    private float duckTimer = 0f;

    public float currentHearingRange;

    void Awake()
    {
        if (stayTimer == null) stayTimer = GetComponent<EnemyStayTimer>();
        if (mover == null) mover = GetComponent<EnemyMove>();
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player != null && playerCtrl == null)
            playerCtrl = player.GetComponent<PlayerController>();
    }
    void OnEnable()
    {
        PlayerDuck.OnDuck += OnDuck;
    }

    void OnDisable()
    {
        PlayerDuck.OnDuck -= OnDuck;
    }

    void Update()
    {
        float targetRange = minHearingRange;

        if (playerCtrl != null)
        {
            targetRange = Mathf.Lerp(minHearingRange, maxHearingRange, playerCtrl.Noise01);
        }

        currentHearingRange = Mathf.Lerp(currentHearingRange, targetRange, hearingLerp * Time.deltaTime);

        bool nearHeard = false;

        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            nearHeard = dist <= currentHearingRange;

            if (nearHeard)
                lastHeardPos = player.position;
        }

        // Duck
        if (duckTimer > 0f)
        {
            duckTimer -= Time.deltaTime;
        }

        // Add time
        isPlayerHeard = nearHeard || duckTimer > 0f;
    }

    void OnDuck(Vector3 duckPos)
    {
        float dist = Vector3.Distance(transform.position, duckPos);
        if (dist > duckRange) return;

        lastHeardPos = duckPos;
        duckTimer = duckHeardMemory;

        if (mover != null)
            mover.GoInvestigate(duckPos);
        
        if (stayTimer != null)
            stayTimer.ApplyDuckAlertJump();
    }

    void OnDrawGizmosSelected()
    {
        // 实时听觉圈（运行时更有用）
        Gizmos.color = Color.yellow;
        float r = Application.isPlaying ? currentHearingRange : minHearingRange;
        Gizmos.DrawWireSphere(transform.position, r);

        // Duck 范围
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, duckRange);
    }
}