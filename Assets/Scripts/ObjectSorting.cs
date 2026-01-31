using UnityEngine;

public class ObjectSorting : MonoBehaviour
{
    void LateUpdate()
    {
        var spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        var sortingLine = transform.Find("sorting_line");

        if (sortingLine == null) return;

        int sortOrder = Mathf.RoundToInt(-sortingLine.position.y * 100);

        foreach (var renderer in spriteRenderers)
        {
            renderer.sortingOrder = sortOrder;
        }
    }
}
