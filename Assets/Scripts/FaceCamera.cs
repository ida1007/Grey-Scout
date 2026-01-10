using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null) return;
        }

        Vector3 dir = transform.position - cam.transform.position;
        dir.y = 0;
        transform.rotation = Quaternion.LookRotation(dir);
    }
}