using System.Collections.Generic;
using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    public AudioSource audioSource;   
    public List<AudioClip> deathClips;
    public float deathVolume = 1f;
    public bool IsDead { get; private set; }

    void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void Die()
    {
        if (IsDead) return;
        IsDead = true;

        Debug.Log("Player Dead");

        PlayDeathSound();

        if (GameOverManager.Instance != null)
            GameOverManager.Instance.TriggerGameOver();

        Time.timeScale = 0f;
    }

    void PlayDeathSound()
    {
        if (audioSource == null) return;
        if (deathClips == null || deathClips.Count == 0) return;

        AudioClip clip = deathClips[Random.Range(0, deathClips.Count)];

        audioSource.pitch = Random.Range(0.95f, 1.05f); // ¿ÉÑ¡£ºÎ¢Ëæ»ú
        audioSource.PlayOneShot(clip, deathVolume);
    }
}
