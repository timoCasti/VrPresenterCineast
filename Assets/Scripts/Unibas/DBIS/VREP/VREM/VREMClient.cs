using System;
using System.Collections;
using System.Text;
using UnityEngine;

namespace DefaultNamespace.VREM
{
    public class VREMClient : MonoBehaviour
    {
        private const string LOAD_EXHIBITION_ACTION = "exhibitions/load/";
        private const string LIST_EXHIBITIONS_ACTION = "exhibitions/list";

        private bool error;

        private string response;

        private Action<string> responseProcessor;
        public string ServerUrl;
        private string suffix;

        /// <summary>
        ///     Requests an exhibition and calls the processor, once the exhibition is loaded.
        /// </summary>
        /// <param name="exhibitionId">The ID of the exhibition</param>
        /// <param name="processor">An Action which processes VREM's response. If null is passed to that action, an error occurred</param>
        public void RequestExhibition(string exhibitionId, Action<string> processor)
        {
            suffix = exhibitionId;
            responseProcessor = processor;
            StartCoroutine(DoExhibitionRequest());
        }

        private IEnumerator DoExhibitionRequest()
        {
            if (!ServerUrl.StartsWith("http://")) ServerUrl = "http://" + ServerUrl;

            if (!ServerUrl.EndsWith("/")) ServerUrl = ServerUrl + "/";
            Debug.Log("[RC] Requesting... " + ServerUrl + LOAD_EXHIBITION_ACTION + suffix);
            var www = new WWW(ServerUrl + LOAD_EXHIBITION_ACTION + suffix);
            yield return www;
            if (www.error == null)
            {
                response = www.text;
                if (responseProcessor != null) responseProcessor.Invoke(response);
            }
            else
            {
                Debug.LogError(www.error);
                // Error, handle it!
                error = true;
                responseProcessor.Invoke(null);
            }
        }

        public bool HasError()
        {
            return error;
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
            var postData = Encoding.ASCII.GetBytes(json.ToCharArray());
            return new WWW(url,
                postData);
        }
    }
}