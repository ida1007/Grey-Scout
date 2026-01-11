using System.Collections.Generic;
using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    public AudioSource audioSource;   
    public List<AudioClip> deathClips;
    public float deathVolume = 1f;

    public GameObject DeathUI;
    public bool IsDead { get; private set; }

    void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (DeathUI == null)
            DeathUI = GameObject.Find("PressEUI");

        if (DeathUI) DeathUI.SetActive(false);
    }

    public void Die()
    {
        if (IsDead) return;
        IsDead = true;

        Debug.Log("Player Dead");

        PlayDeathSound();

        if (DeathUI) DeathUI.SetActive(true);

        if (GameOverManager.Instance != null)
            GameOverManager.Instance.TriggerGameOver();

        Time.timeScale = 0f;
    }

    public void HideRestartUI()
    {
        if (DeathUI) DeathUI.SetActive(false);
    }

    void PlayDeathSound()
    {
        if (audioSource == null) return;
        if (deathClips == null || deathClips.Count == 0) return;

        AudioClip clip = deathClips[Random.Range(0, deathClips.Count)];

        audioSource.pitch = Random.Range(0.95f, 1.05f); 
        audioSource.PlayOneShot(clip, deathVolume);
    }
}
