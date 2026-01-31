using System;
using UnityEngine;

public class Local_Player : MonoBehaviour
{
    Local_Mover mover;
    Local_POI _nearPOI = null;
    private float startTime = 0;
    public float actionDuration = 5;
    Local_Game_manager gameManager;

    void Start()
    {
        mover = GetComponent<Local_Mover>();
        gameManager = FindObjectOfType<Local_Game_manager>();
        transform.localScale = Vector3.one * 0.75f;

        var spawns = FindObjectsByType<PlayerSpawn>(FindObjectsSortMode.None);
        if (spawns.Length > 0)
        {
            var spawn = spawns[UnityEngine.Random.Range(0, spawns.Length)];
            transform.position = spawn.transform.position;
        }
    }

    void Update()
    {
        if (mover == null) return;

        if (gameManager != null && gameManager.CurrentState == Local_Game_manager.GameState.GameOver)
        {
            mover.Stop();
            return;
        }

        Vector2 direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        mover.Move(direction);
        if (_nearPOI != null && Time.time - startTime > actionDuration)
        {
            Debug.Log("Near POI ended: " + _nearPOI.name);
            _nearPOI.RunEffect();
            _nearPOI = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Local_POI poi = other.GetComponent<Local_POI>();
        if (poi != null && !poi.isDestroyed)
        {
            Debug.Log("Near POI: " + poi.name);
            _nearPOI = poi;
            startTime = Time.time;
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("POI"))
        {
            _nearPOI = null;
        }
    }
}
