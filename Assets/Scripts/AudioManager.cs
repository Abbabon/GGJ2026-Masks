using UnityEngine;
using System;

namespace Masks
{
    public class AudioManager : MonoBehaviour
    {
        // Static instance so it's accessible from anywhere
        public static AudioManager Instance;

        [System.Serializable]
        public class Sound
        {
            public string name;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1f;
            [Range(.1f, 3f)] public float pitch = 1f;
        }

        public Sound[] musicSounds, sfxSounds;
        public AudioSource musicSource, sfxSource;

        private void Awake()
        {
            // Singleton Logic: Ensures only one AudioManager exists
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Keeps audio playing between scenes
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PlayMusic(string name)
        {
            Sound s = Array.Find(musicSounds, x => x.name == name);
            if (s != null)
            {
                musicSource.clip = s.clip;
                musicSource.Play();
            }
        }

        public void PlaySFX(string name)
        {
            Sound s = Array.Find(sfxSounds, x => x.name == name);
            if (s != null)
            {
                // PlayOneShot allows sounds to overlap without cutting each other off
                sfxSource.PlayOneShot(s.clip, s.volume);
            }
        }
    }
}
