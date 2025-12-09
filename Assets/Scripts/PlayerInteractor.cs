using UnityEngine;


public class PlayerInteractor : MonoBehaviour
{
    public HostageFollow hostage;
    public float interactRange;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            float distance = Vector3.Distance(transform.position, hostage.transform.position);

            if (distance <= interactRange)
            {
                RescueHostage();
            }
        }
    }

    void RescueHostage()
    {
        if (hostage.isHostageFollowing)
            return;
        hostage.isHostageFollowing = true;
        HostageManager.Instance.hostageNum++;
        HostageManager.Instance.followHostages.Add(hostage.gameObject);
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
