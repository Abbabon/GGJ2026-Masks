using System.Collections;
using UnityEngine;

namespace Masks
{
    public class Local_Npc : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] float minInitialWait = 0.5f;
        [SerializeField] float maxInitialWait = 2f;
        [SerializeField] float minMoveDuration = 1f;
        [SerializeField] float maxMoveDuration = 4f;
        [SerializeField] float minPauseDuration = 0.5f;
        [SerializeField] float maxPauseDuration = 2f;
        [SerializeField] [Range(0f, 1f)] float chanceToChangeDirection = 0.5f;

        public bool IsAlive { get; private set; } = true;

        Local_Mover _mover;

        public void Kill()
        {
            IsAlive = false;
            _mover.Kill();
            // gameObject.SetActive(false);
        }

        void Start()
        {
            _mover = GetComponent<Local_Mover>();
            if (_mover != null)
                StartCoroutine(WanderRoutine());
        }

        IEnumerator WanderRoutine()
        {
            // Initial delay to desynchronize NPCs
            yield return new WaitForSeconds(Random.Range(minInitialWait, maxInitialWait));

            while (true)
            {
                // Pick a random direction and move
                Vector2 direction = Random.insideUnitCircle.normalized;
                _mover.Move(direction);

                // Move for a random duration
                yield return new WaitForSeconds(Random.Range(minMoveDuration, maxMoveDuration));

                // Chance to change direction before stopping
                if (Random.value < chanceToChangeDirection)
                {
                    direction = Random.insideUnitCircle.normalized;
                    _mover.Move(direction);

                    // Move for another duration after changing direction
                    yield return new WaitForSeconds(Random.Range(minMoveDuration, maxMoveDuration));
                }

                // Always stop and wait after the movement phase
                _mover.Stop();
                yield return new WaitForSeconds(Random.Range(minPauseDuration, maxPauseDuration));
            }
        }
    }
}
