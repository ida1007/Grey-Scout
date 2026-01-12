using UnityEngine;
using UnityEngine.UI;

public class EnemyUI : MonoBehaviour
{
    [Header("UI References")]
    public Canvas headCanvas;      
    public Image filledImage;      

    public EnemyStayTimer stayTimer; 

    [Header("UI Offset")]
    public Vector3 offset = new Vector3(0, 2f, 0); // Canvas Position(compare enemy)

    void LateUpdate()
    {
        if (headCanvas != null)
        {
            headCanvas.transform.position = transform.position + offset; 
            Camera cam = Camera.main;
            if (cam != null)
            {
                headCanvas.transform.LookAt(headCanvas.transform.position + cam.transform.forward);
            }
        }

        if (filledImage != null && stayTimer != null)
        {
            filledImage.fillAmount = stayTimer.alertValue / stayTimer.threshold; // Filled update
        }
    }
}
