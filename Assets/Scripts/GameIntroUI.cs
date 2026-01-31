using System;
using System.Threading;
using Cysharp.Threading.Tasks;
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
        [Tooltip("Activate the player GameObject when this screen becomes active.")]
        public bool spawnPlayer;
        [Tooltip("Activate the NPC spawner when this screen becomes active.")]
        public bool spawnNPCs;
    }

    [Header("Settings")]
    [Tooltip("If false, skip the intro and leave everything enabled immediately.")]
    [SerializeField] bool shouldOnboard = true;

    [Header("Screens")]
    [SerializeField] IntroScreen[] screens;

    [Header("Input References")]
    [SerializeField] Local_Cursor godCursor;
    [SerializeField] Local_Player hereticPlayer;

    [Header("Spawning")]
    [Tooltip("Player to enable when the relevant screen is reached.")]
    [SerializeField] Local_Player playerObject;
    [Tooltip("NPC spawner to enable when the relevant screen is reached.")]
    [SerializeField] Local_Spawner npcSpawner;

    [Header("Game")]
    [SerializeField] Local_Game_manager gameManager;

    [Header("Transition")]
    [SerializeField] float fadeDuration = 0.3f;

    [Header("Events")]
    [Tooltip("Invoked when the intro sequence finishes.")]
    public UnityEvent onIntroComplete;

    CancellationTokenSource _cts;

    void OnEnable()
    {
        Cursor.visible = false;

        if (!shouldOnboard)
        {
            EnableAll();
            gameObject.SetActive(false);
            onIntroComplete?.Invoke();
            return;
        }

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
        if (hereticPlayer) hereticPlayer.enabled = false;
    }

    /// <summary>Enables all input and ensures the player and god cursor are active.</summary>
    void EnableAll()
    {
        if (godCursor) godCursor.enabled = true;
        if (hereticPlayer) hereticPlayer.enabled = true;
        if (playerObject) playerObject.gameObject.SetActive(true);
        if (npcSpawner) npcSpawner.enabled = true;
        if (gameManager) gameManager.enabled = true;
    }

    async UniTaskVoid RunIntro(CancellationToken ct)
    {
        DisableAllInput();
        if (playerObject) playerObject.gameObject.SetActive(false);
        if (npcSpawner) npcSpawner.enabled = false;
        if (gameManager) gameManager.enabled = false;

        for (int i = 0; i < screens.Length; i++)
            SetScreenActive(screens[i], false);

        for (int i = 0; i < screens.Length; i++)
        {
            var screen = screens[i];

            // Apply input state for this screen
            if (godCursor) godCursor.enabled = screen.enableGodInput;
            if (hereticPlayer) hereticPlayer.enabled = screen.enableHereticInput;

            // Enable player if marked
            if (screen.spawnPlayer && playerObject && !playerObject.gameObject.activeSelf)
                playerObject.gameObject.SetActive(true);

            // Enable NPC spawner if marked (Local_Spawner.Start fires when first enabled)
            if (screen.spawnNPCs)
            {
                if (npcSpawner && !npcSpawner.enabled)
                    npcSpawner.enabled = true;

                // Let the heretic move once NPCs are in the world
                if (hereticPlayer) hereticPlayer.enabled = true;
            }

            // Fade in
            screen.canvasGroup.gameObject.SetActive(true);
            await FadeCanvasGroup(screen.canvasGroup, 0f, 1f, ct);
            SetScreenActive(screen, true);

            // Wait for space press to advance
            await UniTask.WaitWhile(() => Input.GetKey(KeyCode.Space), PlayerLoopTiming.Update, ct);
            await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space), PlayerLoopTiming.Update, ct);

            // Fade out
            await FadeCanvasGroup(screen.canvasGroup, 1f, 0f, ct);
            SetScreenActive(screen, false);
        }

        EnableAll();
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
