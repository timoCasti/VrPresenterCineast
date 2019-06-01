﻿using System;
using System.Collections;
using System.Collections.Generic;
using CineastUnityInterface.CineastAPI.Query;
using CineastUnityInterface.CineastAPI.Result;
using UnityEngine;
using Random = System.Random;

namespace CineastUnityInterface.CineastAPI
{
    public class CineastApi : MonoBehaviour
    {
        private bool earlyBreak;

        private FilterEngine filterEngine;

        private bool finished;

        private WWW metaRequest;
        private MetaDataResult metaResult;
        private List<MultimediaObject> objectList;

        private WWW objectRequest;
        private ObjectsResult objectsResult;

        private Action<List<MultimediaObject>> queryFinishedCallback;
        private CineastObject[] randomCineastObjectArray;

        //for random ids
        private WWW randomIdResult;
        private List<string> randomObjectIds;

        private List<MultimediaObject> results;

        private WWW segmentRequest;
        private SegmentResult segmentResult;
        private WWW similarRequest;
        private SimilarResult similarResult;

        private void Awake()
        {
            filterEngine = new FilterEngine();
            if (CineastConfiguration.HasConfig()) {
                var config = CineastConfiguration.Load();
                if (!config.IsEmpty())
                    CineastUtils.Configuration = config;
                else
                    CineastUtils.Configuration = CineastConfiguration.GetDefault();
            }
            else {
                CineastConfiguration.StoreEmpty();
            }
        }

        public void RequestMoreLikeThisAndThen(MoreLikeThisQuery query, Action<List<MultimediaObject>> handler)
        {
            queryFinishedCallback = handler;
            StartCoroutine(ExecuteQueryMoreLikeThis(query));
        }

        public void RequestIds(int number, Action<List<MultimediaObject>> handler)
        {
            queryFinishedCallback = handler;
            StartCoroutine(ExecuteRandomId(number));
        }


        public void RequestSimilarAndThen(SimilarQuery query, Action<List<MultimediaObject>> handler)
        {
            queryFinishedCallback = handler;
            StartCoroutine(ExecuteQuery(query));
        }

        public void RequestWeightedSimilarAndThen(SimilarQuery query, CategoryRatio ratio,
            Action<List<MultimediaObject>> handler)
        {
            queryFinishedCallback = handler;
            StartCoroutine(ExecuteMultiQuery(query, ratio));
        }

        public void RequestSimilarThanMasterpiece(SimilarQuery query, Action<List<MultimediaObject>> handler)
        {
            queryFinishedCallback = handler;
            StartCoroutine(ExecuteQueryMoreLikeMasterpiece(query));
        }

        private IEnumerator ExecuteMultiQuery(SimilarQuery query, CategoryRatio ratio)
        {
            finished = false;
            // === SIMILAR ===
            // Initial SimilarQuery

            yield return similarRequest =
                CineastUtils.BuildSimilarRequest(CineastUtils.Configuration.FindSimilarSegmentsUrl(), query);


            // Parse response
            earlyBreak = !Parse(similarRequest.text, out similarResult);
            yield return similarResult;
            if (earlyBreak) yield break;


            // Check if empty
            if (similarResult.IsEmpty()) {
                earlyBreak = true;
                yield break; // Stop and 
            }

            var tempResult = CineastUtils.ExtractContentObjects(similarResult);

            if (ratio != null && similarResult.results.Length > 1) {
                foreach (var ro in similarResult.results) ContentObject.ArrayToStrig(ro.content);

                var merger = new ResultMerger();
                tempResult = merger.Merge(similarResult.results, ratio)
                    .ToArray();
                ContentObject.ArrayToStrig(tempResult);
            }

            // === SEGMENTS ===
            // segments
            yield return segmentRequest =
                CineastUtils.BuildSegmentRequest(CineastUtils.Configuration.FindSegmentsByIdUrl(),
                    CineastUtils.ExtractIdArray(tempResult));

            // parse response
            earlyBreak = !Parse(segmentRequest.text, out segmentResult);
            yield return segmentResult;
            if (earlyBreak) yield break;


            // === METAS ===
            yield return metaRequest =
                CineastUtils.BuildMetadataRequest(CineastUtils.Configuration.FindMetadataUrl(),
                    CineastUtils.ExtractIdArray(segmentResult.content));
            earlyBreak = !Parse(metaRequest.text, out metaResult);
            yield return metaResult;
            if (earlyBreak) yield break;

            // meta->mmo

            objectList = CineastUtils.Convert(metaResult.content);


            // === OBJECTS ===
            yield return objectRequest =
                CineastUtils.BuildObjectsRequest(CineastUtils.Configuration.FindObjectsUrl(),
                    CineastUtils.ExtractIdArray(objectList.ToArray()));

            yield return objectsResult = JsonUtility.FromJson<ObjectsResult>(objectRequest.text);


            // merge results
            var objects = CineastUtils.Convert(objectsResult.content);
            foreach (var mmo in objects)
                if (objectList.Contains(mmo))
                    objectList.Find(o => o.Equals(mmo)).Merge(mmo);


            results = new List<MultimediaObject>(objectList);

            // === WRAPUP ===
            foreach (var mmo in objectList) mmo.resultIndex = CineastUtils.GetIndexOf(mmo, similarResult) + 1;


            // === SORT LIST ===
            objectList.Sort(
                Comparison);

            List<MultimediaObject> transferList;
            if (filterEngine != null)
                transferList = filterEngine.ApplyFilters(objectList);
            else
                transferList = objectList;


            // cleanup
            finished = true;
            if (queryFinishedCallback != null) queryFinishedCallback.Invoke(transferList);

            yield return true;
        }


        private IEnumerator ExecuteQuery(SimilarQuery query)
        {
            finished = false;
            // === SIMILAR ===
            // Initial SimilarQuery
            yield return similarRequest =
                CineastUtils.BuildSimilarRequest(CineastUtils.Configuration.FindSimilarSegmentsUrl(), query);


            // Parse response
            earlyBreak = !Parse(similarRequest.text, out similarResult);
            yield return similarResult;
            if (earlyBreak) yield break;


            // Check if empty
            if (similarResult.IsEmpty()) {
                earlyBreak = true;
                yield break; // Stop and 
            }

            // === SEGMENTS ===
            // segments
            yield return segmentRequest =
                CineastUtils.BuildSegmentRequest(CineastUtils.Configuration.FindSegmentsByIdUrl(),
                    CineastUtils.ExtractIdArray(CineastUtils.ExtractContentObjects(similarResult)));

            // parse response
            earlyBreak = !Parse(segmentRequest.text, out segmentResult);
            yield return segmentResult;
            if (earlyBreak) yield break;


            // === METAS ===
            yield return metaRequest =
                CineastUtils.BuildMetadataRequest(CineastUtils.Configuration.FindMetadataUrl(),
                    CineastUtils.ExtractIdArray(segmentResult.content));
            earlyBreak = !Parse(metaRequest.text, out metaResult);
            yield return metaResult;
            if (earlyBreak) yield break;

            // meta->mmo

            objectList = CineastUtils.Convert(metaResult.content);


            // === OBJECTS ===
            yield return objectRequest =
                CineastUtils.BuildObjectsRequest(CineastUtils.Configuration.FindObjectsUrl(),
                    CineastUtils.ExtractIdArray(objectList.ToArray()));

            yield return objectsResult = JsonUtility.FromJson<ObjectsResult>(objectRequest.text);


            // merge results
            var objects = CineastUtils.Convert(objectsResult.content);
            foreach (var mmo in objects)
                if (objectList.Contains(mmo))
                    objectList.Find(o => o.Equals(mmo)).Merge(mmo);


            results = new List<MultimediaObject>(objectList);

            // === WRAPUP ===
            foreach (var mmo in objectList) mmo.resultIndex = CineastUtils.GetIndexOf(mmo, similarResult) + 1;


            // === SORT LIST ===
            objectList.Sort(
                Comparison);

            List<MultimediaObject> transferList;
            if (filterEngine != null)
                transferList = filterEngine.ApplyFilters(objectList);
            else
                transferList = objectList;


            // cleanup
            finished = true;
            if (queryFinishedCallback != null) queryFinishedCallback.Invoke(transferList);

            yield return true;
        }

        /* new for Randomid #5
            creates an List<String> randomIdlist with all existing Objectids
            selects 5
         */
        private IEnumerator ExecuteRandomId(int number)
        {
            finished = false;
            // Number cant be higher than 50
            int num = number;
            if (num > 50) {
                num = 5;
            }
            // == RandomIds ==

            // JSOn Utlilty doesnt suppport Arrays

            yield return randomIdResult = CineastUtils.BuildRandomRequest(CineastUtils.Configuration.GetRandomIds());

            var resultString = randomIdResult.text;

            // fix Json manually
            resultString = "{\"Items\":" + resultString + "}";


            // use helperfunction to read array to json
            yield return randomCineastObjectArray = CineastUtils.JsonHelper.FromJson<CineastObject>(resultString);

            var random = new Random();
            var len = randomCineastObjectArray.Length;
            var randomfive = new List<int>();
            for (var k = 0; k < number; k++) {
                var ran = random.Next(len);
                while (randomfive.Contains(ran)) ran = random.Next(len);

                randomfive.Add(ran);
            }
            //
            //randomfive.ForEach(el => Debug.Log(el));

            randomObjectIds = new List<string>();

            // Save all Random objectId ( String) into List<String> randomObjecids 
            for (var f = 0; f < num; f++) randomObjectIds.Add(randomCineastObjectArray[randomfive[f]].objectId);


            finished = true;
            yield return randomObjectIds;
        }

        // new for more like this

        private IEnumerator ExecuteQueryMoreLikeThis(MoreLikeThisQuery query)
        {
            // Since api is used several times it has to be set false on call
            finished = false;


            // === SIMILAR ===
            // Initial SimilarQuery
            yield return similarRequest =
                CineastUtils.BuildMoreLikeThisRequest(CineastUtils.Configuration.FindSimilarSegmentsUrl(), query);


            // Parse response
            earlyBreak = !Parse(similarRequest.text, out similarResult);
            yield return similarResult;
            if (earlyBreak) yield break;
            

            // Check if empty
            if (similarResult.IsEmpty()) {
                earlyBreak = true;
                yield break; // Stop and 
            }

            // === SEGMENTS ===
            // segments
            yield return segmentRequest =
                CineastUtils.BuildSegmentRequest(CineastUtils.Configuration.FindSegmentsByIdUrl(),
                    CineastUtils.ExtractIdArray(CineastUtils.ExtractContentObjects(similarResult)));

            // parse response
            earlyBreak = !Parse(segmentRequest.text, out segmentResult);
            yield return segmentResult;
            if (earlyBreak) yield break;


            yield return objectRequest =
                CineastUtils.BuildObjectsRequest(CineastUtils.Configuration.FindObjectsUrl(),
                    CineastUtils.ExtractIdArray(segmentResult.content));

            yield return objectsResult = JsonUtility.FromJson<ObjectsResult>(objectRequest.text);

            //Debug.Log("ObjectRes:" + objectsResult.content[0].path);


// new trsy
            objectList = CineastUtils.Convert(objectsResult.content);
            //Debug.Log("New Test 474" + objectList[0].id);


            // merge results
            var objects = CineastUtils.Convert(objectsResult.content);
            foreach (var mmo in objects)
                if (objectList.Contains(mmo))
                    objectList.Find(o => o.Equals(mmo)).Merge(mmo);

            //Debug.Log("Object id 487" + objectList[0].id);

            results = new List<MultimediaObject>(objectList);

            // === WRAPUP ===
            foreach (var mmo in objectList) mmo.resultIndex = CineastUtils.GetIndexOf(mmo, similarResult) + 1;


            // === SORT LIST ===
            objectList.Sort(
                Comparison);

            List<MultimediaObject> transferList;
            if (filterEngine != null)
                transferList = filterEngine.ApplyFilters(objectList);
            else
                transferList = objectList;


            //Debug.Log("Object id 513" + objectList[0].id);
            //Debug.Log("Object id 514" + transferList[0].id);


            // cleanup
            finished = true;
            if (queryFinishedCallback != null) queryFinishedCallback.Invoke(transferList);

            yield return true;
        }
        
        /*
         *  Query For Masteroiece ( Draw picture)
         */
         private IEnumerator ExecuteQueryMoreLikeMasterpiece(SimilarQuery query)
        {
            // Since api is used several times it has to be set false on call
            finished = false;


            // === SIMILAR ===
            // Initial SimilarQuery
            yield return similarRequest =
                CineastUtils.BuildSimilarRequest(CineastUtils.Configuration.FindSimilarSegmentsUrl(), query);


            // Parse response
            earlyBreak = !Parse(similarRequest.text, out similarResult);
            yield return similarResult;
            if (earlyBreak) yield break;
            

            // Check if empty
            if (similarResult.IsEmpty()) {
                earlyBreak = true;
                yield break; // Stop and 
            }

            // === SEGMENTS ===
            // segments
            yield return segmentRequest =
                CineastUtils.BuildSegmentRequest(CineastUtils.Configuration.FindSegmentsByIdUrl(),
                    CineastUtils.ExtractIdArray(CineastUtils.ExtractContentObjects(similarResult)));

            // parse response
            earlyBreak = !Parse(segmentRequest.text, out segmentResult);
            yield return segmentResult;
            if (earlyBreak) yield break;


            yield return objectRequest =
                CineastUtils.BuildObjectsRequest(CineastUtils.Configuration.FindObjectsUrl(),
                    CineastUtils.ExtractIdArray(segmentResult.content));

            yield return objectsResult = JsonUtility.FromJson<ObjectsResult>(objectRequest.text);



            objectList = CineastUtils.Convert(objectsResult.content);


            // merge results
            var objects = CineastUtils.Convert(objectsResult.content);
            foreach (var mmo in objects)
                if (objectList.Contains(mmo))
                    objectList.Find(o => o.Equals(mmo)).Merge(mmo);


            results = new List<MultimediaObject>(objectList);

            // === WRAPUP ===
            foreach (var mmo in objectList) mmo.resultIndex = CineastUtils.GetIndexOf(mmo, similarResult) + 1;


            // === SORT LIST ===
            objectList.Sort(
                Comparison);

            List<MultimediaObject> transferList;
            if (filterEngine != null)
                transferList = filterEngine.ApplyFilters(objectList);
            else
                transferList = objectList;


            // cleanup
            finished = true;
            if (queryFinishedCallback != null) queryFinishedCallback.Invoke(transferList);

            yield return true;
        }

        private int Comparison(MultimediaObject mmo1, MultimediaObject mmo2)
        {
            return mmo1.resultIndex - mmo2.resultIndex;
        }

        private string DumpMMOList(List<MultimediaObject> list)
        {
            var ret = "[";

            foreach (var mmo in list) {
                ret += JsonUtility.ToJson(mmo);
                ret += ",";
            }

            return ret + "]";
        }

        public ObjectsResult getObjectsResult()
        {
            return objectsResult;
        }

        // String List with ids only
        public List<string> GetRandomObjectIds()
        {
            return randomObjectIds;
        }

        public List<String> GetMoreLikeThisResultIds(int number)
        {
            List<String> re = new List<string>();
            int a = number;
            for (int i = 0; i < a; i++) {
                re.Add(objectList[i].id);
            }

            return re;
        }

        public SimilarResult GetSimilarResult()
        {
            return similarResult;
        }

        public bool HasFinished()
        {
            return finished;
        }

        public bool HasEarlyBreak()
        {
            return earlyBreak;
        }

        public SegmentResult GetSegmentResult()
        {
            return segmentResult;
        }

        public MetaDataResult GetMetaResult()
        {
            return metaResult;
        }

        public bool Getisfinsched()
        {
            return finished;
        }

        public List<MultimediaObject> GetMultimediaObjects()
        {
            return objectList;
        }

        private static bool HasHTTPErrorOccurred(string msg)
        {
            return msg.StartsWith("<html>");
        }

        /**
         *  RETURNS FALSE IF AN ERROR OCCURED
         */
        private static bool Parse<T>(string toParse, out T result)
        {
            if (HasHTTPErrorOccurred(toParse)) {
                result = default(T);
                return false;
            }

            result = JsonUtility.FromJson<T>(toParse);
            return true;
        }

        public void Clean()
        {
            objectList.Clear();
        }

        public void AddCineastFilter(FilterStrategy strategy)
        {
            filterEngine.AddFilterStrategy(strategy);
        }

        public List<MultimediaObject> GetOriginalResults()
        {
            return new List<MultimediaObject>(results);
        }

        public WWW getSimilarrequest()
        {
            return similarRequest;
        }
    }
}