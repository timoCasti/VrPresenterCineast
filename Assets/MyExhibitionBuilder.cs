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
    private ExhibitionManager myexhibitionManager;
    private Exhibition myEx;
    public Vector3 LobbySpawn = new Vector3(0, -9, 0);

    private List<String> randomIds;




    // Use this for initialization
    void Start()
    {
      
        StartCoroutine(getCineastImg(5));
        
        //getCineastImg(5);

    }


    // Update is called once per frame
    void Update()
    {
       // StartCoroutine(collision());
    }


    private IEnumerator collision()
    {

        for (;;)
        {
            String a = myexhibitionManager.GetRoomByIndex(0).Walls[0].Displayals[0].id;
            Debug.Log("DO DO DO   " + a);
            yield return new WaitForSecondsRealtime(.5f);
        }
   

    }
    
    // ExhibitionManager.ResoreExhibits zum Bilder update spöter!!!

    private void testLog()
    {
        String a = myexhibitionManager.GetRoomByIndex(0).Walls[0].Displayals[0].id;
        Debug.Log("DO DO DO   " + a);
    }
    
    
    

    private IEnumerator getCineastImg(int numb)
    {
        CineastApi api = GetComponent<CineastApi>();
        Action<List<MultimediaObject>> handlernew =
            new Action<List<MultimediaObject>>(delegate(List<MultimediaObject> list) { });


        //api.RequestMoreLikeThisAndThen(QueryFactory.buildMoreLikeThisQuery("i_U2Lu5aary0xm4V4X_1"),handlernew);

        api.RequestIds(numb,handlernew);
        yield return new WaitForSecondsRealtime(2);
        randomIds = api.GetRandomObjectIds();

        String url = api.GetRandomObjectIds()[0];
        //url = api.GetMultimediaObjects()[3].id;
        //url = CineastUtils.GetImageUrlbyID(url);

        
        //Debug.Log("getConeastImg" + randomIds[0]);

        createExhibition(numb);
        
        yield return new WaitForSeconds(2);
        
        String a = myexhibitionManager.GetRoomByIndex(0).Walls[0].Displayals[0].id;
        Debug.Log("DO DO DO   " + a);

        Boolean b=myexhibitionManager.GetRoomByIndex(0).Walls[0].Displayals[0].GetComponent<MeshCollider>().isTrigger;

        Debug.Log("DO DO  isTrigger " + b);


        //yield return true;
        // set loaded image
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
    private Exhibit[] getExhibits(int number, List<String> ids)
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
                id = "exhiID",
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