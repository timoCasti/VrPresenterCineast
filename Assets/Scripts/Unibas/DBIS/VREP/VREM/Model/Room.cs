using System;
using UnityEngine;
using World;

namespace DefaultNamespace.VREM.Model
{
    [Serializable]
    public class Room
    {
        public string ambient;
        public string ceiling;
        public Vector3 entrypoint;

        public Exhibit[] exhibits;

        public string floor;
        public Vector3 position;
        public Vector3 size;

        public string text;
        public Wall[] walls;


        public string GetURLEncodedAudioPath()
        {
            if (!string.IsNullOrEmpty(ambient))
                return ServerSettings.SERVER_ID + "content/get/" +
                       ambient.Substring(0).Replace("/", "%2F").Replace(" ", "%20");
            return null;
        }

        public Wall GetWall(WallOrientation orientation)
        {
            foreach (var wall in walls)
            {
                var wor = (WallOrientation) Enum.Parse(typeof(WallOrientation), wall.direction, true);
                if (wor.Equals(orientation)) return wall;
            }

            return null;
        }
    }
}