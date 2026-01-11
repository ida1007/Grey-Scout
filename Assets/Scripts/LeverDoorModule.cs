using UnityEngine;

public class LeverDoorModule : MonoBehaviour
{
    [Header("Refs")]
    public LeverInteractable lever;
    public RollUpDoorController door;

    void Awake()
    {
        // 自动抓子物体（prefab 内安全）
        if (lever == null)
            lever = GetComponentInChildren<LeverInteractable>();

        if (door == null)
            door = GetComponentInChildren<RollUpDoorController>();

        // 把门注入给拉杆
        if (lever != null)
            lever.door = door;
    }
}