﻿using System.Collections.Generic;
using DefaultNamespace.VREM.Model;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class BuildingManager : MonoBehaviour
{
    private Exhibition _exhibition;
    private int currentPosition = 0;

    public float Offset = 5f;
    private Room roomLogic;

    private GameObject roomObject;

    public GameObject RoomPrefab;


    public float RoomSize = 10f;

    private readonly List<Room> theRooms = new List<Room>();

    private int GetNextPosition(int pos)
    {
        return (pos + 1) % _exhibition.rooms.Length;
    }

    private int GetPreviousPosition(int pos)
    {
        return (pos - 1 + _exhibition.rooms.Length) % _exhibition.rooms.Length;
    }


    private Vector3 CalculateRoomPosition(DefaultNamespace.VREM.Model.Room room)
    {
        float x = room.position.x, y = room.position.y, z = room.position.z;
        return new Vector3(x * RoomSize + x * Offset, y * RoomSize + y * Offset, z * RoomSize + z * Offset);
    }

    public void BuildRoom(DefaultNamespace.VREM.Model.Room room)
    {
        var goRoom = Instantiate(RoomPrefab);
        goRoom.transform.position = CalculateRoomPosition(room);
        var roomLogic = goRoom.GetComponent<Room>();
        roomLogic.Populate(room);
    }

    private Room CreateRoom(DefaultNamespace.VREM.Model.Room room)
    {
        var goRoom = Instantiate(RoomPrefab);
        goRoom.transform.position = CalculateRoomPosition(room);
        var roomLogic = goRoom.GetComponent<Room>();
        return roomLogic;
    }


    public void Create(Exhibition exhibition)
    {
        _exhibition = exhibition;
        foreach (var room in exhibition.rooms)
        {
            var r = CreateRoom(room);
            theRooms.Add(r);
            r.Populate(room);
        }

        for (var i = 0; i < exhibition.rooms.Length; i++)
        {
            var r = theRooms[i];
            r.SetNextRoom(theRooms[GetNextPosition(i)]);
            var tp = r.gameObject.transform.Find("TeleportPoint");
            if (tp != null && theRooms.Count > 1)
            {
                var porter = tp.GetComponent<TeleportPoint>();
                var dest = CalculateRoomPosition(r.GetNextRoom().GetRoomModel());
                porter.destination = dest;
                r.SetPrevRoom(theRooms[GetPreviousPosition(i)]);
            }
        }
    }
}