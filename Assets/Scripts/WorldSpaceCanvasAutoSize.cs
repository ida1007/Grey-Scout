using UnityEngine;

[ExecuteAlways]
public class ConstantScreenSize : MonoBehaviour
{
    [Header("Camera")]
    public string uiCameraName = "UICamera";
    private Camera uiCamera;

    [Header("Screen Size")]
    public float targetPixelHeight = 80f;
    public float objectWorldHeightAtScale1 = 1f;

    [Header("Options")]
    public bool billboard = true;
    public bool useLateUpdate = true;

    void OnEnable()
    {
        FindUICamera();
        Refresh();
    }

    void Update()
    {
        if (!useLateUpdate)
            Refresh();
    }

    void LateUpdate()
    {
        if (useLateUpdate)
            Refresh();
    }

    void FindUICamera()
    {
        if (uiCamera != null) return;

        GameObject camObj = GameObject.Find(uiCameraName);
        if (camObj)
            uiCamera = camObj.GetComponent<Camera>();
    }

    void Refresh()
    {
        if (!uiCamera)
        {
            FindUICamera();
            if (!uiCamera) return;
        }

        Vector3 camPos = uiCamera.transform.position;
        Vector3 objPos = transform.position;

        // Utilise the projection distance to avoid 3D dis errors
        float distance = Vector3.Dot(objPos - camPos, uiCamera.transform.forward);
        distance = Mathf.Max(0.01f, distance);

        float worldPerPixel;

        if (uiCamera.orthographic)
        {
            worldPerPixel = (2f * uiCamera.orthographicSize) / Screen.height;
        }
        else
        {
            float halfFovRad = uiCamera.fieldOfView * 0.5f * Mathf.Deg2Rad;
            float screenWorldHeight = 2f * distance * Mathf.Tan(halfFovRad);
            worldPerPixel = screenWorldHeight / Screen.height;
        }

        float desiredWorldHeight = targetPixelHeight * worldPerPixel;
        float scale = desiredWorldHeight / Mathf.Max(0.0001f, objectWorldHeightAtScale1);

        transform.localScale = Vector3.one * scale;

        if (billboard)
        {
            Vector3 dir = objPos - camPos;
            dir.y = 0f; 
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        }
    }
}
