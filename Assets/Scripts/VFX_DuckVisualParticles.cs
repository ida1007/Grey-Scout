using UnityEngine;

public class VFX_DuckVisualParticle : MonoBehaviour
{
    [Header("Life")]
    public float life = 0.35f;

    [Header("Move")]
    public Vector3 velocity;
    public float drag = 0f;

    [Header("Scale")]
    public float startScale = 1f;
    public float peakScale = 1.25f;
    [Range(0.05f, 0.95f)] public float peakTime01 = 0.45f;

    [Header("Billboard / Roll")]
    public bool billboardToCamera = true;
    public float rollDeg = 0f; 

    [Header("Fade")]
    public bool fadeOut = true;

    private float t;
    private Vector3 baseLocalScale;
    private SpriteRenderer sr;
    private Color baseColor;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) baseColor = sr.color;
        baseLocalScale = transform.localScale;
    }

    void LateUpdate()
    {
        // face camera
        if (billboardToCamera && Camera.main != null)
        {
            var cam = Camera.main;

            transform.rotation = Quaternion.LookRotation(cam.transform.forward, cam.transform.up);
            transform.rotation *= Quaternion.AngleAxis(rollDeg, cam.transform.forward);
        }
    }

    void Update()
    {
        t += Time.deltaTime;
        float u = Mathf.Clamp01(t / Mathf.Max(0.0001f, life));

        // Move
        transform.position += velocity * Time.deltaTime;
        if (drag > 0f) velocity = Vector3.Lerp(velocity, Vector3.zero, drag * Time.deltaTime);

        // Scale curve
        float s;
        if (u < peakTime01)
        {
            float a = u / Mathf.Max(0.0001f, peakTime01);
            s = Mathf.Lerp(startScale, peakScale, Ease(a));
        }
        else
        {
            float a = (u - peakTime01) / Mathf.Max(0.0001f, 1f - peakTime01);
            s = Mathf.Lerp(peakScale, 0f, Ease(a));
        }

        transform.localScale = baseLocalScale * s;

        // Fade out
        if (fadeOut && sr != null)
        {
            Color c = baseColor;
            c.a = Mathf.Lerp(baseColor.a, 0f, u);
            sr.color = c;
        }

        if (t >= life)
            Destroy(gameObject);
    }

    static float Ease(float x) => x * x * (3f - 2f * x);
}
