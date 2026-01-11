using UnityEngine;

public class DuckVisualEmitter : MonoBehaviour
{
    [Header("Prefab")]
    public VFX_DuckVisualParticle particlePrefab;

    [Header("Spawn")]
    public int minCount = 2;
    public int maxCount = 3;

    [Header("Spawn Offset (local)")]
    public Vector3 localOffset = new Vector3(0f, 1.6f, 0.25f);

    [Header("Move")]
    public float forwardSpeed = 1.2f;
    public float upSpeed = 1.0f;
    public float sideJitter = 0.35f;
    public float angleJitterDeg = 18f;

    [Header("Scale")]
    public float startScale = 1.0f;
    public float startScaleRand = 0.25f;
    public float peakScale = 1.25f;
    public float peakScaleRand = 0.15f;

    [Header("Spawn Spread")]
    public float spawnRadius = 1f;   
    public float spawnForward = 0.10f;
    public float spawnUp = 0.06f;

    [Header("Orientation")]
    public float rollJitterDeg = 25f;   
    public bool faceCamera = false;     
    [Header("Life")]
    public float life = 0.35f;

    void OnEnable()
    {
        PlayerDuck.OnDuck += HandleDuck;
    }

    void OnDisable()
    {
        PlayerDuck.OnDuck -= HandleDuck;
    }

    void HandleDuck(Vector3 duckWorldPos)
    {
        Emit();
    }

    public void Emit()
    {
        if (particlePrefab == null) return;

        int count = Random.Range(minCount, maxCount + 1);

        // birthplace
        Vector3 spawnPos = transform.TransformPoint(localOffset);

        for (int i = 0; i < count; i++)
        {
            var p = Instantiate(particlePrefab, spawnPos, Quaternion.identity);

            // spare
            Vector2 r = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnJitter =
                transform.right * r.x +
                transform.up * (r.y * 0.6f) +
                transform.forward * Random.Range(0f, spawnForward) +
                transform.up * Random.Range(0f, spawnUp);

            p.transform.position += spawnJitter;

            // direction
            Vector3 baseDir = (transform.forward * forwardSpeed + Vector3.up * upSpeed).normalized;

            float yaw = Random.Range(-angleJitterDeg, angleJitterDeg);
            float pitch = Random.Range(-angleJitterDeg * 0.35f, angleJitterDeg * 0.35f);
            Vector3 dir = Quaternion.Euler(pitch, yaw, 0f) * baseDir;

            // spead and spear
            float spd = Random.Range(0.95f, 1.25f);
            Vector3 v = dir * spd;
            v += transform.right * Random.Range(-sideJitter, sideJitter) * 0.35f;

            p.velocity = v;

            // rotation
            float roll = Random.Range(-rollJitterDeg, rollJitterDeg);
            p.rollDeg = roll;                  
            p.billboardToCamera = true;         


            // routine
            p.life = life * Random.Range(0.9f, 1.15f);
            p.startScale = startScale + Random.Range(-startScaleRand, startScaleRand);
            p.peakScale = peakScale + Random.Range(-peakScaleRand, peakScaleRand);
        }
    }
}
