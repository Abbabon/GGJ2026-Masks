using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Masks
{
    public class LoadingScript : MonoBehaviour
    {
        [Header("Loading Settings")]
        [Tooltip("Duration of the loading animation in seconds")]
        [SerializeField] float loadingDuration = 2f;

        [Header("References (Auto-found if not assigned)")]
        [SerializeField] Image loadFillImage;
        [SerializeField] GameObject buttonGameObject;
        [SerializeField] GameObject loadingGameObject;

        const string CanvasName = "Canvas";
        const string FirstScreenName = "FirstScreen";
        const string EyeName = "Eye";
        const string LoadName = "Load";
        const string ButtonName = "Button";
        const string LoadingName = "Loading";

        void Start()
        {
            // Find references if not assigned
            if (loadFillImage == null)
            {
                loadFillImage = FindLoadImage();
            }

            if (buttonGameObject == null)
            {
                buttonGameObject = FindGameObject(CanvasName, FirstScreenName, ButtonName);
            }

            if (loadingGameObject == null)
            {
                loadingGameObject = FindGameObject(CanvasName, FirstScreenName, LoadingName);
            }

            // Validate references
            if (loadFillImage == null)
            {
                Debug.LogError("[LoadingScript] Could not find Load Image component. Expected path: Canvas/FirstScreen/Eye/Load");
                return;
            }

            if (buttonGameObject == null)
            {
                Debug.LogError("[LoadingScript] Could not find Button GameObject. Expected path: Canvas/FirstScreen/Button");
                return;
            }

            if (loadingGameObject == null)
            {
                Debug.LogError("[LoadingScript] Could not find Loading GameObject. Expected path: Canvas/FirstScreen/Loading");
                return;
            }

            // Initialize state
            loadFillImage.fillAmount = 0f;
            buttonGameObject.SetActive(false);
            loadingGameObject.SetActive(true);

            // Start loading coroutine
            StartCoroutine(LoadingCoroutine());
        }

        IEnumerator LoadingCoroutine()
        {
            float elapsedTime = 0f;

            while (elapsedTime < loadingDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / loadingDuration);
                loadFillImage.fillAmount = progress;

                yield return null;
            }

            // Ensure fill amount is exactly 1
            loadFillImage.fillAmount = 1f;

            // When fully loaded, activate button and deactivate loading text
            buttonGameObject.SetActive(true);
            loadingGameObject.SetActive(false);

            Debug.Log("[LoadingScript] Loading complete!");
        }

        Image FindLoadImage()
        {
            // Find Canvas/FirstScreen/Eye/Load
            GameObject canvas = GameObject.Find(CanvasName);
            if (canvas == null) return null;

            Transform firstScreen = canvas.transform.Find(FirstScreenName);
            if (firstScreen == null) return null;

            Transform eye = firstScreen.Find(EyeName);
            if (eye == null) return null;

            Transform load = eye.Find(LoadName);
            if (load == null) return null;

            return load.GetComponent<Image>();
        }

        GameObject FindGameObject(string parentName, string childName, string targetName)
        {
            GameObject parent = GameObject.Find(parentName);
            if (parent == null) return null;

            Transform child = parent.transform.Find(childName);
            if (child == null) return null;

            Transform target = child.Find(targetName);
            if (target == null) return null;

            return target.gameObject;
        }
    }
}
