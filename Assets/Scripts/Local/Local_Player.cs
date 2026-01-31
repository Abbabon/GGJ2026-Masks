using UnityEngine;

public class Local_Player : MonoBehaviour
{
    Local_Mover mover;
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
    }
}
