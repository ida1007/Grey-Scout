using UnityEngine;

public class RollUpDoorController : MonoBehaviour
{
    [Header("Move")]
    public Transform door;        
    public float openHeight = 3f;
    public float speed = 3f;

    [Header("State")]
    public bool isOpen = false;
    public bool lockAfterOpen = false;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip bigDoorClip;
    public float bigDoorVolume = 1f;

    Vector3 closedLocalPos;
    Vector3 openLocalPos;

    void Awake()
    {
        if (door == null) door = transform;

        closedLocalPos = door.localPosition;
        openLocalPos = closedLocalPos + Vector3.up * openHeight;
    }

    void Update()
    {
        Vector3 target = isOpen ? openLocalPos : closedLocalPos;
        door.localPosition =
            Vector3.Lerp(door.localPosition, target, Time.deltaTime * speed);
    }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        PlaySfxAtPoint(bigDoorClip, bigDoorVolume); //Audio
    }

    public void Close()
    {
        if (lockAfterOpen && isOpen) return;
        isOpen = false;
    }

    public void Toggle()
    {
        if (lockAfterOpen && isOpen) return;
        isOpen = !isOpen;
    }

    void PlaySfxAtPoint(AudioClip clip, float volume)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, transform.position, volume);
    }
}
