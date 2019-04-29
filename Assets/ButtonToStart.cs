using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonToStart : MonoBehaviour
{
    //Displayal d=new Displayal();
    //var ex=new MyExhibitionBuilder();
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
 
        // parsing string to int since the id is a number
        
        
        StartCoroutine(MyExhibitionBuilder.getMorelikeMyMasterpiece());

    }
}

