using UnityEngine;

public class PlayerSorting : MonoBehaviour
{
    public Transform sortingPoint;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // Convert Y position to sorting order
        spriteRenderer.sortingOrder = Mathf.RoundToInt(-sortingPoint.position.y * 100);
    }
}
