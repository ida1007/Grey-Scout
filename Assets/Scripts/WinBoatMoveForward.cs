using UnityEngine;

public class WinBoatMoveForward : MonoBehaviour
{
    public float speed = 1.5f;
    public Vector3 direction = -Vector3.forward;

    void Update()
    {
        transform.position += direction.normalized * speed * Time.deltaTime;
    }
}
