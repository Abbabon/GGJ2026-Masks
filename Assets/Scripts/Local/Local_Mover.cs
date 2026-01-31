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
        Debug.Log("Sprite clicked!");
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
