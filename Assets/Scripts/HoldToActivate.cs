using UnityEngine;
using UnityEngine.UI; // Required if you add a loading bar later

namespace Masks
{
    public partial class HoldToActivate : MonoBehaviour
    {
        [Header("Settings")]
        public float holdDuration = 5.0f; // Total time required
        public KeyCode interactKey = KeyCode.Space;

        [Header("State")]
        private float currentTimer = 0f;
        private bool isPlayerInRange = false;
        private bool isActivated = false;

        void Start() {
            // AudioManager.Instance.PlaySFX("god_smite");
            // AudioManager.Instance.PlayMusic("god_smite");
        }

        void Update()
        {
            // Only allow interaction if player is close and not already activated
            if (isPlayerInRange && !isActivated)
            {
                if (Input.GetKey(interactKey))
                {
                    currentTimer += Time.deltaTime;
                    Debug.Log("Activating... " + (currentTimer / holdDuration * 100).ToString("f0") + "%");

                    if (currentTimer >= holdDuration)
                    {
                        OnActivationComplete();
                    }
                }
                else
                {
                    // Reset progress if they let go of the key
                    currentTimer = 0f;
                }
            }
        }

        private void OnActivationComplete()
        {
            isActivated = true;
            currentTimer = holdDuration;
            Debug.Log("Object Activated!");

            // ADD YOUR LOGIC HERE:
            // GetComponent<Renderer>().material.color = Color.green;
            // doorAnimator.SetTrigger("Open");
        }

        // Proximity Detection
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerInRange = true;
                Debug.Log("Player entered range. Hold Space to activate.");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerInRange = false;
                currentTimer = 0f; // Reset progress if they walk away
                Debug.Log("Player left range.");
            }
        }
    }
}
