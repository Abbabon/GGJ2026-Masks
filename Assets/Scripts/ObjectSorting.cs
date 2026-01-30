using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ObjectSorting : MonoBehaviour
{
    private Transform sortingLine;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        sortingLine = transform.Find("sorting_line");

        if (sortingLine == null)
        {
            Debug.LogError(
                $"[{name}] Missing child 'sorting_line'",
                this
            );
        }
    }

    void LateUpdate()
    {
        if (sortingLine == null) return;

        spriteRenderer.sortingOrder =
            Mathf.RoundToInt(-sortingLine.position.y * 100);
    }
}
