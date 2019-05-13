using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using World;

public class ButtonToResetMasterpiece : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
    private void OnTriggerEnter(Collider other)
    {

        StartCoroutine(WaitForRetrigger(5));
        


    }
    
    public IEnumerator WaitForRetrigger(int seconds)
    {
     
        if (!((Time.time - MyExhibitionBuilder.TimeForTrigger) > seconds)) yield break;

        MyExhibitionBuilder.TimeForTrigger = Time.time;
 
        
        Debug.Log("Reset Trigger");        
        
        MyExhibitionBuilder.resetMasterpiece();

    }
    
    
    
    
    
    
}
