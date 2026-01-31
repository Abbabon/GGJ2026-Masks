using UnityEngine;
using System;

public class Local_Timer : MonoBehaviour
{
    public enum TimerState
    {
        Idle,
        Counting,
        Warning,
        Waiting
    }

    [Header("Settings")]
    public float duration = 5f;
    public float warningTime = 2f;
    public float waitDuration = 3f;

    public TimerState CurrentState { get; private set; } = TimerState.Idle;

    public event Action<TimerState> OnStateChanged;

    private float _timer;

    public void StartTimer()
    {
        CurrentState = TimerState.Counting;
        _timer = duration;
        OnStateChanged?.Invoke(CurrentState);
    }

    public void StopTimer()
    {
        CurrentState = TimerState.Idle;
        OnStateChanged?.Invoke(CurrentState);
    }

    void Update()
    {
        if (CurrentState == TimerState.Idle) return;

        if (CurrentState == TimerState.Counting)
        {
            _timer -= Time.deltaTime;
            if (_timer <= warningTime)
            {
                CurrentState = TimerState.Warning;
                OnStateChanged?.Invoke(CurrentState);
            }
        }
        else if (CurrentState == TimerState.Warning)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                // Go to Waiting, logic should listen to this state change
                CurrentState = TimerState.Waiting;
                _timer = waitDuration;
                OnStateChanged?.Invoke(CurrentState);
            }
        }
        else if (CurrentState == TimerState.Waiting)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                // Restart cycle
                StartTimer();
            }
        }
    }
}
