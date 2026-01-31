using System.Collections;
using UnityEngine;

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
        gameObject.SetActive(false);
    }

    void Start()
    {
        _mover = GetComponent<Local_Mover>();
        if (_mover != null)
        {
            var gameManager = FindFirstObjectByType<Local_Game_manager>();
            if (gameManager != null && gameManager.CurrentState == Local_Game_manager.GameState.Playing)
            {
                StartCoroutine(WanderRoutine());
            }
            else
            {
                Local_Game_manager.OnGameStart += HandleGameStart;
            }
        }
    }

    void OnDestroy()
    {
        Local_Game_manager.OnGameStart -= HandleGameStart;
    }

    void HandleGameStart()
    {
        if (_mover != null) 
            StartCoroutine(WanderRoutine());
    }

    IEnumerator WanderRoutine()
    {
        while (true)
        {
            // Initial random wait
            yield return new WaitForSeconds(Random.Range(minInitialWait, maxInitialWait));

            // Pick a random direction and move
            Vector2 direction = Random.insideUnitCircle.normalized;
            _mover.Move(direction);

            // Move for a random duration
            yield return new WaitForSeconds(Random.Range(minMoveDuration, maxMoveDuration));

            if (Random.value < chanceToChangeDirection)
            {
                // Change direction
                direction = Random.insideUnitCircle.normalized;
                _mover.Move(direction);
            }
            else
            {
                // Pause (stop, then wait)
                _mover.Stop();
                yield return new WaitForSeconds(Random.Range(minPauseDuration, maxPauseDuration));
            }
        }
    }
}
