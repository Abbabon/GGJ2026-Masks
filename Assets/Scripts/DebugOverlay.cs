using Fusion;
using TMPro;
using UnityEngine;

/// <summary>
/// Populates a TextMeshProUGUI element with live debug info.
/// Auto-disables itself in non-development builds.
///
/// Setup: Attach to a Canvas GameObject and assign the TMP text element.
/// </summary>
public class DebugOverlay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private float _updateInterval = 0.25f;

    private float _timer;
    private int _frameCount;
    private float _fpsAccumulator;

    private void Awake()
    {
        if (!Debug.isDebugBuild)
        {
            gameObject.SetActive(false);
            return;
        }
    }

    private void Update()
    {
        if (_text == null) return;

        _frameCount++;
        _fpsAccumulator += Time.unscaledDeltaTime;
        _timer += Time.unscaledDeltaTime;

        if (_timer < _updateInterval) return;

        float fps = _frameCount / _fpsAccumulator;
        _frameCount = 0;
        _fpsAccumulator = 0f;
        _timer = 0f;

        _text.text = BuildText(fps);
    }

    private string BuildText(float fps)
    {
        var sb = new System.Text.StringBuilder(256);
        sb.AppendLine($"FPS: {fps:F0}");

        var runner = NetworkRunner.Instances.Count > 0
            ? NetworkRunner.Instances[0]
            : null;

        if (runner == null || !runner.IsRunning)
        {
            sb.AppendLine("Network: Offline");
            return sb.ToString();
        }

        sb.AppendLine($"Session: {runner.SessionInfo.Name}");
        sb.AppendLine($"Players: {runner.SessionInfo.PlayerCount}/{runner.SessionInfo.MaxPlayers}");
        sb.AppendLine($"Region: {runner.SessionInfo.Region}");

        if (runner.TryGetPlayerObject(runner.LocalPlayer, out var localObj))
        {
            var pos = localObj.transform.position;
            sb.AppendLine($"Pos: ({pos.x:F1}, {pos.y:F1})");
        }

        return sb.ToString();
    }
}
