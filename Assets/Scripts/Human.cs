using UnityEngine;

public class Human : MonoBehaviour
{
    [SerializeField] float defaultSpeed = 5f;
    Vector2 moveDirection;
    bool inMotion;
    Rigidbody2D rb;

    public float Speed
    {
        get => defaultSpeed;
        set => defaultSpeed = value;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.bodyType = RigidbodyType2D.Kinematic; // so we control position; physics won't override
    }

    public void Move(Vector2 direction)
    {
        moveDirection = direction.normalized;
        inMotion = true;
    }

    public void Stop()
    {
        inMotion = false;
    }

    public void Action()
    {
        Debug.Log("action");
    }

    void FixedUpdate()
    {
        if (inMotion && rb != null)
        {
            Vector2 delta = moveDirection * defaultSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + delta);
        }
    }

    void Update()
    {
        if (inMotion && rb == null)
        {
            Vector3 delta = new Vector3(moveDirection.x, moveDirection.y, 0f) * defaultSpeed * Time.deltaTime;
            transform.position += delta;
        }
    }
}
