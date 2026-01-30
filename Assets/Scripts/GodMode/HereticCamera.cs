using UnityEngine;

namespace GodMode
{
    /// <summary>
    /// Follows a target with a deadzone and clamps to WorldBounds.
    /// </summary>
    public class HereticCamera : MonoBehaviour
    {
        [Header("Targeting")]
        public Transform Target;
        [Tooltip("The SpriteRenderer that defines the world boundaries.")]
        [SerializeField] private SpriteRenderer _boundarySprite;

        [Header("Settings")]
        [Tooltip("Radius of the deadzone in World Units.")]
        [SerializeField] private float _deadZoneRadius = 2.0f;
        [SerializeField] private float _smoothTime = 0.2f;
        [SerializeField] private float _orthoSize = 5f;

        private Camera _cam;
        private Vector3 _currentVelocity;
        private Vector3 _targetPosition;

        private void Start()
        {
            _cam = GetComponent<Camera>();
            if (_boundarySprite == null) Debug.LogError("HereticCamera: Boundary Sprite is not assigned!");
            _targetPosition = transform.position; // Initialize
        }

        private void LateUpdate()
        {
            if (Target == null || _cam == null) return;
            
            // 1. Calculate Desired Position based on Circular Dead Zone
            Vector3 targetPos = Target.position;
            
            // Use the TARGET position (not current camera position) for deadzone check
            Vector3 targetPos2D = new Vector3(targetPos.x, targetPos.y, 0);
            Vector3 cameraTarget2D = new Vector3(_targetPosition.x, _targetPosition.y, 0);

            float dist = Vector3.Distance(cameraTarget2D, targetPos2D);

            // Only update target position if outside deadzone
            if (dist > _deadZoneRadius)
            {
                Vector3 moveDir = (targetPos2D - cameraTarget2D).normalized;
                float moveDist = dist - _deadZoneRadius;
                
                _targetPosition = cameraTarget2D + moveDir * moveDist;
                _targetPosition.z = -10f;
            }

            // 2. Smooth Move towards the target position
            Vector3 smoothedPos = _targetPosition;//Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime / _smoothTime);

            // 3. Clamp to World Bounds
            if (_boundarySprite != null)
            {
                Bounds b = _boundarySprite.bounds;
                
                float h = _cam.orthographicSize;
                float w = h * _cam.aspect;

                float minX = b.min.x + w;
                float maxX = b.max.x - w;
                
                if (minX > maxX) smoothedPos.x = b.center.x;
                else smoothedPos.x = Mathf.Clamp(smoothedPos.x, minX, maxX);

                float minY = b.min.y + h;
                float maxY = b.max.y - h;
                
                if (minY > maxY) smoothedPos.y = b.center.y;
                else smoothedPos.y = Mathf.Clamp(smoothedPos.y, minY, maxY);
            }

            transform.position = smoothedPos;
            _cam.orthographicSize = _orthoSize;
        }



        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            // Draw Circle at Camera Center
            Gizmos.DrawWireSphere(transform.position, _deadZoneRadius);
        }
    }
}
