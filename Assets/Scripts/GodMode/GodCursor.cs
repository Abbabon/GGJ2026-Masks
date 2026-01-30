using Fusion;
using Network;
using UnityEngine;

namespace GodMode
{
    /// <summary>
    /// Handles the God cursor:
    /// - If local player is God (LobbyState.GodPlayer): drive from mouse and replicate position via the God player's PlayerController.GodCursorWorldPosition.
    /// - Otherwise: read God cursor position from the God player's networked object and drive visuals from that.
    /// </summary>
    public class GodCursor : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The visual that snaps directly to the cursor (e.g., a crosshair).")]
        public Transform CursorVisual;

        [Tooltip("The mask/iris that drags behind.")]
        public Transform IrisVisual;

        [Header("Settings")]
        [Tooltip("Time to reach the target (smooth damp) for iris and for remote cursor.")]
        [SerializeField] private float _smoothTime = 0.3f;

        [Tooltip("Maximum speed of the iris.")]
        [SerializeField] private float _maxSpeed = 20f;

        private Vector3 _currentVelocity;
        private Camera _mainCamera;
        private NetworkRunner _runner;
        private LobbyState _lobbyState;

        private void Start()
        {
            _mainCamera = Camera.main;
            _runner = UnityEngine.Object.FindObjectOfType<NetworkRunner>();
            if (CursorVisual) CursorVisual.position = GetWorldMousePosition();
            if (IrisVisual) IrisVisual.position = GetWorldMousePosition();
        }

        private void Update()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null) return;

            if (_runner == null) _runner = UnityEngine.Object.FindObjectOfType<NetworkRunner>();
            if (_runner == null) return;

            if (_lobbyState == null) _lobbyState = UnityEngine.Object.FindObjectOfType<LobbyState>();
            if (_lobbyState == null || !_lobbyState.Id.IsValid || !_lobbyState.GameStarted) return;

            bool isGod = _lobbyState.GodPlayer == _runner.LocalPlayer;
            Vector3 targetPos;

            if (isGod)
            {
                targetPos = GetWorldMousePosition();
                // Replicate to other clients via our player object (we have state authority).
                if (_runner.TryGetPlayerObject(_runner.LocalPlayer, out var myObject) && myObject.TryGetComponent<PlayerController>(out var pc))
                    pc.GodCursorWorldPosition = targetPos;
            }
            else
            {
                // Read from God player's networked position.
                if (_lobbyState.GodPlayer.IsNone) return;
                if (!_runner.TryGetPlayerObject(_lobbyState.GodPlayer, out var godObject) || !godObject.TryGetComponent<PlayerController>(out var godPc))
                    return;
                targetPos = godPc.GodCursorWorldPosition;
            }

            ApplyVisuals(targetPos);
        }

        private void ApplyVisuals(Vector3 targetPos)
        {
            if (CursorVisual)
                CursorVisual.position = new Vector3(targetPos.x, targetPos.y, CursorVisual.position.z);

            if (IrisVisual)
            {
                float currentZ = IrisVisual.position.z;
                Vector3 currentPos = IrisVisual.position;
                Vector3 targetPos2D = new Vector3(targetPos.x, targetPos.y, currentZ);
                Vector3 newPos = Vector3.SmoothDamp(currentPos, targetPos2D, ref _currentVelocity, _smoothTime, _maxSpeed);
                IrisVisual.position = newPos;
            }
        }

        private Vector3 GetWorldMousePosition()
        {
            Vector3 mouseScreen = Input.mousePosition;
            float distance = -_mainCamera.transform.position.z;
            Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, distance));
            mouseWorld.z = 0f;
            return mouseWorld;
        }
    }
}
