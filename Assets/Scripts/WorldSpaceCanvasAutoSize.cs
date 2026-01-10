using UnityEngine;

[ExecuteAlways]
public class ConstantScreenSize : MonoBehaviour
{
    [Header("Camera")]
    [Tooltip("自动查找名称为 UICamera 的相机")]
    public string uiCameraName = "UICamera";
    private Camera uiCamera;

    [Header("Screen Size")]
    [Tooltip("希望物体在屏幕上看起来的高度（像素）")]
    public float targetPixelHeight = 80f;

    [Tooltip("物体在 localScale = 1 时的世界高度")]
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

        // 使用投影距离，避免斜向误差
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
            dir.y = 0f; // 如果你希望上下也对准相机，删掉这行
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        }
    }
}
