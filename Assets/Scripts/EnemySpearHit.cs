using UnityEngine;

public class SpearHit : MonoBehaviour
{
    private bool hasHit = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        var playerDeath = other.GetComponentInParent<PlayerDeath>();
        if (playerDeath == null) return;

        if (!playerDeath.IsDead)
        {
            hasHit = true;
            playerDeath.Die();
        }
    }
}
