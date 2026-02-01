using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Masks
{
    public class OnboardingUI : MonoBehaviour
    {
        [Serializable]
        public class OnboardingScreen
        {
            public CanvasGroup canvasGroup;
            public Button nextButton;
            [Tooltip("Leave empty for the first screen.")]
            public Button backButton;
        }

        [Header("Screens")]
        [SerializeField] OnboardingScreen[] screens;

        [Header("Transition")]
        [SerializeField] float fadeDuration = 0.3f;

        [Header("Events")]
        [Tooltip("Invoked when the player finishes all onboarding screens.")]
        public UnityEngine.Events.UnityEvent onOnboardingComplete;

        int _currentIndex;
        CancellationTokenSource _transitionCts;

        void Awake()
        {
            for (int i = 0; i < screens.Length; i++)
                SetScreenActive(screens[i], i == 0);

            _currentIndex = 0;
            RefreshScreen(0);
        }

        void OnEnable()
        {
            for (int i = 0; i < screens.Length; i++)
            {
                int index = i;
                var screen = screens[i];
                if (screen.nextButton != null)
                    screen.nextButton.onClick.AddListener(() => OnNextClicked(index));
                if (screen.backButton != null)
                    screen.backButton.onClick.AddListener(() => OnBackClicked(index));
            }
        }

        void OnDisable()
        {
            for (int i = 0; i < screens.Length; i++)
            {
                var screen = screens[i];
                if (screen.nextButton != null)
                    screen.nextButton.onClick.RemoveAllListeners();
                if (screen.backButton != null)
                    screen.backButton.onClick.RemoveAllListeners();
            }

            CancelTransition();
        }

        void OnNextClicked(int fromIndex)
        {
            if (fromIndex != _currentIndex) return;

            if (_currentIndex < screens.Length - 1)
                TransitionTo(_currentIndex + 1).Forget();
            else
                FinishOnboarding();
        }

        void OnBackClicked(int fromIndex)
        {
            if (fromIndex != _currentIndex) return;

            if (_currentIndex > 0)
                TransitionTo(_currentIndex - 1).Forget();
        }

        async UniTaskVoid TransitionTo(int targetIndex)
        {
            CancelTransition();
            _transitionCts = new CancellationTokenSource();
            var ct = _transitionCts.Token;

            SetNavigationInteractable(_currentIndex, false);

            var from = screens[_currentIndex];
            var to = screens[targetIndex];

            to.canvasGroup.gameObject.SetActive(true);
            to.canvasGroup.alpha = 0f;

            await FadeCanvasGroup(from.canvasGroup, 1f, 0f, ct);
            SetScreenActive(from, false);

            _currentIndex = targetIndex;
            RefreshScreen(targetIndex);

            await FadeCanvasGroup(to.canvasGroup, 0f, 1f, ct);
            SetScreenActive(to, true);

            SetNavigationInteractable(targetIndex, true);
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

        void SetScreenActive(OnboardingScreen screen, bool active)
        {
            screen.canvasGroup.gameObject.SetActive(active);
            screen.canvasGroup.alpha = active ? 1f : 0f;
            screen.canvasGroup.interactable = active;
            screen.canvasGroup.blocksRaycasts = active;
        }

        void RefreshScreen(int index)
        {
            var screen = screens[index];

            if (screen.backButton != null)
                screen.backButton.gameObject.SetActive(index > 0);

        }

        void SetNavigationInteractable(int index, bool interactable)
        {
            var screen = screens[index];
            if (screen.nextButton != null)
                screen.nextButton.interactable = interactable;
            if (screen.backButton != null)
                screen.backButton.interactable = interactable;
        }

        void CancelTransition()
        {
            _transitionCts?.Cancel();
            _transitionCts?.Dispose();
            _transitionCts = null;
        }

        void FinishOnboarding()
        {
            onOnboardingComplete?.Invoke();
            SceneManager.LoadScene("local");
        }
    }
}
