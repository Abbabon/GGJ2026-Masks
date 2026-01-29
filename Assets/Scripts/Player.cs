using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] KeyCode actionKey = KeyCode.E;
    Human human;
    bool actionPressedLastFrame;

    void Start()
    {
        human = GetComponent<Human>();
        if (human == null)
            human = GetComponentInChildren<Human>();
    }

    void Update()
    {
        if (human == null) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(h, v);

        if (dir.sqrMagnitude > 0.01f)
            human.Move(dir.normalized);
        else
            human.Stop();

        bool actionPressed = Input.GetKey(actionKey);
        if (actionPressed && !actionPressedLastFrame)
            human.ActionStart();
        else if (!actionPressed && actionPressedLastFrame)
            human.ActionStop();
        actionPressedLastFrame = actionPressed;
    }
}
