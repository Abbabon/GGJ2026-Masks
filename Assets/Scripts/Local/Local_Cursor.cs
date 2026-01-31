using UnityEngine;

public class Local_Cursor : MonoBehaviour
{
    public float speed = 10f;
    Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        Vector2 target = cam.ScreenToWorldPoint(Input.mousePosition);
        transform.position = Vector2.Lerp(
            transform.position,
            target,
            speed * Time.deltaTime
        );

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
    }
}
