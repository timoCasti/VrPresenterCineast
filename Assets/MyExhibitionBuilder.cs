using System;
using System.Collections;
using System.Collections.Generic;
using CineastUnityInterface.CineastAPI;
using DefaultNamespace;
using DefaultNamespace.VREM;
using DefaultNamespace.VREM.Model;
using Unibas.DBIS.VREP;
using Unibas.DBIS.VREP.Core;
using Unibas.DBIS.VREP.World;
using UnityEngine;
using UnityEngine.Tilemaps;
using World;
using Room = DefaultNamespace.VREM.Model.Room;
using Wall = DefaultNamespace.VREM.Model.Wall;

public class MyExhibitionBuilder : MonoBehaviour
{
    private static ExhibitionManager myexhibitionManager;
    private Exhibition myEx;
    public Vector3 LobbySpawn = new Vector3(0, -9, 0);
    public Boolean testBool = false;

    private static List<String> randomIds;
    private static List<String> similarIds;
    
    // Triggerlist
    private List<Boolean> triggerList;


   
    // Use this for initialization
    void Start()
    {
        
        
        StartCoroutine(getCineastImg(5));
        
        //OnTriggerEnter();

        //getCineastImg(5);
    }


    // Update is called once per frame
    void Update()
    {
        // StartCoroutine(collision());
        /*if (myexhibitionManager.GetRoomByIndex(0).Walls[0].Displayals[0].isActiveAndEnabled)
        {
            OnTriggerEnter(myexhibitionManager.GetRoomByIndex(0).Walls[0].Displayals[0].GetComponent<BoxCollider>());
            Debug.Log("i got here");
        }*/

        /*
        if (testBool)
        {
            testLog();
        }
        */

    }


    public static IEnumerator getMorelikeThisOne(int exhibitNumber, CineastApi api)
    {
        Debug.Log( " Exhibitnumber:  "+ exhibitNumber);

        CineastApi myApi = CineastApi.FindObjectOfType<CineastApi>();
        Action<List<MultimediaObject>> handlernew =
            new Action<List<MultimediaObject>>(delegate(List<MultimediaObject> list) { });
        Debug.Log(" Do myEx 72:  "+ randomIds[exhibitNumber]);
        //Debug.Log(api.isActiveAndEnabled);
        Debug.Log(myApi.isActiveAndEnabled);
        Debug.Log("myapi  "+ myApi.name);
        
        // There should be searched for segmentIds instead ob fixed Objectids _1 hardcoding unnecessary
        myApi.RequestMoreLikeThisAndThen(QueryFactory.buildMoreLikeThisQuery(randomIds[exhibitNumber]+"_1"),handlernew); 
        yield return new WaitUntil(myApi.HasFinished);
        //List<MultimediaObject> localObjectList = myApi.GetMultimediaObjects();
        //myApi.getObjectsResult().content[0].objectId
        similarIds = myApi.GetMoreLikeThisResultIds(5);
        Debug.Log("ID after Query :      " + similarIds[0]);
        
        
        //Debug.Log("Can i change my room? from here  " + myexhibitionManager.GetRoomByIndex(0).RoomData.exhibits[0].id);
        
        // update images
        //myexhibitionManager.GetRoomByIndex(0).RoomData.exhibits = getExhibits(5, similarIds);
        
        
        
        //Debug.Log("Can i change my room?  to this" + myexhibitionManager.GetRoomByIndex(0).RoomData.exhibits[0].id);
        
        //myexhibitionManager.GetRoomByIndex(0).Walls[0].RestoreDisplayals();
        //myexhibitionManager.GetRoomByIndex(0).Walls[1].AttachExhibits();

        //myexhibitionManager.GetRoomByIndex(0).Walls[0].WallData.exhibits = getExhibits(5, similarIds);

        Debug.Log("did i change now 101   " + myexhibitionManager.GetRoomByIndex(0).Walls[0].WallData.exhibits.Length);

        //myexhibitionManager.GetRoomByIndex(0).Walls[0].WallData.
        
        myexhibitionManager.GetRoomByIndex(0).Walls[0].WallData.exhibits = getExhibits(5, similarIds);
        
        //myexhibitionManager.GetRoomByIndex(0).PopulateWalls();
        myexhibitionManager.GetRoomByIndex(0).DeleteOldDisp();
        
        
    
        
        
        
        //yield return new WaitForSeconds(1);
    }

    public CineastApi getApi()
    {
        return GetComponent<CineastApi>();
    }

    private void setTestingBool()
    {
        
        testBool = true;
    }


    private IEnumerator collision()
    {

        for (;;)
        {
            String a = myexhibitionManager.GetRoomByIndex(0).Walls[0].Displayals[0].id;
            Debug.Log("DO DO DO collision method  " + a);
            yield return new WaitForSecondsRealtime(.5f);
        }
   

    }
    
    // ExhibitionManager.ResoreExhibits zum Bilder update spöter!!!

    private void testLog()
    {
        
        Debug.Log("DO DO DO  testLog GOT TRIGGERED " );
        testBool = false;
    }
    
    
    

    private IEnumerator getCineastImg(int numb)
    {
        CineastApi api = GetComponent<CineastApi>();
        Action<List<MultimediaObject>> handlernew =
            new Action<List<MultimediaObject>>(delegate(List<MultimediaObject> list) { });


    
        api.RequestIds(numb,handlernew);
        yield return new WaitForSecondsRealtime(2);
        randomIds = api.GetRandomObjectIds();

        String url = api.GetRandomObjectIds()[0];
        //url = api.GetMultimediaObjects()[3].id;
        //url = CineastUtils.GetImageUrlbyID(url);

        
        //Debug.Log("getConeastImg" + randomIds[0]);

        createExhibition(numb);
        
        yield return new WaitForSeconds(2);
        
        // for some reason I cant make the Boxcolliders as Trigger in creation, therefor its done here
        makeCollidersTriggers(randomIds.Count);
        
        //String a = myexhibitionManager.GetRoomByIndex(0).Walls[0].Displayals[0].id;
        //Debug.Log("DO DO DO   " + a);

        
        //String b = myexhibitionManager.GetRoomByIndex(0).Walls[0].Displayals[0].GetComponent<Plane>().normal.ToString(); //GetComponent<MeshCollider>().isTrigger;

        //String b = myexhibitionManager.GetRoomByIndex(0).Walls[0].Displayals[0].GetExhibit().path;

        //Debug.Log("DO DO  path" + b);

        //Collider lookForaCollider = myexhibitionManager.GetRoomByIndex(0).Walls[0].Displayals[0].GetComponent<BoxCollider>();
        //String Collidername=lookForaCollider.name;
        
        //Debug.Log("is trigger?" +lookForaCollider.isTrigger);

        //lookForaCollider.isTrigger = true;

        //Debug.Log(" Collider tag  " +lookForaCollider.tag);
        
        //Debug.Log("is trigger now?  " +lookForaCollider.isTrigger);
        
        //lookForaCollider.
        
        //Debug.Log(" This is the collider name  " + Collidername);
        //OnTriggerEnter(lookForaCollider);
        


        //yield return true;
        // set loaded image
    }
    
    //might be useful
    /*private void OnTriggerEnter(Collider collider)
    {
        if (!useTag || useTag && collider.gameObject.tag == tagFilter)
            Destroy(collider.gameObject.transform.root.gameObject);
    }*/

    private void makeCollidersTriggers(int amountColliders)
    {
        for (int i = 0; i < amountColliders; i++)
        {
            myexhibitionManager.GetRoomByIndex(0).Walls[0].Displayals[i].GetComponent<BoxCollider>().isTrigger=true;
        }
    }



    private void createExhibition(int numb)
    {
       
        // get the random Images from Cineast uses int parameter for number of images
        Exhibit[] arr = getExhibits(numb,randomIds);
        Exhibit[] exarrEmpty = new Exhibit[0];
        

        //Create four Walls

        DefaultNamespace.VREM.Model.Wall wallone = new DefaultNamespace.VREM.Model.Wall
        {
            color = new Vector3(0.7f, 0.7f, 0.7f), texture = "Fabric02Material", direction = "NORTH", exhibits = arr
        };

        DefaultNamespace.VREM.Model.Wall walltwo = new DefaultNamespace.VREM.Model.Wall
        {
            color = new Vector3(0.7f, 0.7f, 0.7f), texture = "Fabric02Material", direction = "EAST",
            exhibits = exarrEmpty
        };
        DefaultNamespace.VREM.Model.Wall wallthree = new DefaultNamespace.VREM.Model.Wall
        {
            color = new Vector3(0.7f, 0.7f, 0.7f), texture = "Fabric02Material", direction = "SOUTH",
            exhibits = exarrEmpty
        };
        DefaultNamespace.VREM.Model.Wall wallfour = new DefaultNamespace.VREM.Model.Wall
        {
            color = new Vector3(0.7f, 0.7f, 0.7f), texture = "Fabric02Material", direction = "WEST",
            exhibits = exarrEmpty
        };


        // Create room wth these four walls

        DefaultNamespace.VREM.Model.Wall[] wallar = {wallone, walltwo, wallthree, wallfour};
        DefaultNamespace.VREM.Model.Room myFirstRoom = new DefaultNamespace.VREM.Model.Room
        {
            walls = wallar,
            size = new Vector3(16, 6, 10),
            floor = "NWoodFloor",
            ceiling = "Fabric02Material",
            text = "TExtofRoom",
            position = new Vector3(0, 0, 0),
            entrypoint = new Vector3(0.7f, 0.7f, 0.7f)
        };
        // what is z?


   
        DefaultNamespace.VREM.Model.Room[] myroomArr = {myFirstRoom};
        

        // create an Exhibition with the created room
        
        Exhibition ex = new Exhibition
            {id = "myid1", name = "myExhibition1", description = "myNoDesc", rooms = myroomArr};


        myEx = new Exhibition();
        myEx = ex;
        myexhibitionManager = new ExhibitionManager(ex);
        myexhibitionManager.GenerateExhibition();
       
        
        //collision();
        ;
    }


    

/**
 * Takes an Int and returns an Exhibit[] with filled with the specific number of Exhibits
 */
    private static Exhibit[] getExhibits(int number, List<String> ids)
    {
        Exhibit[] re= new Exhibit[number];
        List<String> id = ids;
        Exhibit add;
        int pos = 2;
        int name = 1;
        
        for (int i = 0; i < number; i++)
        {
            //Debug.Log("print iteraror" + i);
           
            var exhibitToAdd = new Exhibit
            {
                path = CineastUtils.GetImageUrlbyID(id[i]),
                name = name.ToString(),
                position = new Vector3(pos, 3, 1),
                size = new Vector3(2, 2, 2),
                id = ""+i,
                type = "IMAGE",
                light = false,
                description = ""
            };
            pos += 3;
            name++;
            re[i] = exhibitToAdd;
        }



        return re;
    }
    
    
}