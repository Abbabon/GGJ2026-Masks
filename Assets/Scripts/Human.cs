using UnityEngine;
using UnityEngine.Events;

public class Human : MonoBehaviour
{
    [SerializeField] float defaultSpeed = 5f;
    Vector2 moveDirection;
    bool inMotion;
    Rigidbody2D rb;

    [Header("Action (hold to complete)")]
    [SerializeField] float actionDurationSeconds = 1f;
    [SerializeField] UnityEvent onActionComplete;

    bool isActionActive;
    float actionTimer;
    bool actionCompletedThisHold;

    public float Speed
    {
        get => defaultSpeed;
        set => defaultSpeed = value;
    }

    public float ActionDurationSeconds
    {
        get => actionDurationSeconds;
        set => actionDurationSeconds = value;
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

    /// <summary>Start the action (e.g. button pressed). Hold for ActionDurationSeconds to fire onActionComplete.</summary>
    public void ActionStart()
    {
        isActionActive = true;
        actionCompletedThisHold = false;
    }

    /// <summary>Stop the action (e.g. button released). Resets hold progress.</summary>
    public void ActionStop()
    {
        isActionActive = false;
        actionTimer = 0f;
        actionCompletedThisHold = false;
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

        if (isActionActive)
        {
            actionTimer += Time.deltaTime;
            if (actionTimer >= actionDurationSeconds && !actionCompletedThisHold)
            {
                actionCompletedThisHold = true;
                onActionComplete?.Invoke();
                Debug.Log("action complete");
            }
        }
    }
}
