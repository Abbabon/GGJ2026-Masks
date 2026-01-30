using UnityEngine;

namespace GodMode
{
    /// <summary>
    /// Makes this object orbit around a center Transform while pointing at a target Transform.
    /// Useful for directional indicators.
    /// </summary>
    public class OrbitPointer : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The target object to look towards (e.g., the God).")]
        public Transform LookTarget;

        [Header("Settings")]
        [Tooltip("Distance from the center.")]
        [SerializeField] private float _radius = 2.0f;

        [Tooltip("Additional rotation offset in degrees (e.g. -90 if sprite points Up but needs to point Right).")]
        [SerializeField] private float _rotationOffset = 0f;

        private void LateUpdate()
        {
            if (LookTarget == null) return;

            // 1. Calculate Direction
            Vector3 centerPos = transform.position;
            Vector3 targetPos = LookTarget.position;
            
            // Assume 2D plane (XY)
            Vector2 direction = (new Vector2(targetPos.x, targetPos.y) - new Vector2(centerPos.x, centerPos.y)).normalized;
            
            // 2. Set Position
            // Place on ring at Radius distance
            Vector3 newPos = centerPos + (Vector3)direction * _radius;
            newPos.z = transform.position.z; // Maintain Z
            transform.position = newPos;

            // 3. Set Rotation (Point at Target)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle + _rotationOffset);
        }
    }
}
