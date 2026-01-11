using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WinWhiteFade : MonoBehaviour
{
    public Volume volume;
    public float fadeDuration = 4f;

    private ColorAdjustments color;
    private float t;

    void Start()
    {
        volume.profile.TryGet(out color);
        t = 0f;
    }

    void Update()
    {
        t += Time.deltaTime / fadeDuration;

        color.postExposure.value = Mathf.Lerp(0f, 2.5f, t);
        color.contrast.value = Mathf.Lerp(0f, -50f, t);
        color.saturation.value = Mathf.Lerp(0f, -100f, t);
    }
}
