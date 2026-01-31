using UnityEngine;

public class MenuMusicPlayer : MonoBehaviour
{
    void Start()
    {
        // Wait a frame to ensure AudioManager is initialized
        if (AudioManager.Instance != null)
        {
            Debug.Log("[MenuMusicPlayer] Playing opening theme music");
            AudioManager.Instance.PlayMusic("opening_theme");
        }
        else
        {
            Debug.LogWarning("[MenuMusicPlayer] AudioManager.Instance is null. Cannot play opening theme.");
        }
    }
}
