using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unibas.DBIS.DynamicModelling.Models
{
    [Serializable]
    public class PolygonRoomModel : IModel
    {
        private readonly List<WallModel> _walls;
        public Material CeilingMaterial;
        public Material FloorMaterial;
        public Vector3 Position;

        public PolygonRoomModel(Vector3 position, List<WallModel> walls, Material floorMaterial,
            Material ceilingMaterial)
        {
            Position = position;
            _walls = walls;
            FloorMaterial = floorMaterial;
            CeilingMaterial = ceilingMaterial;
        }

        public WallModel[] GetWalls()
        {
            return _walls.ToArray();
        }

        public WallModel GetWallAt(int index)
        {
            return _walls[index];
        }

        public void Add(WallModel model)
        {
            _walls.Add(model);
        }
    }
}