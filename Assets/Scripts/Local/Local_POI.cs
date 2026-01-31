using UnityEngine;

public class Local_POI : MonoBehaviour
{
    public bool isDestroyed = false;
    

    // Update is called once per frame
    public void RunEffect()
    {
        isDestroyed = true;
        Debug.Log(gameObject.name + " is destroyed");
        Destroy(this);
    }
}
