using UnityEngine;
using UnityEngine.SceneManagement;

namespace Masks
{
    public class Local_EndScreen : MonoBehaviour
    {
        [SerializeField] GameObject hereticWinScreen;
        [SerializeField] GameObject godWinScreen;

        Canvas canvas;

        void Awake()
        {
            canvas = GetComponent<Canvas>();
            if (canvas != null)
                canvas.enabled = false;
            hereticWinScreen.SetActive(false);
            godWinScreen.SetActive(false);
        }

        void Show()
        {
            Cursor.visible = true;
            if (canvas != null)
                canvas.enabled = true;
        }

        public void ShowHereticWin()
        {
            Show();
            hereticWinScreen.SetActive(true);
        }

        public void ShowGodWin()
        {
            Show();
            godWinScreen.SetActive(true);
        }

        public void Replay()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
