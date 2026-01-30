using UnityEngine;

namespace GodMode
{
    /// <summary>
    /// Setups a test scene with God and Heretic modes, togglable via 'Q'.
    /// </summary>
    public class GameTestSetup : MonoBehaviour
    {
        [Header("Resources")]
        [SerializeField] private Sprite _backgroundSprite;
        [SerializeField] private Sprite _irisMaskSprite;
        [SerializeField] private Sprite _hereticSprite;

        private GameObject _godRoot;
        private GameObject _hereticRoot;
        private Camera _mainCamera;

        private bool _isGodMode = true;

        private void Start()
        {
            _mainCamera = Camera.main;
            SetupWorld();
        }

        private void Update()
        {
            // Toggle Mode
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ToggleMode();
            }

            // Camera Follow logic (for Heretic Mode)
            if (!_isGodMode && _hereticRoot != null && _mainCamera != null)
            {
                Vector3 targetPos = _hereticRoot.transform.position;
                _mainCamera.transform.position = new Vector3(targetPos.x, targetPos.y, -10f); // Keep Z offset
                _mainCamera.orthographicSize = 5f; // Zoomed in
            }
            else if (_isGodMode && _mainCamera != null)
            {
                // Reset camera for God Mode (Full view)
                _mainCamera.transform.position = new Vector3(0, 0, -10f);
                _mainCamera.orthographicSize = 10f; // Zoomed out/Full view
            }
        }

        private void ToggleMode()
        {
            _isGodMode = !_isGodMode;
            
            // Toggle Controls
            var godController = _godRoot.GetComponent<GodCursor>();
            var hereticMovement = _hereticRoot.GetComponent<SimpleMovement>();

            if (godController) godController.enabled = _isGodMode;
            if (hereticMovement) hereticMovement.enabled = !_isGodMode;

            // Toggle Vision (Masks)
            // Only the active player's mask determines what is revealed in the overlay
            var godMask = _godRoot.GetComponentInChildren<SpriteMask>();
            var hereticMask = _hereticRoot.GetComponentInChildren<SpriteMask>();

            if (godMask) godMask.enabled = _isGodMode;
            if (hereticMask) hereticMask.enabled = !_isGodMode;

            Debug.Log($"Switched to {(_isGodMode ? "GOD" : "HERETIC")} Mode");
        }

        private void SetupWorld()
        {
             // 1. Create Background (The world)
            var bgObj = new GameObject("Background_World");
            var bgRender = bgObj.AddComponent<SpriteRenderer>();
            // Use Gradient if no sprite provided
            bgRender.sprite = _backgroundSprite ? _backgroundSprite : CreateGradientSprite(2048, 2048);
            bgRender.sortingOrder = -10;
            bgObj.transform.localScale = new Vector3(1, 1, 1);

            var overlayObj = new GameObject("Overlay_Darkness");
            var overlayRender = overlayObj.AddComponent<SpriteRenderer>();
            overlayRender.sprite = _backgroundSprite ? _backgroundSprite : CreateColorSprite(Color.black, 2048, 2048);
            overlayRender.sortingOrder = 10;
            overlayRender.transform.localScale = new Vector3(10, 10, 1);
            
            // Set Opacity to 0.75
            Color col = overlayRender.color;
            col.a = 0.75f;
            overlayRender.color = col;
            
            // Critical: Set Mask Interaction to "Visible Outside Mask"
            overlayRender.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;

            // 3. Setup God
            SetupGod();

            // 4. Setup Heretic
            SetupHeretic();

            // Initial State
            _isGodMode = true;
            // Ensure both are active
            if (_godRoot) _godRoot.SetActive(true);
            if (_hereticRoot) _hereticRoot.SetActive(true);
            
            // Apply initial control and mask state
            var godController = _godRoot.GetComponent<GodCursor>();
            var hereticMovement = _hereticRoot.GetComponent<SimpleMovement>();
            var godMask = _godRoot.GetComponentInChildren<SpriteMask>();
            var hereticMask = _hereticRoot.GetComponentInChildren<SpriteMask>();

            if (godController) godController.enabled = true;
            if (hereticMovement) hereticMovement.enabled = false;
            
            // Only God mask active initially
            if (godMask) godMask.enabled = true;
            if (hereticMask) hereticMask.enabled = false;
        }

        private void SetupGod()
        {
            _godRoot = new GameObject("God_Root");
            
            var cursorObj = new GameObject("Cursor_Visual");
            var irisObj = new GameObject("God_Iris_Mask");
            
            cursorObj.transform.parent = _godRoot.transform;
            irisObj.transform.parent = _godRoot.transform;

            // Cursor
            var cursorRender = cursorObj.AddComponent<SpriteRenderer>();
            cursorRender.sprite = CreateColorSprite(Color.red, 32, 32); 
            cursorRender.sortingOrder = 20;

            // Iris Mask
            var mask = irisObj.AddComponent<SpriteMask>();
            mask.sprite = _irisMaskSprite ? _irisMaskSprite : CreateCircleSprite(512); // Big Iris
            
            // Controller
            var controller = _godRoot.AddComponent<GodCursor>();
            controller.CursorVisual = cursorObj.transform;
            controller.IrisVisual = irisObj.transform;
        }

        private void SetupHeretic()
        {
            _hereticRoot = new GameObject("Heretic_Root");

            // Heretic Body
            var bodyObj = new GameObject("Heretic_Body");
            bodyObj.transform.parent = _hereticRoot.transform;
            var bodyRender = bodyObj.AddComponent<SpriteRenderer>();
            bodyRender.sprite = _hereticSprite ? _hereticSprite : CreateColorSprite(Color.blue, 64, 64);
            bodyRender.sortingOrder = 0; // Under overlay, but visible where masked

            // Heretic Vision Mask (Fog of War clearer)
            var maskObj = new GameObject("Heretic_Vision_Mask");
            maskObj.transform.parent = _hereticRoot.transform;
            maskObj.transform.localPosition = Vector3.zero;
            
            var mask = maskObj.AddComponent<SpriteMask>();
            mask.sprite = _irisMaskSprite ? _irisMaskSprite : CreateCircleSprite(300); // Smaller than God's maybe?

            // Movement
            _hereticRoot.AddComponent<SimpleMovement>();
        }

        // Helpers (Duplicated for availability, could be static util)
        private Sprite CreateColorSprite(Color color, int width, int height)
        {
            Texture2D texture = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        }

        private Sprite CreateCircleSprite(int size)
        {
            Texture2D texture = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];
            float center = size / 2f;
            float radius = size / 2f;
            float rSquared = radius * radius;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    if (dx*dx + dy*dy < rSquared)
                        pixels[y * size + x] = Color.white;
                    else
                        pixels[y * size + x] = Color.clear;
                }
            }
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private Sprite CreateGradientSprite(int width, int height)
        {
            Texture2D texture = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                // Vertical gradient from White (bottom) to Gray (top)
                float t = (float)y / (height - 1);
                Color c = Color.Lerp(Color.white, Color.gray, t);
                
                for (int x = 0; x < width; x++)
                {
                    pixels[y * width + x] = c;
                }
            }
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        }
    }
}
