using System.Collections;
using UnityEngine;

public class Local_POI : MonoBehaviour
{
    public bool isDestroyed = false;
    public float glowTime = 4;

    public void RunEffect()
    {
        StartCoroutine(RunEffectAfterDelay());
    }

    IEnumerator RunEffectAfterDelay()
    {
        Local_POI[] allPOIs = FindObjectsByType<Local_POI>(FindObjectsSortMode.None);
        Debug.Log("Glowing all POIs" + allPOIs.Length);
        foreach (Local_POI poi in allPOIs)
        {
            poi.StartGlow();
        }
        yield return new WaitForSeconds(glowTime);
        isDestroyed = true;
        Debug.Log(gameObject.name + " is destroyed");
        Destroy(this);
        Local_Game_manager instance = FindObjectOfType<Local_Game_manager>();
        instance.SetLeftPOI(allPOIs.Length - 1);
    }

    public void StartGlow()
    {
        // DO ANIMATION HERE
        Debug.Log(gameObject.name);
        StartCoroutine(GlowEffect());
    }

    IEnumerator GlowEffect()
    {
        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(glowTime);
        spriteRenderer.color = Color.white;
    }
}
