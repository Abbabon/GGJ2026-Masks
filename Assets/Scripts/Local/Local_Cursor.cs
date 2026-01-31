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
    }
}
