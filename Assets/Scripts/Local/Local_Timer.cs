using UnityEngine;
using System;
using TMPro;

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
    public float warningTime = 2f;
    public float waitDuration = 3f;

    [Header("Animations")]
    [SerializeField] Animation _animation;
    [SerializeField] string idleClipName = "Timer_Idle";
    [SerializeField] string warningClipName = "Timer_Warning";

    public TimerState CurrentState { get; private set; } = TimerState.Idle;

    public event Action<TimerState> OnStateChanged;

    private float _timer;
    private float _currentDuration;
    private TMP_Text _timerText;

    private void Awake()
    {
        _timerText = GetComponentInChildren<TMP_Text>();
        _animation = GetComponent<Animation>();
    }

    public void StartTimer(float duration)
    {
        _currentDuration = duration;
        CurrentState = TimerState.Counting;
        _timer = duration;
        OnStateChanged?.Invoke(CurrentState);
        PlayAnimation(idleClipName);
    }

    public void StopTimer()
    {
        CurrentState = TimerState.Idle;
        OnStateChanged?.Invoke(CurrentState);
        PlayAnimation(idleClipName);
    }
    
    private void PlayAnimation(string clipName)
    {
        if (_animation != null && !string.IsNullOrEmpty(clipName))
        {
            _animation.Play(clipName);
        }
    }

    void Update()
    {
        if (CurrentState == TimerState.Idle) return;

        if (_timerText != null)
        {
            if (CurrentState == TimerState.Waiting)
            {
                 _timerText.text = "00s";
            }
            else
            {
                 _timerText.text = Mathf.CeilToInt(_timer).ToString("00") + "s";
            }
        }

        if (CurrentState == TimerState.Counting)
        {
            _timer -= Time.deltaTime;
            if (_timer <= warningTime)
            {
                CurrentState = TimerState.Warning;
                OnStateChanged?.Invoke(CurrentState);
                PlayAnimation(warningClipName);
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
                PlayAnimation(idleClipName);
            }
        }
        else if (CurrentState == TimerState.Waiting)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                // Restart cycle
                // StartTimer(_currentDuration);
            }
        }
    }
}
