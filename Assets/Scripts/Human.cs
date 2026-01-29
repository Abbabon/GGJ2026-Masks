using UnityEngine;

public class Human : MonoBehaviour
{
    [SerializeField] float defaultSpeed = 5f;
    Vector2 moveDirection;
    bool inMotion;

    public float Speed
    {
        get => defaultSpeed;
        set => defaultSpeed = value;
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

    void Update()
    {
        if (inMotion)
        {
            Vector3 delta = new Vector3(moveDirection.x, moveDirection.y, 0f) * defaultSpeed * Time.deltaTime;
            transform.position += delta;
        }
    }
}
