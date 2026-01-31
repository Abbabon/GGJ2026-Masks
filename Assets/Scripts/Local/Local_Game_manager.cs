using UnityEngine;

public class Local_Game_manager : MonoBehaviour
{
    
    public void PoiDestroyed(Local_POI poi)
    {
        Debug.Log("A" + poi.name);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HereticKilled()
    {
        Debug.Log("Heretic Killed");
    }

    public void NoneHereticKilled()
    {
        Debug.Log("NoneHeretic Killed");
    }
}
