﻿using System;

namespace CineastUnityInterface.CineastAPI.Query
{
    [Serializable]
    public class TermsObject
    {
        public const string SPATIAL_CATEGORY = "spatial";
        public const string LOCATION_TYPE = "LOCATION";

        public const string TERMPORAL_CATEGORY = "temporal";
        public const string TIME_TYPE = "TIME";

        public const string ID_TYPE = "ID";


        public const string IMAGE_TYPE = "IMAGE";
        public const string GLOBALCOLOR_CATEGORY = "globalcolor";
        public const string LOCALCOLOR_CATEGORY = "localcolor";
        public const string EDGE_CATEGORY = "edge";
        public const string QUANTIZED_CATEGORY = "quantized";

        public string[] categories;

        public string data;
        public string type;


        public TermsObject(string type, string[] categories)
        {
            this.type = type;
            this.categories = categories;
        }

        public static TermsObject BuildLocationTermsObject(double latitude, double longitude)
        {
            var built = new TermsObject(LOCATION_TYPE, new[] {SPATIAL_CATEGORY});
            built.data = string.Format("[{0},{1}]", latitude, longitude);
            return built;
        }
    }
}