using System;

namespace CineastUnityInterface.CineastAPI.Query
{
    [Serializable]
    public class IdsQuery
    {
        public string[] ids;

        public IdsQuery(string[] ids)
        {
            this.ids = ids;
        }
    }
}