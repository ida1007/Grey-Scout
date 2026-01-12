using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class AudioWind : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource windSource;
    public AudioClip[] windClips;

    [Header("Volume")]
    [Range(0f, 1f)]
    public float volume = 0.6f;

    [Header("Pitch Random")]
    public Vector2 pitchRange = new Vector2(0.9f, 1.1f);
    public Vector2 playInterval = new Vector2(6f, 14f);

    [Header("Control")]
    public bool playOnStart = true;
    public bool stopWhenTimeScaleZero = false;

    Coroutine windRoutine;

    void Awake()
    {
        if (windSource == null)
        {
            windSource = gameObject.AddComponent<AudioSource>();
            windSource.playOnAwake = false;
            windSource.loop = false;
            windSource.spatialBlend = 0f; 
        }
    }

    void Start()
    {
        if (playOnStart)
            StartWind();
    }

    public void StartWind()
    {
        if (windRoutine != null) return;
        windRoutine = StartCoroutine(WindLoop());
    }

    public void StopWind()
    {
        if (windRoutine != null)
        {
            StopCoroutine(windRoutine);
            windRoutine = null;
        }

        if (windSource.isPlaying)
            windSource.Stop();
    }

    IEnumerator WindLoop()
    {
        while (true)
        {
            float wait = Random.Range(playInterval.x, playInterval.y);

            if (stopWhenTimeScaleZero)
            {
                float timer = 0f;
                while (timer < wait)
                {
                    if (Time.timeScale > 0f)
                        timer += Time.unscaledDeltaTime;

                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSecondsRealtime(wait);
            }

            PlayRandomWind();
        }
    }

    void PlayRandomWind()
    {
        if (windClips == null || windClips.Length == 0)
            return;

        windSource.clip = windClips[Random.Range(0, windClips.Length)];
        windSource.volume = volume;
        windSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        windSource.Play();
    }
}
