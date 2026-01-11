using UnityEngine;

public class GrassSpawnerGridByHeight : MonoBehaviour
{
    [Header("Refs")]
    public Terrain terrain;
    public GameObject grassPrefab;

    [Header("Height Filter (World Y)")]
    public float minWorldHeight = 0.6f;
    public float maxWorldHeight = 5f;

    [Header("Density (Grid)")]
    public float spacing = 1.5f;
    public float jitter = 0.35f;

    [Header("Slope Filter")]
    [Range(0f, 90f)] public float maxSlopeDeg = 40f;

    [Header("Look Randomness")]
    public bool alignToNormal = true;
    public Vector2 randomYawDeg = new Vector2(0f, 360f);
    public Vector2 randomScale = new Vector2(0.85f, 1.25f);

    [Header("Optional Noise Mask (Natural patches)")]
    public bool useNoiseMask = false;
    public float noiseScale = 0.03f;
    [Range(0f, 1f)]
    public float noiseThreshold = 0.5f;

    [Header("Exclude Near Obstacles (Obstacle layer is OK even if Terrain is also on it)")]
    public LayerMask obstacleMask;
    public float excludeRadius = 1.5f;
    public float probeHeight = 1.6f;
    public bool includeTriggersAsObstacles = false;

    [Header("Parent / Cleanup")]
    public Transform parent;
    public bool clearChildrenOnSpawn = true;

    [Header("Safety")]
    public int maxInstances = 50000;

    // 为了减少 GC：复用数组（可选，但很实用）
    private Collider[] _overlapBuffer = new Collider[64];

    void Start()
    {
        if (terrain == null) terrain = FindFirstObjectByType<Terrain>();
        if (terrain == null || grassPrefab == null)
        {
            Debug.LogError("[GrassSpawnerGridByHeight] Missing Terrain or grassPrefab.");
            return;
        }

        if (parent == null) parent = transform;
        Spawn();
    }

    [ContextMenu("Spawn")]
    public void Spawn()
    {
        if (clearChildrenOnSpawn)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                DestroyImmediate(parent.GetChild(i).gameObject);
        }

        TerrainData td = terrain.terrainData;
        Vector3 tPos = terrain.transform.position;
        Vector3 size = td.size;

        int spawned = 0;

        QueryTriggerInteraction qti = includeTriggersAsObstacles
            ? QueryTriggerInteraction.Collide
            : QueryTriggerInteraction.Ignore;

        // 网格遍历：以 spacing 为步长扫过整个地形 XZ
        for (float x = 0f; x <= size.x; x += spacing)
        {
            for (float z = 0f; z <= size.z; z += spacing)
            {
                if (spawned >= maxInstances)
                {
                    Debug.LogWarning($"[GrassSpawnerGridByHeight] Reached maxInstances={maxInstances}. Stop.");
                    Debug.Log($"[GrassSpawnerGridByHeight] Spawned: {spawned}");
                    return;
                }

                // 抖动，让分布不那么“棋盘”
                float jx = (jitter > 0f) ? Random.Range(-jitter, jitter) : 0f;
                float jz = (jitter > 0f) ? Random.Range(-jitter, jitter) : 0f;

                float localX = Mathf.Clamp(x + jx, 0f, size.x);
                float localZ = Mathf.Clamp(z + jz, 0f, size.z);

                float worldX = tPos.x + localX;
                float worldZ = tPos.z + localZ;

                // 可选：噪声遮罩（让草有“块状自然边缘”，否则就是完全铺满）
                if (useNoiseMask)
                {
                    float n = Mathf.PerlinNoise(worldX * noiseScale, worldZ * noiseScale);
                    if (n < noiseThreshold) continue;
                }

                // 采样高度（世界 Y）
                float worldY = terrain.SampleHeight(new Vector3(worldX, 0f, worldZ)) + tPos.y;
                if (worldY < minWorldHeight || worldY > maxWorldHeight) continue;

                // 坡度过滤
                Vector3 normal = td.GetInterpolatedNormal(localX / size.x, localZ / size.z);
                float slopeDeg = Vector3.Angle(normal, Vector3.up);
                if (slopeDeg > maxSlopeDeg) continue;

                // 避障：Obstacle 附近不生成（Terrain 也在 Obstacle 没问题：会忽略 TerrainCollider）
                if (excludeRadius > 0.001f && IsNearObstacle(worldX, worldY, worldZ, qti))
                    continue;

                // 生成
                Vector3 pos = new Vector3(worldX, worldY, worldZ);

                Quaternion yaw = Quaternion.Euler(0f, Random.Range(randomYawDeg.x, randomYawDeg.y), 0f);
                Quaternion rot = alignToNormal ? (Quaternion.FromToRotation(Vector3.up, normal) * yaw) : yaw;

                GameObject go = Instantiate(grassPrefab, pos, rot, parent);

                float s = Random.Range(randomScale.x, randomScale.y);
                go.transform.localScale *= s;

                spawned++;
            }
        }

        Debug.Log($"[GrassSpawnerGridByHeight] Spawned: {spawned} (spacing={spacing}, height={minWorldHeight}-{maxWorldHeight})");
    }

    private bool IsNearObstacle(float worldX, float worldY, float worldZ, QueryTriggerInteraction qti)
    {
        // 探测点抬高：减少和地面的“无意义接触”
        Vector3 probePos = new Vector3(worldX, worldY + probeHeight, worldZ);

        // 用 NonAlloc 减少 GC（比 OverlapSphere 更稳）
        int hitCount = Physics.OverlapSphereNonAlloc(probePos, excludeRadius, _overlapBuffer, obstacleMask, qti);

        if (hitCount <= 0) return false;

        for (int i = 0; i < hitCount; i++)
        {
            Collider c = _overlapBuffer[i];
            if (c == null) continue;

            // 关键：Terrain 在 Obstacle 图层时，必须忽略它，否则会全被判定为“附近有障碍”
            if (c is TerrainCollider)
                continue;
            return true;
        }

        return false;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (excludeRadius <= 0.001f) return;
        Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * probeHeight, excludeRadius);
    }
#endif
}
