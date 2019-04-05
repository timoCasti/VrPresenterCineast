using System;

namespace CineastUnityInterface.CineastAPI.Result
{
    [Serializable]
    public class SegmentResult
    {
        public const string MESSAGE_TYPE = "QR_SEGMENT";
        public SegmentObject[] content;
        public string messageType;

        public string queryId;
    }
}