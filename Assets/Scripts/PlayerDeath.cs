using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    public bool IsDead { get; private set; }

    public void Die()
    {
        if (IsDead) return;
        IsDead = true;

        Debug.Log("Player Dead");

        if (GameOverManager.Instance != null)
            GameOverManager.Instance.TriggerGameOver();

        Time.timeScale = 0f;
    }
}
