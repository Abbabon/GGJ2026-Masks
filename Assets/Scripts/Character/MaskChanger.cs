using UnityEngine;

public class MaskChanger : MonoBehaviour
{
	public GameObject[] Masks_Array;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    	for(int i=0 ; i<=7 ; i++)
    	{
    		Masks_Array[i].SetActive(false);
    	}
        int randomInt = Random.Range(0,7);
        {
        	Masks_Array[randomInt].SetActive(true);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
