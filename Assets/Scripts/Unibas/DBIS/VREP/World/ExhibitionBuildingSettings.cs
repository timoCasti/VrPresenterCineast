using System;

namespace World
{
    /// <summary>
    ///     Class to store various exhibition building related settings.
    /// </summary>
    [Serializable]
    public class ExhibitionBuildingSettings
    {
        private static ExhibitionBuildingSettings _instance;

        private readonly float _roomOffset = 2f;

        private readonly float _wallOffset = 0.1f;

        public string StandardDisplayalPrefabName = "Displayal";

        public bool UseStandardDisplayalPrefab = true;

        /// <summary>
        ///     Positive offset between the wall and the displayal.
        /// </summary>
        public float WallOffset
        {
            get { return _wallOffset; }
        }

        public float RoomOffset
        {
            get { return _roomOffset; }
        }

        public static ExhibitionBuildingSettings Instance
        {
            get { return _instance ?? (_instance = new ExhibitionBuildingSettings()); }
        }
    }
}