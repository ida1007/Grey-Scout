using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    [Header("Refs")]
    public Volume globalVolume;

    [Header("Grayscale Settings")]
    public float grayscaleDuration = 0.6f; // 变黑白所需时间（秒）

    private ColorAdjustments colorAdjustments;
    private bool isGameOver = false;

    private void Awake()
    {
        Instance = this;

        if (globalVolume != null)
        {
            globalVolume.profile.TryGet(out colorAdjustments);
            if (colorAdjustments != null)
            {
                colorAdjustments.saturation.value = 0f;
            }
        }
    }

    private void Update()
    {
        if (!isGameOver) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Restart();
        }
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        // 用 Coroutine + unscaled time 做渐变
        StartCoroutine(GrayscaleLerp());

        // 游戏立刻停
        Time.timeScale = 0f;
    }

    private IEnumerator GrayscaleLerp()
    {
        if (colorAdjustments == null)
            yield break;

        float start = colorAdjustments.saturation.value; // 一般是 0
        float target = -100f;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / grayscaleDuration;
            colorAdjustments.saturation.value = Mathf.Lerp(start, target, t);
            yield return null;
        }

        colorAdjustments.saturation.value = target;
    }

    private void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
