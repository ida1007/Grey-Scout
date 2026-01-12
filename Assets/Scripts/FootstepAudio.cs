using UnityEngine;

public class FootstepAudio : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource source;

    [Header("Clips - Default")]
    public AudioClip[] walkClips;
    public AudioClip[] runClips;
    public AudioClip[] crouchClips;

    [Header("Volume by State")]
    [Range(0f, 1f)] public float walkVol = 0.45f;
    [Range(0f, 1f)] public float runVol = 0.75f;
    [Range(0f, 1f)] public float crouchVol = 0.25f;

    [Header("Pitch Random")]
    public Vector2 pitchRange = new Vector2(0.92f, 1.08f);

    [Header("Anti-Spam")]
    public float minInterval = 0.10f; // Avoid repeat
    private float lastTime = -999f;

    [Header("Surface Detect (Optional)")]
    public bool useSurface = true;
    public Transform rayOrigin;           
    public float rayDistance = 1.5f;
    public LayerMask groundMask = ~0;

    [System.Serializable]
    public class SurfaceSet
    {
        public string surfaceTag;         // Groud Tag
        public AudioClip[] walk;
        public AudioClip[] run;
        public AudioClip[] crouch;
    }
    public SurfaceSet[] surfaces;

    public void PlayStep(PlayerController.PlayerMoveState state)
    {
        if (source == null) return;
        if (Time.time - lastTime < minInterval) return;
        lastTime = Time.time;

        AudioClip[] pool = GetClipsByState(state);

        if (pool == null || pool.Length == 0) return;

        // random clip and pitch
        var clip = pool[Random.Range(0, pool.Length)];
        source.pitch = Random.Range(pitchRange.x, pitchRange.y);

        // diff volume
        float vol =
            state == PlayerController.PlayerMoveState.Run ? runVol :
            state == PlayerController.PlayerMoveState.CrouchWalk ? crouchVol :
            walkVol;

        source.PlayOneShot(clip, vol);
    }

    AudioClip[] GetClipsByState(PlayerController.PlayerMoveState state)
    {
        // surface 
        if (useSurface)
        {
            string tag = DetectSurfaceTag();
            if (!string.IsNullOrEmpty(tag))
            {
                for (int i = 0; i < surfaces.Length; i++)
                {
                    if (surfaces[i] != null && surfaces[i].surfaceTag == tag)
                    {
                        return state == PlayerController.PlayerMoveState.Run ? surfaces[i].run :
                               state == PlayerController.PlayerMoveState.CrouchWalk ? surfaces[i].crouch :
                               surfaces[i].walk;
                    }
                }
            }
        }

        // fallback Default choise
        return state == PlayerController.PlayerMoveState.Run ? runClips :
               state == PlayerController.PlayerMoveState.CrouchWalk ? crouchClips :
               walkClips;
    }

    string DetectSurfaceTag()
    {
        Transform o = rayOrigin != null ? rayOrigin : transform;
        Ray ray = new Ray(o.position + Vector3.up * 0.2f, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            return hit.collider.tag; 
        }
        return null;
    }
}
