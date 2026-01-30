using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Mover : MonoBehaviour
{
    public float Speed = 2f;

    Rigidbody2D rb;
    Vector2 moveDir;
    bool moving;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    public void Move(Vector2 dir) { moveDir = dir.normalized; moving = true; }
    public void Stop() { moving = false; moveDir = Vector2.zero; }

    void FixedUpdate()
    {
        if (!moving) return;
        Vector2 next = rb.position + moveDir * Speed * Time.fixedDeltaTime;
        rb.MovePosition(next);
    }
}