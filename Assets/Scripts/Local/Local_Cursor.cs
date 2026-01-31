using Unity.VisualScripting;
using UnityEngine;

public class Local_Cursor : MonoBehaviour
{
    public float speed = 10f;
    Camera cam;
    Animation anim;
    Color color;
    SpriteRenderer spriteRenderer;

    void Start()
    {
        cam = Camera.main;
        // anim = transform.Find("Aim_Container").GetComponent<Animation>();
        spriteRenderer = transform.Find("Aim_Container/Aim/Aim_Inner").GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        Vector2 target = cam.ScreenToWorldPoint(Input.mousePosition);
        transform.position = target;
        spriteRenderer.color = color;

        if (Input.GetMouseButtonDown(0))
        {
            var hit = Physics2D.OverlapPoint(target);
            if (hit != null)
            {
                var mover = hit.GetComponent<Local_Mover>();
                if (mover != null)
                    mover.OnClicked();
            }
        }

        if (Input.GetMouseButton(0))
        {
            // anim.Play("God_Aim_Shoot");
            spriteRenderer.color = Color.red;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("OnTriggerEnter2D");
        if (other.gameObject.CompareTag("Character"))
        {
            // anim.Play("God_Aim_OnCollide");
            color = Color.white;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("OnTriggerExit2D");
        if (other.gameObject.CompareTag("Character"))
        {
            // anim.Play("God_Aim_Idle");
            color = Color.black;
        }
    }
}
