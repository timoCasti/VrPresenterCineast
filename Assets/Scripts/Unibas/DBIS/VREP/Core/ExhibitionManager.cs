using System.Collections.Generic;
using DefaultNamespace;
using DefaultNamespace.VREM.Model;
using InGamePaint;
using Unibas.DBIS.VREP.World;
using UnityEngine;
using World;

namespace Unibas.DBIS.VREP.Core
{
    public class ExhibitionManager:MonoBehaviour
    {
        private readonly Exhibition _exhibition;

        private readonly List<CuboidExhibitionRoom> _rooms = new List<CuboidExhibitionRoom>();

        public ExhibitionManager(Exhibition exhibition)
        {
            _exhibition = exhibition;
        }

        public CuboidExhibitionRoom GetRoomByIndex(int index)
        {
            return _rooms[index];
        }

        public void RestoreExhibits()
        {
            _rooms.ForEach(r => r.RestoreWallExhibits());
        }

        private int GetNextRoomIndex(int pos)
        {
            return (pos + 1) % _exhibition.rooms.Length;
        }

        private int GetPreviousRoomIndex(int pos)
        {
            return (pos - 1 + _exhibition.rooms.Length) % _exhibition.rooms.Length;
        }

        private int GetRoomIndex(DefaultNamespace.VREM.Model.Room room)
        {
            for (var i = 0; i < _exhibition.rooms.Length; i++)
                if (room.Equals(_exhibition.rooms[i]))
                    return i;

            return -1;
        }

        private DefaultNamespace.VREM.Model.Room GetNext(DefaultNamespace.VREM.Model.Room room)
        {
            var pos = GetRoomIndex(room);
            if (pos == -1) return null;

            return _exhibition.rooms[GetNextRoomIndex(pos)];
        }

        private DefaultNamespace.VREM.Model.Room GetPrevious(DefaultNamespace.VREM.Model.Room room)
        {
            var pos = GetRoomIndex(room);
            if (pos == -1) return null;

            return _exhibition.rooms[GetPreviousRoomIndex(pos)];
        }

        public void GenerateExhibition()
        {
            foreach (var room in _exhibition.rooms)
            {
                var roomGameObject = ObjectFactory.BuildRoom(room);
                var exhibitionRoom = roomGameObject.GetComponent<CuboidExhibitionRoom>();
                _rooms.Add(exhibitionRoom);

                //if (VREPController.Instance.Settings.CeilingLogoEnabled)
                //{
                var pref = Resources.Load<GameObject>("Objects/unibas");
                var logo = Object.Instantiate(pref);
                logo.name = "UnibasLogo";
                logo.transform.SetParent(exhibitionRoom.transform, false);
                //logo.transform.localPosition = new Vector3(-1.493f, room.size.y-.01f, -0.642f); // manually found values
                logo.transform.localPosition =
                    new Vector3(-1.493f, room.size.y - .01f, 3.35f); // manually found values
                logo.transform.localRotation = Quaternion.Euler(new Vector3(90, 180));
                logo.transform.localScale = Vector3.one * 1000;
                //}

                
               // try add paintable canvas here

               // name= Displayal (1000)
               //var Canv = exhibitionRoom.Walls[1].Displayals.Find(displayal => displayal.name.Equals("Masterpiece"));
               //Debug.Log(exhibitionRoom.Walls[1].Displayals[0].name);
               //Debug.Log("found canvas" + Canv.name);
               
               var canv = exhibitionRoom.Walls[1].Displayals[0];
               var Canvas = canv.gameObject.transform.Find("MyCanvas").gameObject;
               //Debug.Log(Canvas.name);
               Canvas.gameObject.AddComponent<Paintable>();
               Canvas.gameObject.AddComponent<VrMoveable>();

               Canvas.gameObject.GetComponent<VrMoveable>().enabled = true;
               
               canv.gameObject.GetComponent<BoxCollider>().isTrigger = false;
               
               // Add Palette
               var pal = ObjectFactory.GetPalettePrefab();
               var palette =Instantiate(pal);
               palette.transform.SetParent(exhibitionRoom.transform,false);
               palette.name = "MyPalette";
               palette.transform.localPosition=new Vector3(3.99f,1.7f,-2);
               palette.transform.localRotation= Quaternion.Euler(new Vector3(90,-90,0));
               palette.transform.localScale=Vector3.one*0.2f;


            }

            // For teleporting, each room needs to be created.
            foreach (var room in _rooms) CreateAndAttachTeleporters(room);
        }
        
        // new Method to update images at the wall

        public void UpdateRoom(Room roomUpdate)
        {
            
            
        }
        


        private void CreateAndAttachTeleporters(CuboidExhibitionRoom room)
        {
            var index = GetRoomIndex(room.RoomData);
            var next = _rooms[GetNextRoomIndex(index)];
            var prev = _rooms[GetPreviousRoomIndex(index)];

            var nd = next.GetEntryPoint();
            var pd = prev.GetEntryPoint();

            var backPos = new Vector3(-.25f, 0, .2f);
            var nextPos = new Vector3(.25f, 0, .2f);

            // TODO Configurable TPBtnModel
            var model = new SteamVRTeleportButton.TeleportButtonModel(0.1f, .02f, 1f,
                TexturingUtility.LoadMaterialByName("NMetal"),
                TexturingUtility.LoadMaterialByName("NMetal"), TexturingUtility.LoadMaterialByName("NPlastic"));

            if (_exhibition.rooms.Length > 1)
            {
                // back teleporter
                var backTpBtn = SteamVRTeleportButton.Create(room.gameObject, backPos, pd, model
                    ,
                    Resources.Load<Sprite>("Sprites/UI/chevron-left"));

                backTpBtn.OnTeleportStart = room.OnRoomLeave;
                backTpBtn.OnTeleportEnd = prev.OnRoomEnter;

                // back teleporter
                var nextTpBtn = SteamVRTeleportButton.Create(room.gameObject, nextPos, nd,
                    model,
                    Resources.Load<Sprite>("Sprites/UI/chevron-right"));

                nextTpBtn.OnTeleportStart = room.OnRoomLeave;
                nextTpBtn.OnTeleportEnd = next.OnRoomEnter;
            }


            /*if (VREPController.Instance.Settings.StartInLobby)
            {
                var lobbyTpBtn = SteamVRTeleportButton.Create(room.gameObject, new Vector3(0, 0, .2f),
                    VREPController.Instance.LobbySpawn,
                    model,
                    "Lobby");
                lobbyTpBtn.OnTeleportStart = room.OnRoomLeave;
            }*/
        }
    }
}