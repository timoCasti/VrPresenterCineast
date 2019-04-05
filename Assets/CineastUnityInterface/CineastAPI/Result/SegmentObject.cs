using System;

namespace CineastUnityInterface.CineastAPI.Result
{
    [Serializable]
    public class SegmentObject
    {
        public int count;
        public int end;
        public double endabs;
        public string objectId;
        public string segmentId;
        public int sequenceNumber;
        public int start;
        public double startabs;
    }
}