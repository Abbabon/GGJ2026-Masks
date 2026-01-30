using UnityEngine;

namespace GodMode
{
    /// <summary>
    /// Handles the God character movement:
    /// 1. A direct "Cursor" visual that stays exactly on the mouse.
    /// 2. A "Mask/Iris" visual that smoothly follows the mouse.
    /// </summary>
    public class GodCursor : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The visual that snaps directly to the mouse (e.g., a crosshair).")]
        public Transform CursorVisual;

        [Tooltip("The mask/iris that drags behind deeply.")]
        public Transform IrisVisual;

        [Header("Settings")]
        [Tooltip("Time to reach the target (smooth damp).")]
        [SerializeField] private float _smoothTime = 0.3f; // Slower for "drag" feel

        [Tooltip("Maximum speed of the iris.")]
        [SerializeField] private float _maxSpeed = 20f;

        private Vector3 _currentVelocity;
        private Camera _mainCamera;

        private void Start()
        {
            _mainCamera = Camera.main;
            if (CursorVisual) CursorVisual.position = GetWorldMousePosition();
            if (IrisVisual) IrisVisual.position = GetWorldMousePosition();
        }

        private void Update()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null) return;

            Vector3 targetPos = GetWorldMousePosition();

            // 1. Direct Visual
            if (CursorVisual)
            {
                CursorVisual.position = new Vector3(targetPos.x, targetPos.y, CursorVisual.position.z);
            }

            // 2. Smooth Iris Visual
            if (IrisVisual)
            {
                // We keep z same as original to avoid sorting issues, or ensure it's set correctly
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
            // Distance from camera to z=0 plane.
            // Assuming 2D game at z=0.
            float distance = -_mainCamera.transform.position.z;
            Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, distance));
            mouseWorld.z = 0f; // Force to 2D plane
            return mouseWorld;
        }
    }
}
