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
                hostage.isHostageFollowing = !hostage.isHostageFollowing;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
