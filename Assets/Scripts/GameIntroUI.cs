using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GodMode;
using UnityEngine;
using UnityEngine.Events;

public class GameIntroUI : MonoBehaviour
{
    [Serializable]
    public class IntroScreen
    {
        public CanvasGroup canvasGroup;
        [Tooltip("Enable god (mouse) input while this screen is active.")]
        public bool enableGodInput;
        [Tooltip("Enable heretic (keyboard) input while this screen is active.")]
        public bool enableHereticInput;
        [Tooltip("Skip waiting for Space and auto-advance after autoAdvanceDelay seconds.")]
        public bool autoAdvance;
        [Tooltip("Seconds before auto-advancing (only used if autoAdvance is true).")]
        public float autoAdvanceDelay = 3f;
    }

    [Header("Screens")]
    [SerializeField] IntroScreen[] screens;

    [Header("Input References")]
    [SerializeField] GodCursor godCursor;
    [SerializeField] SimpleMovement hereticMovement;

    [Header("Transition")]
    [SerializeField] float fadeDuration = 0.3f;

    [Header("Events")]
    [Tooltip("Invoked when the intro sequence finishes.")]
    public UnityEvent onIntroComplete;

    CancellationTokenSource _cts;

    void OnEnable()
    {
        _cts = new CancellationTokenSource();
        RunIntro(_cts.Token).Forget();
    }

    void OnDisable()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    /// <summary>Disables both god (mouse) and heretic (keyboard) input.</summary>
    public void DisableAllInput()
    {
        if (godCursor) godCursor.enabled = false;
        if (hereticMovement) hereticMovement.enabled = false;
    }

    async UniTaskVoid RunIntro(CancellationToken ct)
    {
        DisableAllInput();

        for (int i = 0; i < screens.Length; i++)
            SetScreenActive(screens[i], false);

        for (int i = 0; i < screens.Length; i++)
        {
            var screen = screens[i];

            // Apply input state for this screen
            if (godCursor) godCursor.enabled = screen.enableGodInput;
            if (hereticMovement) hereticMovement.enabled = screen.enableHereticInput;

            // Fade in
            screen.canvasGroup.gameObject.SetActive(true);
            await FadeCanvasGroup(screen.canvasGroup, 0f, 1f, ct);
            SetScreenActive(screen, true);

            // Wait for advance condition
            if (screen.autoAdvance)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(screen.autoAdvanceDelay),
                    DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update, ct);
            }
            else
            {
                // Wait for space to be released first (in case held from previous screen)
                await UniTask.WaitWhile(() => Input.GetKey(KeyCode.Space), PlayerLoopTiming.Update, ct);
                // Then wait for a fresh press
                await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space), PlayerLoopTiming.Update, ct);
            }

            // Fade out
            await FadeCanvasGroup(screen.canvasGroup, 1f, 0f, ct);
            SetScreenActive(screen, false);
        }

        DisableAllInput();
        gameObject.SetActive(false);
        onIntroComplete?.Invoke();
    }

    async UniTask FadeCanvasGroup(CanvasGroup group, float from, float to, CancellationToken ct)
    {
        float elapsed = 0f;
        group.alpha = from;

        while (elapsed < fadeDuration)
        {
            ct.ThrowIfCancellationRequested();
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / fadeDuration));
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        group.alpha = to;
    }

    void SetScreenActive(IntroScreen screen, bool active)
    {
        screen.canvasGroup.gameObject.SetActive(active);
        screen.canvasGroup.alpha = active ? 1f : 0f;
        screen.canvasGroup.interactable = active;
        screen.canvasGroup.blocksRaycasts = active;
    }
}
