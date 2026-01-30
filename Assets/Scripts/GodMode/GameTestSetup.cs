using UnityEngine;

namespace GodMode
{
    /// <summary>
    /// Setups a test scene with God and Heretic modes, togglable via 'Q'.
    /// Requires objects to be present in the scene.
    /// </summary>
    public class GameTestSetup : MonoBehaviour
    {
        [Header("Scene References")]
        [Tooltip("Existing God GameObject in the scene")]
        [SerializeField] private GameObject _godObject;
        [Tooltip("Existing Heretic GameObject in the scene")]
        [SerializeField] private GameObject _hereticObject;
        [Tooltip("Existing World/Overlay GameObject in the scene")]
        [SerializeField] private GameObject _worldOverlayObject;
        
        [Header("Managers")]
        [Tooltip("Existing VisionManager in the scene")]
        [SerializeField] private VisionManager _visionManager;

        private Camera _mainCamera;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q) && _visionManager != null)
            {
                _visionManager.ToggleRole();
            }
        }

        private void Start()
        {
            _mainCamera = Camera.main;
            
            // Auto-find references if missing
            if (_godObject == null) _godObject = GameObject.Find("GodCharacter");
            if (_hereticObject == null) _hereticObject = GameObject.Find("HereticCharacter");
            if (_worldOverlayObject == null) _worldOverlayObject = GameObject.Find("WorldOverlay");
            if (_visionManager == null) _visionManager = FindAnyObjectByType<VisionManager>();

            // Setup Vision Manager
            if (_visionManager != null)
            {
                _visionManager.GodRoot = _godObject;
                _visionManager.HereticRoot = _hereticObject;
                
                GameObject overlayObj = null;
                // check if the user assigned the ROOT GameWorld instead of the Overlay
                var renderers = _worldOverlayObject.GetComponentsInChildren<SpriteRenderer>();

                if (renderers.Length > 1) 
                {
                    // It's likely a container. Find the specific overlay child.
                    foreach(var r in renderers)
                    {
                        if (r.maskInteraction == SpriteMaskInteraction.VisibleOutsideMask && r.gameObject != _worldOverlayObject)
                        {
                            overlayObj = r.gameObject;
                            break;
                        }
                    }
                }
                
                // If we didn't find a child, fallback to the assigned object itself 
                if (overlayObj == null)
                {
                     overlayObj = _worldOverlayObject;
                }
                _visionManager.WorldOverlay = overlayObj;
                
                _visionManager.MainCamera = _mainCamera;
            }
            else
            {
                Debug.LogError("VisionManager not found! Please add it to the scene.");
            }
        }
    }
}
