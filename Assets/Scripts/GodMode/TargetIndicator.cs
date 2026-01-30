using UnityEngine;

namespace GodMode
{
    /// <summary>
    /// A comprehensive indicator that:
    /// 1. Orbits a center object (Source) pointing towards a Target.
    /// 2. Scales based on distance to the Target.
    /// 3. Hides itself if the Target is within a certain range.
    /// </summary>
    public class TargetIndicator : MonoBehaviour
    {
        [Header("References")]
        public Transform Source; // The player/center
        public Transform Target; // The POI

        [Header("Orbit Settings")]
        [Tooltip("Distance from the Source to place this indicator.")]
        public float OrbitRadius = 2.0f;
        [Tooltip("Additional rotation offset (e.g. -90 if sprite points Up but needs to point Right).")]
        public float RotationOffset = 0f;

        [Header("Scale Settings")]
        [Tooltip("Scale when distance is >= MaxDistance (Far).")]
        public float MinScale = 0.5f;
        [Tooltip("Scale when distance is <= MinDistance (Close).")]
        public float MaxScale = 1.5f;
        [Tooltip("Distance at which scaling is at MinScale.")]
        public float ScalingMaxDistance = 10f;
        [Tooltip("Distance at which scaling is at MaxScale.")]
        public float ScalingMinDistance = 0f;

        [Header("Visibility Settings")]
        [Tooltip("If distance to target is less than this, hide the indicator.")]
        public float HideDistance = 3.0f;
        [Tooltip("Visuals to toggle (so we don't disable this script).")]
        public GameObject VisualsRoot; 

        private void LateUpdate()
        {
            if (Source == null || Target == null) return;

            // 1. Calculate Distances and Direction
            Vector3 centerPos = Source.position;
            Vector3 targetPos = Target.position;
            Vector3 diff = targetPos - centerPos;
            
            // Assume 2D XY plane for direction
            Vector2 flatDiff = new Vector2(diff.x, diff.y);
            float dist = flatDiff.magnitude;

            // 2. Handle Visibility
            if (VisualsRoot != null)
            {
                bool shouldShow = dist > HideDistance;
                if (VisualsRoot.activeSelf != shouldShow)
                    VisualsRoot.SetActive(shouldShow);
                
                if (!shouldShow) return; // Optimization: skip the rest if hidden
            }

            Vector2 direction = dist > 0.001f ? flatDiff / dist : Vector2.up;

            // 3. Orbit Position
            Vector3 newPos = centerPos + (Vector3)direction * OrbitRadius;
            newPos.z = transform.position.z; // Maintain Z
            transform.position = newPos;

            // 4. Rotation (LookAt 2D)
            transform.up = direction;
            if (RotationOffset != 0f)
            {
                transform.Rotate(0f, 0f, RotationOffset);
            }

            // 5. Scaling
            // Scale based on distance. 
            // Closer = MaxScale, Farther = MinScale? 
            // Usually indicators might get smaller if far away, OR bigger to be noticed.
            // Following previous DistanceScaler logic: "Scales UP as distance gets smaller (Target gets closer)"
            // dist: 0 -> MaxScale
            // dist: ScalingMaxDistance -> MinScale
            float t = Mathf.InverseLerp(ScalingMaxDistance, ScalingMinDistance, dist); 
            // Note: InverseLerp(10, 0, 5) returns 0.5. 
            // If dist is 0 (Close), t=1. If dist is 10 (Far), t=0.
            
            float scale = Mathf.Lerp(MinScale, MaxScale, t);
            transform.localScale = Vector3.one * scale;

            // Debug
            Debug.DrawLine(centerPos, targetPos, Color.Lerp(Color.red, Color.green, t));
        }
    }
}
