using UnityEngine;

namespace GodMode
{
    /// <summary>
    /// Simulates a "South Park" style cardboard cutout waddle animation by rotating a child sprite 
    /// back and forth when the parent moves.
    /// </summary>
    public class CutoutAnimator : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("The sprite object to animate (rotate).")]
        public Transform SpriteTarget;

        [Header("Settings")]
        [Tooltip("How fast the waddle cycle repeats.")]
        [SerializeField] private float _waddleSpeed = 15f;

        [Tooltip("Maximum rotation angle in degrees.")]
        [SerializeField] private float _waddleAngle = 10f;

        [Tooltip("How fast the sprite returns to upright when stopped.")]
        [SerializeField] private float _returnSpeed = 10f;

        [Tooltip("Minimum movement speed to trigger animation.")]
        [SerializeField] private float _moveThreshold = 0.1f;

        private Vector3 _lastPosition;
        private float _waddleTimer;

        private void Start()
        {
            _lastPosition = transform.position;
            if (SpriteTarget == null)
            {
                // Try to find a child sprite if not assigned
                var sprite = GetComponentInChildren<SpriteRenderer>();
                if (sprite != null && sprite.gameObject != gameObject)
                {
                    SpriteTarget = sprite.transform;
                }
            }
        }

        private void Update()
        {
            if (SpriteTarget == null) return;

            // Calculate movement speed
            Vector3 currentPos = transform.position;
            // Use horizontal plane only or full 3D? Adjust as needed. 
            // Assuming 2D movement for now.
            float moveDist = (currentPos - _lastPosition).magnitude;
            float speed = moveDist / Time.deltaTime;
            
            _lastPosition = currentPos;

            if (speed > _moveThreshold)
            {
                // Moving: Waddle
                _waddleTimer += Time.deltaTime * _waddleSpeed;
                
                float rotationZ = Mathf.Sin(_waddleTimer) * _waddleAngle;
                SpriteTarget.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
            }
            else
            {
                // Stopped: Return to zero
                // Optional: Snap _waddleTimer to nearest zero crossing or just reset?
                // Resetting timer might cause a jump if we start moving again immediately.
                // For "return to zero", we just Lerp the rotation.
                
                SpriteTarget.localRotation = Quaternion.Lerp(
                    SpriteTarget.localRotation, 
                    Quaternion.identity, 
                    Time.deltaTime * _returnSpeed
                );
                
                // Reset timer so we don't start midway through a cycle next time (optional)
                // _waddleTimer = 0f; 
            }
        }
    }
}
