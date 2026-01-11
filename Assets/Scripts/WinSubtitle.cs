using UnityEngine;
using System.Collections;

public class WinSubtitle : MonoBehaviour
{
    public CanvasGroup winPanel;
    public float delay = 4f;
    public float fadeDuration = 1f;

    void Start()
    {
        winPanel.alpha = 0f;
        winPanel.gameObject.SetActive(true);

        StartCoroutine(FadeInAfterDelay());
    }

    IEnumerator FadeInAfterDelay()
    {
        yield return new WaitForSeconds(delay);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            winPanel.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        winPanel.alpha = 1f;
    }
}
