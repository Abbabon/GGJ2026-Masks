using UnityEngine;

namespace GodMode
{
    /// <summary>
    /// Scales this object based on the distance between a Source and a Target.
    /// Scales UP as distance gets smaller (Target gets closer).
    /// </summary>
    public class DistanceScaler : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The target position (e.g., God).")]
        public Transform Target;

        [Header("Settings")]
        [Tooltip("Scale when distance is >= MaxDistance (Far away).")]
        [SerializeField] private float _minScale = 0.5f;

        [Tooltip("Scale when distance is approx 0 (Very close).")]
        [SerializeField] private float _maxScale = 1.5f;

        [Tooltip("The distance at which scaling starts (Clamped beyond this).")]
        [SerializeField] private float _maxDistance = 10f;

        private void LateUpdate()
        {
            if (Target == null) return;

            float dist = Vector3.Distance(transform.position, Target.position);
            
            // Calculate proximity factor t
            // t = 1 when distance is 0 (Close)
            // t = 0 when distance is MaxDistance (Far)
            float t = 1f - Mathf.Clamp01(dist / _maxDistance);

            float scale = Mathf.Lerp(_minScale, _maxScale, t);

            transform.localScale = Vector3.one * scale;
        }
    }
}
