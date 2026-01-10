using UnityEngine;

public class HostageInteractable : InteractableBase
{
    public HostageFollowNew hostage;

    void Awake()
    {
        if (hostage == null) hostage = GetComponent<HostageFollowNew>();
        promptText = "Press E to Rescue";
    }

    public override void Interact(PlayerInteractor player)
    {
        if (hostage == null) return;
        if (hostage.isHostageFollowing) return;

        hostage.followTarget = HostageManager.Instance.GetLastHostage();

        if (hostage.followTarget != null) hostage.lastTargetPos = hostage.followTarget.position;

        hostage.isHostageFollowing = true;


        HostageManager.Instance.AddRescuedHostage(hostage.gameObject);
    }
}
