using UnityEngine;

public class Npc : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] float speedVariance = 1f;

    [Header("Walk duration")]
    [SerializeField] float moveDurationSeconds = 3f;
    [SerializeField] float moveDurationVariance = 0.5f;

    [Header("Pause before next move")]
    [SerializeField] float idleTimeoutSeconds = 1f;
    [SerializeField] float idleTimeoutVariance = 0.5f;

    Human human;
    float moveTimer;
    float idleTimer;
    bool isIdle;

    static readonly Vector2[] Directions = new Vector2[]
    {
        Vector2.up,
        Vector2.down,
        Vector2.left,
        Vector2.right
    };

    void Start()
    {
        human = GetComponent<Human>();
        if (human == null)
            human = GetComponentInChildren<Human>();

        if (human == null) return;

        float baseSpeed = human.Speed;
        human.Speed = baseSpeed + Random.Range(-speedVariance, speedVariance);

        StartIdle();
    }

    void Update()
    {
        if (human == null) return;

        if (isIdle)
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0f)
                StartRandomMove();
        }
        else
        {
            moveTimer -= Time.deltaTime;
            if (moveTimer <= 0f)
            {
                human.Stop();
                StartIdle();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (human == null) return;

        human.Stop();
        StartIdle();
    }

    void StartIdle()
    {
        isIdle = true;
        float duration = idleTimeoutSeconds + Random.Range(-idleTimeoutVariance, idleTimeoutVariance);
        idleTimer = Mathf.Max(0.01f, duration);
    }

    void StartRandomMove()
    {
        if (human == null) return;

        isIdle = false;
        Vector2 dir = Directions[Random.Range(0, Directions.Length)];
        human.Move(dir);

        float duration = moveDurationSeconds + Random.Range(-moveDurationVariance, moveDurationVariance);
        moveTimer = Mathf.Max(0.01f, duration);
    }
}
