﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CineastUnityInterface.CineastAPI.Query;
using CineastUnityInterface.CineastAPI.Result;
using UnityEngine;

namespace CineastUnityInterface.CineastAPI
{
    /**
     * Utilities related to cineast API.
     */
    public static class CineastUtils
    {
        /**
         * The key of meta 'latitude'
         * */
        public const string LATITUDE_KEY = "latitude";

        /**
         * The key of meta 'longitude'
         */
        public const string LONGITUDE_KEY = "longitude";

        /**
         * The key of meta 'datetime'
         */
        public const string DATETIME_KEY = "datetime";
        public const string BEARING_KEY = "bearing";

        public static readonly string[] KNOWN_KEYS = {LATITUDE_KEY, LONGITUDE_KEY, DATETIME_KEY, BEARING_KEY};


        public static readonly CategoryRatio QUERY_BY_EXAMPLE_EDGE_GLOBALCOL =
            new CategoryRatio(new[] {TermsObject.EDGE_CATEGORY, TermsObject.GLOBALCOLOR_CATEGORY}, new[] {0.5, 0.5});

        public static readonly CategoryRatio QBE_GLOBAL_LOCAL_EDGE_EQUAL_RATIO =
            new CategoryRatio(
                new[] {TermsObject.EDGE_CATEGORY, TermsObject.GLOBALCOLOR_CATEGORY, TermsObject.LOCALCOLOR_CATEGORY},
                new[] {1d / 3d, 1d / 3d, 1d / 3d});

        public static CineastConfiguration Configuration { get; set; }

        public static CategoryRatio CreateUniformRatio(string[] categories)
        {
            var cr = new CategoryRatio();

            foreach (var category in categories) cr.AddWeight(category, 1d / categories.Length);

            return cr;
        }

        public static CategoryRatio CreateUniformRatio(SimilarQuery query)
        {
            var cr = new CategoryRatio();

            var categories = new List<string>();

            foreach (var tc in query.containers)
            foreach (var to in tc.terms)
                categories.AddRange(to.categories);

            foreach (var category in categories) cr.AddWeight(category, 1d / categories.Count);

            return cr;
        }

        /**
         * Creates an object of a given metadataobject and sets the meta data
         * 
         * Currently is only lat/lon supported
         * 
         * @param meta - the meta data object.
         * 
         * */
        public static MultimediaObject ConvertFrom(MetaDataObject meta)
        {
            var output = new MultimediaObject();
            output.id = meta.objectId;
            output.AddMetaData(meta);
            return output;
        }

        public static MultimediaObject ConvertFrom(CineastObject obj)
        {
            var output = new MultimediaObject();
            output.id = obj.objectId;
            output.name = obj.name;
            output.path = obj.path;
            return output;
        }

        public static List<MultimediaObject> Convert(MetaDataObject[] metas)
        {
            var objects = new List<MultimediaObject>();

            foreach (var meta in metas)
            {
                var obj = ConvertFrom(meta);
                if (objects.Contains(obj))
                    objects.Find(o => o.Equals(obj)).Merge(obj);
                else
                    objects.Add(obj);
            }

            return objects;
        }

        public static List<MultimediaObject> Convert(CineastObject[] objects)
        {
            var list = new List<MultimediaObject>();
            foreach (var obj in objects)
            {
                var mmo = ConvertFrom(obj);
                if (list.Contains(mmo))
                    list.Find(o => o.Equals(mmo)).Merge(mmo);
                else
                    list.Add(mmo);
            }

            return list;
        }


        /**
         * Generates a WWW object with given params
         * 
         * @param url - A string which represents the url
         * @param json - The json data to send, as a string
         * 
         */
        public static WWW GenerateJSONPostRequest(string url, string json)
        {
            var headers = new Hashtable();
            headers.Add("Content-Type", "application/json");
            Debug.Log("Target URL: " + url + "\nPost data:\n" + json);
            var postData = Encoding.ASCII.GetBytes(json.ToCharArray());
            return new WWW(url,
                postData);
        }


        // created for get Random ids

        public static WWW GenerateJSONGetRequest(string url)
        {
            var headers = new Hashtable();
            headers.Add("Content-Type", "application/json");
            //Debug.Log("Target URL: " + url + "\nPost data:\n" + "Keine");
            //byte[] postData = Encoding.ASCII.GetBytes(json.ToCharArray());
            return new WWW(url); //,postData);
        }


        /**
         * Builds a SimilarQuery and sends a request to the specified url.
         * 
         * @param url - The url to the server
         * @param lat - The latitude of the query
         * @param lon - The longitude of the query
         */
        public static WWW BuildSimilarRequest(string url, double lat, double lon)
        {
            var sQuery = QueryFactory.BuildSpatialSimilarQuery(lat, lon);
            return GenerateJSONPostRequest(url, JsonUtility.ToJson(sQuery));
        }

        public static WWW BuildSimilarRequest(string url, string utcTime)
        {
            var sq = QueryFactory.BuildTemporalSimilarQuery(utcTime);
            return GenerateJSONPostRequest(url, JsonUtility.ToJson(sq));
        }

        // new for more like this query

        public static WWW BuildMoreLikeThisRequest(string url, MoreLikeThisQuery query)
        {
            return GenerateJSONPostRequest(url, JsonUtility.ToJson(query));
        }

        // new for Rondom id
        public static WWW BuildRandomRequest(string url) //, GetRandomIds query)
        {
            return GenerateJSONGetRequest(url); //, JsonUtility.ToJson(query));
        }


        public static WWW BuildSimilarRequest(string url, SimilarQuery query)
        {
            return GenerateJSONPostRequest(url, JsonUtility.ToJson(query));
        }

        /**
         * Extracts a string array containing only the Ids (aka the keys) of the given content object array.
         * 
         * @param contentObjects - The array of content objects to extract the keys from.
         * */
        public static string[] ExtractIdArray(ContentObject[] contentObjects)
        {
            var list = new List<string>();
            foreach (var obj in contentObjects)
                // TODO: At later stage: check value (aka score)
                list.Add(obj.key);
            return list.ToArray();
        }

        /**
         * Extracts a string array containing only the objectIds of the given segment object array.
         * 
         * @param segemntObjects - The array of segment objects to extract the keys from.
         * 
         */
        public static string[] ExtractIdArray(SegmentObject[] segmentObjects)
        {
            var list = new List<string>();
            foreach (var obj in segmentObjects) list.Add(obj.objectId);
            return list.ToArray();
        }

        public static ContentObject[] ExtractContentObjects(SimilarResult result)
        {
            return result.results[0].content; // TODO: Check if not empty?
        }

        /**
         * Builds a IdsQuery and sends a request to the specified url.
         * 
         * @param url - The url to the server
         * @param ids - The array of segment ids to request more data from
         * */
        public static WWW BuildSegmentRequest(string url, string[] ids)
        {
            var query = new IdsQuery(ids);
            return GenerateJSONPostRequest(url, JsonUtility.ToJson(query));
        }

        /**
         * Builds a MetaDataQuery and sends a request to the specified url.
         * 
         * @param url - the url to the server
         * @param objectIds  -the array of object ids to request the metadata from
         */
        public static WWW BuildMetadataRequest(string url, string[] objectIds)
        {
            var query = new ObjectQuery(objectIds);
            return GenerateJSONPostRequest(url, JsonUtility.ToJson(query));
        }

        /**
         * Extracts all the ids for a given MultimediaObject array.
         * 
         */
        public static string[] ExtractIdArray(MultimediaObject[] objects)
        {
            var ids = new List<string>();
            foreach (var obj in objects) ids.Add(obj.id);
            return ids.ToArray();
        }

        public static WWW BuildObjectsRequest(string url, string[] ids)
        {
            var query = new IdsQuery(ids);
            return GenerateJSONPostRequest(url, JsonUtility.ToJson(query));
        }

        // new Method for Image retrieval over objectid
        public static string GetImageUrlbyID(string id)
        {
            return Configuration.cineastHost + "/api/v1/get/objects/" + id;
        }

        public static string GetImageUrl(MultimediaObject mmo)
        {
            return Configuration.imagesHost + "/images/" + mmo.path;
        }

        public static string GetThumbnailUrl(MultimediaObject mmo)
        {
            return Configuration.imagesHost + "thumbnails/" + mmo.id + "/" + mmo.id + "_1.png";
        }

        /**
         * -1 == empty similar result
         * -2 == empty result object
         * -3 == not found
         */
        public static int GetIndexOf(MultimediaObject needle, SimilarResult haystack)
        {
            if (haystack.IsEmpty()) return -1;
            foreach (var resultObject in haystack.results)
            {
                if (resultObject.IsEmpty()) return -2;
                foreach (var contentObject in resultObject.content)
                    if (contentObject.key.Equals(needle.id + "_1")) // FIX hardcoded segment id
                        return Array.IndexOf(resultObject.content, contentObject);
            }

            return -3;
        }

        /// <summary>
        ///     Converts the given year (numerical value) to a ISO8601 conform timestamp.
        ///     This conversion isn't smart and doesn't check for negative values or too large ones.
        /// </summary>
        /// <param name="year">The year as a numerical value (usually a 4 digit, positive integer)</param>
        /// <returns>A ISO8601 conform timestamp string, set to January the first at noon in this year. No timezone specified</returns>
        public static string ConvertYearToISO8601(int year)
        {
            return ConvertToISO8601(year, 1, 1, 12, 0, 0);
        }

        /// <summary>
        ///     Converts the given time specification to a ISO8601 conform timestamp representation.
        ///     This conversion isn't smart and doesn't perfrom any sanity checks (e.g. 0 &lt; minutes &lt; 59 )
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="dayOfMonth"></param>
        /// <param name="hours"></param>
        /// <param name="minutes"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static string ConvertToISO8601(int year, int month, int dayOfMonth, int hours, int minutes, int seconds)
        {
            return string.Format("{0:D4}-{1:D2}-{2:D2}T{3:D2}:{4:D2}:{5:D2}Z", year, month, dayOfMonth, hours, minutes,
                seconds); // year-month-day[THH:MM:SSZ]
        }

        // JsonHelper to read Arrays into Json
        public static class JsonHelper
        {
            public static T[] FromJson<T>(string json)
            {
                var wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
                return wrapper.Items;
            }

            public static string ToJson<T>(T[] array)
            {
                var wrapper = new Wrapper<T>();
                wrapper.Items = array;
                return JsonUtility.ToJson(wrapper);
            }

            public static string ToJson<T>(T[] array, bool prettyPrint)
            {
                var wrapper = new Wrapper<T>();
                wrapper.Items = array;
                return JsonUtility.ToJson(wrapper, prettyPrint);
            }

            [Serializable]
            private class Wrapper<T>
            {
                public T[] Items;
            }
        }
    }
}