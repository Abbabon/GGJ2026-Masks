using UnityEngine;

public class Local_Spawner : MonoBehaviour
{
    
    public GameObject prefab;

    public int count = 50;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        SpriteRenderer spr = gameObject.GetComponent<SpriteRenderer>();
        Transform characterContainer = transform.parent;
        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(prefab, characterContainer);
            Vector2 pos = InSpriteBounds(spr);
            go.transform.position = pos;
        }

        Destroy(gameObject);
    }
    
    public static Vector2 InSpriteBounds(SpriteRenderer sr)
    {
        Bounds b = sr.bounds;

        float x = Random.Range(b.min.x, b.max.x);
        float y = Random.Range(b.min.y, b.max.y);

        return new Vector2(x, y);
    }

}
