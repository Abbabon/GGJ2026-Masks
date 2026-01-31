using System;
using UnityEngine;

public class Local_Mover : MonoBehaviour
{
    Rigidbody2D rb;
    public float _speed = 1;
    public Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = transform.Find("Anim").GetComponent<Animator>();
    }

    void OnMouseDown()
    {
        OnClicked();
    }

    public void OnClicked()
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
        // Destroy(gameObject);
        animator.SetBool("Dead",true);
        Destroy(this);
        Local_Npc npc = gameObject.GetComponent<Local_Npc>();
        if (npc != null)
        {
            Destroy(npc);
        }
    }

    private void Update()
    {
        animator.SetFloat("Speed", rb.linearVelocity.normalized.magnitude);
    }

    public void Move(Vector2 direction)
    {
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * _speed;   
        }
    }

    public void Stop()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;   
        }
    }

   
}
