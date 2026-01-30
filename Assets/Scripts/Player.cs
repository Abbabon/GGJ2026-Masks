using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] KeyCode actionKey = KeyCode.E;
    [Tooltip("Input in this direction is ignored after hitting a trigger until player chooses another direction.")]
    [SerializeField] float blockedDirectionDotThreshold = 0.7f;
    Mover mover;
    Actionable actionable;
    bool actionPressedLastFrame;
    Vector2 lastMoveDirection;
    Vector2? excludedDirection;

    void Start()
    {
        mover = GetComponent<Mover>();
        if (mover == null) mover = GetComponentInChildren<Mover>();

        actionable = GetComponent<Actionable>();
        if (actionable == null) actionable = GetComponentInChildren<Actionable>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (mover == null) return;
        mover.Stop();
        excludedDirection = lastMoveDirection;
    }

    void Update()
    {
        if (mover != null)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector2 dir = new Vector2(h, v);

            if (dir.sqrMagnitude > 0.01f)
            {
                Vector2 normalized = dir.normalized;
                if (excludedDirection.HasValue &&
                    Vector2.Dot(normalized, excludedDirection.Value) >= blockedDirectionDotThreshold)
                {
                    mover.Stop();
                }
                else
                {
                    excludedDirection = null;
                    lastMoveDirection = normalized;
                    mover.Move(normalized);
                }
            }
            else
            {
                mover.Stop();
            }
        }

        if (actionable != null)
        {
            bool actionPressed = Input.GetKey(actionKey);
            if (actionPressed && !actionPressedLastFrame)
                actionable.ActionStart();
            else if (!actionPressed && actionPressedLastFrame)
                actionable.ActionStop();
            actionPressedLastFrame = actionPressed;
        }
    }
}
