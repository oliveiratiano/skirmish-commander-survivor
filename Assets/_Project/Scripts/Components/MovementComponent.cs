using UnityEngine;

public class MovementComponent : MonoBehaviour
{
    public float moveSpeed = 5f;

    public Vector3 Velocity { get; private set; }

    public void Move(Vector3 direction)
    {
        if (direction.sqrMagnitude > 1f)
            direction.Normalize();

        Velocity = direction * moveSpeed;
        transform.position += Velocity * Time.deltaTime;
    }

    public void Stop()
    {
        Velocity = Vector3.zero;
    }
}
