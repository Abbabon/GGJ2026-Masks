using System;
using UnityEngine;

public class Local_Mover : MonoBehaviour
{
    Rigidbody2D rb;
    public float _speed = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnMouseDown()
    {
        Local_Player player = gameObject.GetComponent<Local_Player>();
        Local_Game_manager instance = FindObjectOfType<Local_Game_manager>();
        if (player != null)
        {
            instance.HereticKilled();
        }
        else
        {
            Debug.Log("NonHeretic killed");
            instance.NoneHereticKilled();
        }
        // animation?
        Destroy(gameObject);
    }

    public void Move(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * _speed;
    }

    public void Stop()
    {
        rb.linearVelocity = Vector2.zero;
    }

   
}
