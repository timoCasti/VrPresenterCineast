using System;
using System.Collections;
using System.Collections.Generic;
using CineastUnityInterface.CineastAPI;
using DefaultNamespace;
using DefaultNamespace.VREM;
using DefaultNamespace.VREM.Model;
using InGamePaint;
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

    private static Boolean isFinished;

    public static float TimeForTrigger;

    public static Boolean Masterpiece;


    // Use this for initialization
    void Start()
    {
        StartCoroutine(getCineastImg(5));

    }

    public static IEnumerator getMorelikeThisOne(int exhibitNumber)
    {
        CineastApi myApi = CineastApi.FindObjectOfType<CineastApi>();
        Action<List<MultimediaObject>> handlernew =
            new Action<List<MultimediaObject>>(delegate(List<MultimediaObject> list) { });

        //feature list for MLT query
        String[] categories = {"globalcolor", "localcolor", "edge"};

        myApi.RequestMoreLikeThisAndThen(
            QueryFactory.buildMoreLikeThisQuery(categories, randomIds[exhibitNumber] + "_1"), handlernew);

        yield return new WaitUntil(myApi.HasFinished);


        similarIds = myApi.GetMoreLikeThisResultIds(5);

        randomIds = similarIds;

        myexhibitionManager.GetRoomByIndex(0).Walls[0].WallData.exhibits = getExhibits(5, similarIds);

        myexhibitionManager.GetRoomByIndex(0).DeleteOldandUpdate();

        isFinished = true;
    }

    public static void resetMasterpiece()
    {
        myexhibitionManager.GetRoomByIndex(0).DeleteMasterpieceAndUpdate();
    }

    public static IEnumerator getMorelikeMyMasterpiece()
    {
        CineastApi myApi = CineastApi.FindObjectOfType<CineastApi>();
        Action<List<MultimediaObject>> handlernew =
            new Action<List<MultimediaObject>>(delegate(List<MultimediaObject> list) { });
        String imgData;
        Paintable[] p = GameObject.FindObjectsOfType<Paintable>();

        imgData = p[0].GetBase64();
        byte[] bytes;
        String b64;

        imgData = "data:image/jpeg;base64," + imgData;


        // features for Masterpiece query
        String[] categories = {"globalcolor", "localcolor"};

        myApi.RequestSimilarThanMasterpiece(QueryFactory.BuildMultiCategoryQuery(categories, imgData), handlernew);


        yield return new WaitUntil(myApi.HasFinished);
        yield return similarIds = myApi.GetMoreLikeThisResultIds(5);
        randomIds = similarIds;

        myexhibitionManager.GetRoomByIndex(0).Walls[0].WallData.exhibits = getExhibits(5, similarIds);

        myexhibitionManager.GetRoomByIndex(0).DeleteOldandUpdate();
    }


    private IEnumerator getCineastImg(int numb)
    {
        CineastApi api = GetComponent<CineastApi>();
        Action<List<MultimediaObject>> handlernew =
            new Action<List<MultimediaObject>>(delegate(List<MultimediaObject> list) { });

        api.RequestIds(numb, handlernew);

        yield return new WaitUntil(api.HasFinished);
        randomIds = api.GetRandomObjectIds();


        createExhibition(numb);

        TimeForTrigger = Time.time;
    }


    private void createExhibition(int numb)
    {
        // get the random Images from Cineast uses int parameter for number of images
        Exhibit[] arr = getExhibits(numb, randomIds);
        Exhibit[] exarrEmpty = new Exhibit[0];
        Exhibit[] Canvas = createMyCanvas(randomIds);


        //Create four Walls

        DefaultNamespace.VREM.Model.Wall wallone = new DefaultNamespace.VREM.Model.Wall
        {
            color = new Vector3(0.7f, 0.7f, 0.7f), texture = "Fabric02Material", direction = "NORTH", exhibits = arr
        };

        DefaultNamespace.VREM.Model.Wall walltwo = new DefaultNamespace.VREM.Model.Wall
        {
            color = new Vector3(0.7f, 0.7f, 0.7f), texture = "Fabric02Material", direction = "EAST",
            exhibits = Canvas
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
            size = new Vector3(8, 3, 10),
            floor = "NWoodFloor",
            ceiling = "Fabric02Material",
            text = "TExtofRoom",
            position = new Vector3(0, 0, 0),
            entrypoint = new Vector3(0.7f, 0.7f, 0.7f)
        };


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


    private static Exhibit[] createMyCanvas(List<String> id)
    {
        Exhibit[] re;

        re = new[]
            {
                new Exhibit
                {
                    name = "Masterpiece",
                    position = new Vector3(4, 1.5f, 1),
                    size = new Vector3(1f, 1f, 1f),
                    id = "1000",
                    type = "IMAGE",
                    light = false,
                    description = ""
                }
            }
            ;

        return re;
    }


/**
 * Takes an Int and returns an Exhibit[] with filled with the specific number of Exhibits
 */
    private static Exhibit[] getExhibits(int number, List<String> ids)
    {
        Exhibit[] re = new Exhibit[number];
        List<String> id = ids;
        Exhibit add;
        float pos = 1;
        int name = 1;

        for (int i = 0; i < number; i++) {
            var exhibitToAdd = new Exhibit
            {
                path = CineastUtils.GetImageUrlbyID(id[i]),
                name = name.ToString(),
                position = new Vector3(pos, 1.5f, 1),
                size = new Vector3(1, 1, 1),
                id = "" + i,
                type = "IMAGE",
                light = false,
                description = ""
            };
            pos += 1.5f;
            name++;
            re[i] = exhibitToAdd;
        }


        return re;
    }

    public Boolean GetisFinished()
    {
        return isFinished;
    }
}