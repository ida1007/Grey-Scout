using UnityEngine;

public class LeverInteractable : InteractableBase
{
    [Header("Lever Visual")]
    public Transform leverHandle;
    public Vector3 onEuler = new Vector3(-45f, 0, 0);
    public Vector3 offEuler = new Vector3(45f, 0, 0);
    public float leverSpeed = 8f;

    [HideInInspector] public RollUpDoorController door;

    public bool oneShot = true;
    bool isOn = false;

    void Awake()
    {
        if (leverHandle == null) leverHandle = transform;
    }

    void Update()
    {
        Quaternion target = Quaternion.Euler(isOn ? onEuler : offEuler);
        leverHandle.localRotation =
            Quaternion.Slerp(leverHandle.localRotation, target, Time.deltaTime * leverSpeed);
    }

    public override void Interact(PlayerInteractor player)
    {
        if (oneShot && isOn) return;

        isOn = true;
        door?.Open();
    }
}
