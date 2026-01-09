using UnityEngine;

public class CageDoorOpenInteractable : InteractableBase
{
    public Transform door;
    public float liftHeight = 2f;
    public float moveSpeed = 3f;

    bool opened;
    Vector3 closedLocalPos, openLocalPos;

    void Awake()
    {
        promptText = "Press E to Open";
        if (door == null) door = transform.Find("Door");
        closedLocalPos = door.localPosition;
        openLocalPos = closedLocalPos + Vector3.up * liftHeight;
    }

    void Update()
    {
        if (!opened) return;
        door.localPosition = Vector3.MoveTowards(door.localPosition, openLocalPos, moveSpeed * Time.deltaTime);
    }

    public override void Interact(PlayerInteractor player)
    {
        if (opened) return;
        opened = true;
    }
}
