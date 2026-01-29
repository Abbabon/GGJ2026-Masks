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
    Vector2 lastMoveDirection;
    Vector2? excludedDirection;

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
        excludedDirection = lastMoveDirection; // don't pick this direction again
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
        Vector2 dir = PickDirectionExcluding(excludedDirection);
        excludedDirection = null;
        lastMoveDirection = dir;
        human.Move(dir);

        float duration = moveDurationSeconds + Random.Range(-moveDurationVariance, moveDurationVariance);
        moveTimer = Mathf.Max(0.01f, duration);
    }

    Vector2 PickDirectionExcluding(Vector2? exclude)
    {
        float angleDeg;
        if (exclude == null)
        {
            angleDeg = Random.Range(0f, 360f);
        }
        else
        {
            Vector2 ex = exclude.Value.normalized;
            float hitAngleDeg = Mathf.Atan2(ex.y, ex.x) * Mathf.Rad2Deg;
            // Pick an angle at least 90° away from the hit direction (go sideways or opposite)
            angleDeg = hitAngleDeg + 90f + Random.Range(0f, 180f);
        }
        float angleRad = angleDeg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)).normalized;
    }
}
