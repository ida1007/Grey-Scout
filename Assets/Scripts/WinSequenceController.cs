using UnityEngine;

public class WinSequenceController : MonoBehaviour
{
    public MonoBehaviour[] sequenceParts;
    void Awake()
    {
        foreach (var part in sequenceParts)
            if (part) part.enabled = false;
    }

    public void Play()
    {
        foreach (var part in sequenceParts)
            part.enabled = true;
    }
}
