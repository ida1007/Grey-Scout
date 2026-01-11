using UnityEngine;

public class WinMusic : MonoBehaviour
{
    public AudioSource audioSource;
    public float delay = 2f;

    void Start()
    {
        Invoke(nameof(Play), delay);
    }

    void Play()
    {
        audioSource.Play();
    }
}
