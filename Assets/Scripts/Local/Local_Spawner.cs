using UnityEngine;

public class Local_Spawner : MonoBehaviour
{
    
    public GameObject prefab;

    private SpriteRenderer _spr;

    public int count = 50;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        _spr = gameObject.GetComponent<SpriteRenderer>();
        Transform characterContainer = transform.parent;
        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(prefab, characterContainer);
            Vector2 pos = GetSpriteBounds();
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * Local_Game_manager.kCharactersScale;
        }

        Destroy(gameObject);
    }
    
    public Vector2 GetSpriteBounds()
    {
        Bounds b = _spr.bounds;

        float x = Random.Range(b.min.x, b.max.x);
        float y = Random.Range(b.min.y, b.max.y);

        return new Vector2(x, y);
    }

}
