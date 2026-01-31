using System;
using UnityEngine;

public class Local_Mover : MonoBehaviour
{
    Rigidbody2D rb;
    public float _speed = 1;
    public Animator animator;
    [SerializeField] private GodIris _godIrisPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = transform.Find("Anim").GetComponent<Animator>();
    }

    public void Smite()
    {
        Kill(true);
        AudioManager.Instance.PlaySFX("god_smite");
    }

    public void Kill(bool isFromGod = false)
    {
        Local_Player player = gameObject.GetComponent<Local_Player>();
        Local_Game_manager instance = FindObjectOfType<Local_Game_manager>();

        GodIris iris = null;
        if (_godIrisPrefab != null)
        {
            iris = Instantiate(_godIrisPrefab, transform.position, Quaternion.identity);
        }

        if (player != null)
        {
            instance.HereticKilled();
            if (iris != null) iris.GodWon();
        }
        else
        {
            Debug.Log("NonHeretic killed");
            if (isFromGod) {
                instance.NoneHereticKilled();
                if (iris != null) iris.GodLost();
            }
        }

        Stop();
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
        Vector2 linearVelocity = rb.linearVelocity;
        animator.SetFloat("Speed", linearVelocity.normalized.magnitude);
        if (linearVelocity.x != 0)
        {
            transform.localScale = new Vector3( linearVelocity.x < 0 ? 1:-1, transform.localScale.y, transform.localScale.z);    
        }
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
