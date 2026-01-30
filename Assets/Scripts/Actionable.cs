using UnityEngine;
using UnityEngine.Events;

public class Actionable : MonoBehaviour
{
    [Header("Action (hold to complete)")]
    [SerializeField] float actionDurationSeconds = 1f;
    [SerializeField] UnityEvent onActionComplete;

    bool isActionActive;
    float actionTimer;
    bool actionCompletedThisHold;

    public float ActionDurationSeconds
    {
        get => actionDurationSeconds;
        set => actionDurationSeconds = value;
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

    void Update()
    {
        if (isActionActive)
        {
            actionTimer += Time.deltaTime;
            if (actionTimer >= actionDurationSeconds && !actionCompletedThisHold)
            {
                actionCompletedThisHold = true;
                onActionComplete?.Invoke();
            }
        }
    }
}
