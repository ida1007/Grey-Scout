using UnityEngine;

public class LeverDoorModule : MonoBehaviour
{
    [Header("Refs")]
    public LeverInteractable lever;
    public RollUpDoorController door;

    void Awake()
    {
        // get conponent
        if (lever == null)
            lever = GetComponentInChildren<LeverInteractable>();

        if (door == null)
            door = GetComponentInChildren<RollUpDoorController>();

        if (lever != null)
            lever.door = door;
    }
}