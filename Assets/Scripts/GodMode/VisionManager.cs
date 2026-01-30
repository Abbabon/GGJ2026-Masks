using UnityEngine;

namespace GodMode
{
    /// <summary>
    /// Manages the local player's view:
    /// - Toggles between God and Heretic roles (Camera & Masks).
    /// - Ensures appropriate visibility masks are enabled.
    /// </summary>
    public class VisionManager : MonoBehaviour
    {
        [Header("Grouping")]
        [Tooltip("Parent object for all characters (Heretic, NPCs, etc.) to toggle visibility.")]
        public GameObject CharactersRoot;

        [Header("Roles")]
        public GameObject GodRoot;
        public GameObject HereticRoot;
        public GameObject WorldOverlay;

        [Header("Camera")]
        public Camera MainCamera;
        [SerializeField] private HereticCamera _hereticCamEffect;

        [Tooltip("Position of the camera when in God View.")]
        public Vector3 GodCameraPosition = new Vector3(0, 0, -10f);
        [Tooltip("Orthographic Size of the camera when in God View.")]
        public float GodCameraSize = 10f;

        [Header("State")]
        [SerializeField] private bool _isGodMode = true;
        public bool IsGodMode
        {
            get => _isGodMode;
            set => _isGodMode = value;
        }

        private void Start()
        {
            if (MainCamera == null) MainCamera = Camera.main;
            
            // Auto-configure Heretic Camera Component
            if (MainCamera != null)
            {
                if (_hereticCamEffect == null) _hereticCamEffect = MainCamera.GetComponent<HereticCamera>();
                if (_hereticCamEffect == null) _hereticCamEffect = MainCamera.gameObject.AddComponent<HereticCamera>();
            }

            if (CharactersRoot == null) Debug.LogError("VisionManager: CharactersRoot is not assigned!");

            InitMode();
        }

        public void ToggleRole()
        {
            IsGodMode = !IsGodMode;
            InitMode();
        }

        /// <summary>
        /// Applies the current Game Mode state to all systems (Visuals, Camera, Controls).
        /// </summary>
        private void InitMode()
        {
            Debug.Log($"[VisionManager] InitMode: {(_isGodMode ? "GOD" : "HERETIC")}");

            // 1. Controls
            var godCursor = GodRoot ? GodRoot.GetComponent<GodCursor>() : null;
            var hereticMove = HereticRoot ? HereticRoot.GetComponent<SimpleMovement>() : null;

            if (godCursor) godCursor.enabled = _isGodMode;
            if (hereticMove) hereticMove.enabled = !_isGodMode;

            // 2. Character Visibility (Mask Interaction)
            if (CharactersRoot)
            {
                var renderers = CharactersRoot.GetComponentsInChildren<SpriteRenderer>();
                foreach (var r in renderers)
                {
                    r.maskInteraction = _isGodMode 
                        ? SpriteMaskInteraction.VisibleInsideMask 
                        : SpriteMaskInteraction.None;
                }
            }

            // 3. God Mask
            var godMask = GodRoot ? GodRoot.GetComponentInChildren<SpriteMask>() : null;
            if (godMask) godMask.enabled = _isGodMode;

            // 4. World Overlay (Darkness)
            if (WorldOverlay)
            {
                WorldOverlay.SetActive(_isGodMode);
            }

            // 5. Camera Behavior
            // We assume ONE Camera. We toggle the "HereticCamera" script (Follow behavior).
            if (_isGodMode)
            {
                // GOD: Static / Zoomed Out
                if (_hereticCamEffect) _hereticCamEffect.enabled = false;

                if (MainCamera)
                {
                    MainCamera.transform.position = GodCameraPosition;
                    MainCamera.orthographicSize = GodCameraSize; // God View Zoom
                }
            }
            else
            {
                // HERETIC: Follow Target
                if (_hereticCamEffect)
                {
                    _hereticCamEffect.enabled = true;
                    if (HereticRoot) _hereticCamEffect.Target = HereticRoot.transform;
                }
            }
        }
    }
}
