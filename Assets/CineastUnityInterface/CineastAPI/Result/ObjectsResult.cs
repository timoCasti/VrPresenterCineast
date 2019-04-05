using System;

namespace CineastUnityInterface.CineastAPI.Result
{
    [Serializable]
    public class ObjectsResult
    {
        public const string MESSAGE_TYPE = "QR_OBJECT";
        public CineastObject[] content;
        public string messageType; // QR_OBJECT

        public string queryId;
    }
}