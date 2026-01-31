using System;
using UnityEngine;

public class Local_Player : MonoBehaviour
{
    Local_Mover mover;
    Local_POI _nearPOI = null;
    private float startTime = 0;
    public float actionDuration = 5; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mover = GetComponent<Local_Mover>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        mover.Move(direction);
        if (_nearPOI != null && Time.time - startTime > actionDuration)
        {
            _nearPOI.RunEffect();
            _nearPOI = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Local_POI poi = other.GetComponent<Local_POI>();
        if (poi != null && !poi.isDestroyed)
        {
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
