using System;

namespace CineastUnityInterface.CineastAPI.Query
{
    [Serializable]
    public class SimilarQuery
    {
        public TermContainer[] containers;
        public string[] types = {"IMAGE"};

        public SimilarQuery(TermContainer[] containers)
        {
            this.containers = containers;
        }

        public void With(SimilarQuery query)
        {
            TermContainer[] tc = {containers[0], query.containers[0]};
            containers = tc;
        }
    }
}