﻿using System;
using CineastUnityInterface.CineastAPI;
using DefaultNamespace.VREM.Model;
using Unibas.DBIS.DynamicModelling;
using Unibas.DBIS.DynamicModelling.Models;
using UnityEngine;
using Valve.VR.InteractionSystem;

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
        if (tp != null)
        {
            if (string.IsNullOrEmpty(exhibit.name))
                tp.gameObject.SetActive(false);
            else
                tp.GetComponent<Plaquette>().text.text = exhibit.name;
        }
        else
        {
            Debug.LogError("no tp");
        }

        var dp = transform.Find("DescriptionPlaquette");
        if (dp != null)
        {
            if (string.IsNullOrEmpty(exhibit.description))
                dp.gameObject.SetActive(false);
            else
                dp.GetComponent<Plaquette>().text.text = exhibit.description;
        }
        else
        {
            Debug.LogError("no dp");
        }
        
        // create new Boxcollider

        /*var magicOffset = 0.17f;
        var anch = ModelFactory.CreateCuboid(_anchor);
        var col = anch.AddComponent<BoxCollider>();
       
        col.center = new Vector3(_anchor.Width / 2, _anchor.Height / 2, _anchor.Depth/2);
        col.size = new Vector3(_anchor.Width, _anchor.Height, _anchor.Depth);
        anch.name = "Anchor (" + id + ")";
        anch.transform.parent = transform.parent;
        anch.transform.localPosition = new Vector3(_exhibitModel.position.x-_anchor.Width/2, _exhibitModel.position.y-(_exhibitModel.size.y/2+magicOffset), -_anchor.Depth); //0.2 is magic number for frame
        anch.transform.localRotation = Quaternion.Euler(Vector3.zero);

        col.isTrigger = true; // funktioniert nid
        */

        
        //if (VREPController.Instance.Settings.PlaygroundEnabled)
        {
            var magicOffset = 0.17f;
            //var t = gameObject.AddComponent<Throwable>();
            //t.attachmentFlags = Hand.AttachmentFlags.VelocityMovement | Hand.AttachmentFlags.TurnOffGravity;
                //Hand.AttachmentFlags.VelocityMovement  Hand.AttachmentFlags.TurnOffGravity;
            //t.releaseVelocityStyle = ReleaseStyle.AdvancedEstimation;
            var anch = ModelFactory.CreateCuboid(_anchor);
            var col = anch.AddComponent<BoxCollider>();
            col.center = new Vector3(_anchor.Width / 2, _anchor.Height / 2, _anchor.Depth/2);
            col.size = new Vector3(_anchor.Width, _anchor.Height, _anchor.Depth);
            col.isTrigger = true;
            anch.name = "Anchor (" + id + ")";
            anch.transform.parent = transform.parent;
            anch.transform.localPosition = new Vector3(_exhibitModel.position.x-_anchor.Width/2, _exhibitModel.position.y-(_exhibitModel.size.y/2+magicOffset), -_anchor.Depth); //0.2 is magic number for frame
            anch.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        
        Debug.Log("TAG in Displayal is actually gameobjectname   " + other.gameObject.name);
        Debug.Log("This is the tag we are looking for    " + other.tag);
        
        if (other.tag == "Player")
        {
            Debug.Log("other.tag==Player");
            //setTestingBool();
        }

        // We use the HeadCollider for now because thats the trigger when "Driving" through the img
        if (other.gameObject.name == "HeadCollider")
        {
            Debug.Log("Here we gotta call our function");
            int x = 0;

            Int32.TryParse(this.id, out x);
            CineastApi api = GetComponent<CineastApi>();
            StartCoroutine(MyExhibitionBuilder.getMorelikeThisOne(x,api));
            //String a =other.GetComponent<BoxCollider>().gameObject.name;
            Debug.Log("das do  " + this.id); // this id returns "Exhibit 0" kame also ufrüefe mitere methode us MyExhibition..
            // Mach zerscht e Methode wo direkt mit dem trigger neue Bilder ladet..
        }
        
        if (other.tag == "HeadCollider")
        {
            Debug.Log("Wieso ? other.tag == HeadCollider");
        }

       
        //throw new System.NotImplementedException();
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