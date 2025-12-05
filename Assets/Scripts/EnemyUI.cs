using UnityEngine;
using UnityEngine.UI;

public class EnemyUI : MonoBehaviour
{
    [Header("UI References")]
    public Canvas headCanvas;      // 敌人头顶 Canvas（World Space）
    public Image filledImage;      // 圆形进度条（Filled）

    public EnemyStayTimer stayTimer; // 警戒值系统

    [Header("UI Offset")]
    public Vector3 offset = new Vector3(0, 2f, 0); // Canvas相对于敌人头顶偏移

    void LateUpdate()
    {
        if (headCanvas != null)
        {
            headCanvas.transform.position = transform.position + offset; // 固定 Canvas 在敌人头顶
            Camera cam = Camera.main;// 始终面向摄像机
            if (cam != null)
            {
                headCanvas.transform.LookAt(headCanvas.transform.position + cam.transform.forward);
            }
        }

        if (filledImage != null && stayTimer != null)
        {
            filledImage.fillAmount = stayTimer.alertValue / stayTimer.threshold; // 更新圆形填充比例
        }
    }
}
