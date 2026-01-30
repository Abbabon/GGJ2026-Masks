#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace GodMode
{
    /// <summary>
    /// Temporary tool to generate the Prefabs for GGJ.
    /// Run via Tools > Generate GGJ Prefabs.
    /// </summary>
    public class PrefabGenerator
    {
        [MenuItem("Tools/Generate GGJ Prefabs")]
        public static void Generate()
        {
            string folder = "Assets/Prefabs";
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            CreateGodPrefab(folder);
            CreateHereticPrefab(folder);
            CreateGameWorldPrefab(folder);
            
            AssetDatabase.Refresh();
            Debug.Log("Generated Prefabs in " + folder);
        }

        private static void CreateGodPrefab(string folder)
        {
            GameObject root = new GameObject("GodCharacter");
            
            // Components
            var cursor = root.AddComponent<GodCursor>();

            // Children
            GameObject cVisual = new GameObject("CursorVisual");
            cVisual.transform.parent = root.transform;
            var sr = cVisual.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite(32, Color.red);
            sr.sortingOrder = 20;

            GameObject iris = new GameObject("IrisMask");
            iris.transform.parent = root.transform;
            var mask = iris.AddComponent<SpriteMask>();
            mask.sprite = CreateCircleSprite(512, Color.white);

            // Link
            cursor.CursorVisual = cVisual.transform;
            cursor.IrisVisual = iris.transform;

            PrefabUtility.SaveAsPrefabAsset(root, folder + "/GodCharacter.prefab");
            Object.DestroyImmediate(root);
        }

        private static void CreateHereticPrefab(string folder)
        {
            GameObject root = new GameObject("HereticCharacter");

            // Components
            root.AddComponent<SimpleMovement>();

            // Children
            GameObject body = new GameObject("BodyVisual");
            body.transform.parent = root.transform;
            var sr = body.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite(64, Color.blue);
            sr.sortingOrder = 0;
            // Stealth
            sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

            PrefabUtility.SaveAsPrefabAsset(root, folder + "/HereticCharacter.prefab");
            Object.DestroyImmediate(root);
        }

        private static void CreateGameWorldPrefab(string folder)
        {
            GameObject root = new GameObject("GameWorld");
            
            // 1. Background
            GameObject bg = new GameObject("Background");
            bg.transform.parent = root.transform;
            var sr = bg.AddComponent<SpriteRenderer>();
            // 1920x1080 placeholder (Gray Gradient)
            sr.sprite = CreateSquareSprite(1024, Color.gray); // Placeholder sizing, user will likely replace sprite
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(19.2f, 10.8f); // roughly 1920x1080 in units if PPU=100
            sr.sortingOrder = -10;
            
            // Bounds
            // User now uses the Background Sprite itself as bounds, so no component needed.

            // 2. Overlay
            GameObject overlay = new GameObject("WorldOverlay");
            overlay.transform.parent = root.transform;
            var osr = overlay.AddComponent<SpriteRenderer>();
            osr.sprite = CreateSquareSprite(1024, new Color(0, 0, 0, 0.75f));
            osr.drawMode = SpriteDrawMode.Tiled;
            osr.size = new Vector2(50, 50); // Huge coverage
            osr.sortingOrder = 10;
            osr.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;

            PrefabUtility.SaveAsPrefabAsset(root, folder + "/GameWorld.prefab");
            Object.DestroyImmediate(root);
        }

         private static Sprite CreateSquareSprite(int size, Color c)
        {
            Texture2D texture = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = c;
            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private static Sprite CreateCircleSprite(int size, Color c)
        {
            Texture2D texture = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];
            float center = size / 2f;
            float radius = size / 2f;
            float rSquared = radius * radius;
            Color clear = Color.clear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    if (dx*dx + dy*dy < rSquared)
                        pixels[y * size + x] = c;
                    else
                        pixels[y * size + x] = clear;
                }
            }
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }
    }
}
#endif
