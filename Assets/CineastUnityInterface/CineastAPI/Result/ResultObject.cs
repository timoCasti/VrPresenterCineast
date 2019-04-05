using System;

namespace CineastUnityInterface.CineastAPI.Result
{
    [Serializable]
    public class ResultObject
    {
        public const string MESSAGE_TYPE = "QR_SIMILARITY";
        public string category; // spatial or later temporal?
        public ContentObject[] content;
        public string messagetype; // QR_SIMILARITY

        public string queryId;

        public bool IsEmpty()
        {
            return content.Length == 0;
        }
    }
}