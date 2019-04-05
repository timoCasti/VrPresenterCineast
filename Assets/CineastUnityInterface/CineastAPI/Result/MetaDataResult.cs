using System;

namespace CineastUnityInterface.CineastAPI.Result
{
    [Serializable]
    public class MetaDataResult
    {
        public const string MESSAGE_TYPE = "QR_METADATA";
        public MetaDataObject[] content;
        public string messageType;
        public string queryId;
    }
}