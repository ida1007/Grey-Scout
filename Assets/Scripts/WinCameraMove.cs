using UnityEngine;

public class WinCameraMove : MonoBehaviour
{
    public Transform cam;
    public MonoBehaviour cameraFollow; // 拖你的鼠标控制脚本
    public Vector3 moveOffset = new Vector3(0, 6f, -4f);
    public float duration = 6f;

    private Vector3 startPos;
    private Vector3 targetPos;
    private float t;

    void OnEnable()
    {
        // close cam follow
        if (cameraFollow != null)
            cameraFollow.enabled = false;

        // lock mouse
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        startPos = cam.position;
        targetPos = cam.position + moveOffset;
        t = 0f;
    }

    void Update()
    {
        t += Time.deltaTime / duration;
        cam.position = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0, 1, t));
    }

    void OnDisable()
    {
        // return mouse control
        if (cameraFollow != null)
            cameraFollow.enabled = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
