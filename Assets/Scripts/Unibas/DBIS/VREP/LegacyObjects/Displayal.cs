using System;
using System.Collections;
using System.Collections.Generic;
using CineastUnityInterface.CineastAPI;
using DefaultNamespace.VREM.Model;
using Unibas.DBIS.DynamicModelling;
using Unibas.DBIS.DynamicModelling.Models;
using UnityEngine;
using Valve.VR.InteractionSystem;
using World;

public class Displayal : MonoBehaviour
{
    private CuboidModel _anchor = new CuboidModel(1, 0.01f, .1f);
    private Exhibit _exhibitModel;


    public string id;


    private Renderer m_Renderer;

    public Vector3 OriginalPosition;
    public Quaternion OriginalRotation;

    

    public void RestorePosition()
    {
        transform.localPosition = OriginalPosition;
        transform.localRotation = OriginalRotation;
        var rigid = GetComponent<Rigidbody>();
        rigid.velocity = Vector3.zero;
    }

    public void SetExhibitModel(Exhibit exhibit)
    {
        _exhibitModel = exhibit;
        id = _exhibitModel.id;
        name = "Displayal (" + id + ")";
        var tp = transform.Find("TitlePlaquette");
        if (tp != null) {
            if (string.IsNullOrEmpty(exhibit.name))
                tp.gameObject.SetActive(false);
            else
                tp.GetComponent<Plaquette>().text.text = exhibit.name;
        }
        else {
            Debug.LogError("no tp");
        }

        var dp = transform.Find("DescriptionPlaquette");
        if (dp != null) {
            if (string.IsNullOrEmpty(exhibit.description))
                dp.gameObject.SetActive(false);
            else
                dp.GetComponent<Plaquette>().text.text = exhibit.description;
        }
        else {
            Debug.LogError("no dp");
        }

    }

    private void OnTriggerEnter(Collider other)
    {
    StartCoroutine(WaitForRetrigger(3));
        
    }
   

    // Delays the retrigger possibility
    public IEnumerator WaitForRetrigger(int seconds)
    {
     
        if (!((Time.time - MyExhibitionBuilder.TimeForTrigger) > seconds)) yield break;

        MyExhibitionBuilder.TimeForTrigger = Time.time;
 
        // parsing string to int since the id is a number
        int x = 0;
        Int32.TryParse(this.id, out x);
        
        StartCoroutine(MyExhibitionBuilder.getMorelikeThisOne(x));

    }


    public Exhibit GetExhibit()
    {
        return _exhibitModel;
    }

    // Use this for initialization
    private void Start()
    {
        m_Renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    private void Update()
    {
    }
}