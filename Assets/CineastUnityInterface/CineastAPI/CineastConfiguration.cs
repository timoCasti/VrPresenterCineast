using System.IO;
using UnityEngine;

namespace CineastUnityInterface.CineastAPI
{
    public class CineastConfiguration
    {
        public const string API_VERSION = "api/v1/";
        public const string SIMILAR_QUERY_URL = "find/segments/similar";
        public const string SEGMENTS_QUERY_URL = "find/segments/by/id";
        public const string METAS_QUERY_URL = "find/metadata/by/id";
        public const string OBJECT_QUERY_URL = "find/objects/by/id";

        public const string RANDID_QUERY_URL = "find/objects/all/image";

        public const string FILE_NAME = "cineast.config";

        public string cineastHost;
        public string imagesHost;

        public CineastConfiguration()
        {
        }

        public CineastConfiguration(string cineastHost, string imagesHost)
        {
            this.cineastHost = cineastHost;
            this.imagesHost = imagesHost;
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(cineastHost) || string.IsNullOrEmpty(imagesHost);
        }

        public string GetRandomIds()
        {
            return cineastHost + API_VERSION + RANDID_QUERY_URL;
        }

        public string FindSimilarSegmentsUrl()
        {
            return cineastHost + API_VERSION + SIMILAR_QUERY_URL;
        }

        public string FindSegmentsByIdUrl()
        {
            return cineastHost + API_VERSION + SEGMENTS_QUERY_URL;
        }

        public string FindMetadataUrl()
        {
            return cineastHost + API_VERSION + METAS_QUERY_URL;
        }

        public string FindObjectsUrl()
        {
            return cineastHost + API_VERSION + OBJECT_QUERY_URL;
        }

        public static bool HasConfig()
        {
            return File.Exists(GetFilePath());
        }

        public static CineastConfiguration Load()
        {
            if (!HasConfig()) throw new FileNotFoundException("Configuration not found", GetFilePath());

            var config = ReadJson<CineastConfiguration>(GetFilePath());

            // Sanatize
            if (!string.IsNullOrEmpty(config.cineastHost) && !config.cineastHost.EndsWith("/"))
                config.cineastHost += "/";

            if (!string.IsNullOrEmpty(config.imagesHost) && !config.imagesHost.EndsWith("/")) config.imagesHost += "/";

            return config;
        }

        private static void WriteJson(string json, string path)
        {
            var sw = File.CreateText(path);
            sw.WriteLine(json);
            sw.Flush();
            sw.Close();
        }

        private static T ReadJson<T>(string path)
        {
            var sr = File.OpenText(path);
            var content = sr.ReadToEnd();
            sr.Close();
            return JsonUtility.FromJson<T>(content);
        }

        private static string GetFilePath()
        {
#if UNITY_EDITOR
            return Application.dataPath + "/" + FILE_NAME;
#elif UNITY_ANDROID
                return Application.persistentDataPath + "/" + FILE_NAME;
#endif
        }

        public static void StoreEmpty()
        {
            WriteJson(JsonUtility.ToJson(new CineastConfiguration()), GetFilePath());
        }

        public static CineastConfiguration GetDefault()
        {
            return new CineastConfiguration("http://localhost:4567/", "http://localhost/");
        }
    }
}