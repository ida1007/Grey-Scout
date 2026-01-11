using UnityEngine;
using System;

public class PlayerDuck : MonoBehaviour
{
    public static event Action<Vector3> OnDuck;

    [Header("Input")]
    public KeyCode duckKey = KeyCode.F;

    [Header("Duck Sound")]
    public AudioSource audioSource;
    public AudioClip duckClip;

    [Header("Duck Settings")]
    public float cooldown = 0.6f;

    private float cdTimer = 0f;

    void Update()
    {
        if (cdTimer > 0f) cdTimer -= Time.deltaTime;

        if (Input.GetKeyDown(duckKey) && cdTimer <= 0f)
        {
            cdTimer = cooldown;

            if (audioSource != null && duckClip != null)
                audioSource.PlayOneShot(duckClip);

            OnDuck?.Invoke(transform.position); 
        }
    }
}
