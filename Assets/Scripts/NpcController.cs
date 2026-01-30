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

    Mover mover;
    float moveTimer;
    float idleTimer;
    bool isIdle;
    Vector2 lastMoveDirection;
    Vector2? excludedDirection;

    void Start()
    {
        mover = GetComponent<Mover>();
        if (mover == null)
            mover = GetComponentInChildren<Mover>();

        if (mover == null) return;

        float baseSpeed = mover.Speed;
        mover.Speed = baseSpeed + Random.Range(-speedVariance, speedVariance);

        StartIdle();
    }

    void Update()
    {
        if (mover == null) return;

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
                mover.Stop();
                StartIdle();
            }
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (mover == null) return;

        mover.Stop();
        excludedDirection = lastMoveDirection;
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
        if (mover == null) return;

        isIdle = false;
        Vector2 dir = PickDirectionExcluding(excludedDirection);
        excludedDirection = null;
        lastMoveDirection = dir;
        mover.Move(dir);

        float duration = moveDurationSeconds + Random.Range(-moveDurationVariance, moveDurationVariance);
        moveTimer = Mathf.Max(0.01f, duration);
    }

    // 8 directions at 45° intervals (right, up-right, up, up-left, left, down-left, down, down-right)
    static readonly Vector2[] Directions45 = new Vector2[]
    {
        new Vector2(1f, 0f), new Vector2(0.707f, 0.707f), new Vector2(0f, 1f), new Vector2(-0.707f, 0.707f),
        new Vector2(-1f, 0f), new Vector2(-0.707f, -0.707f), new Vector2(0f, -1f), new Vector2(0.707f, -0.707f)
    };

    Vector2 PickDirectionExcluding(Vector2? exclude)
    {
        if (exclude == null)
            return Directions45[Random.Range(0, Directions45.Length)];

        Vector2 ex = exclude.Value.normalized;
        int excludeIndex = 0;
        float bestDot = Vector2.Dot(Directions45[0], ex);
        for (int i = 1; i < Directions45.Length; i++)
        {
            float d = Vector2.Dot(Directions45[i], ex);
            if (d > bestDot) { bestDot = d; excludeIndex = i; }
        }

        int count = Directions45.Length - 1;
        int pick = Random.Range(0, count);
        for (int i = 0; i < Directions45.Length; i++)
        {
            if (i == excludeIndex) continue;
            if (pick == 0) return Directions45[i];
            pick--;
        }
        return Directions45[0];
    }
}
