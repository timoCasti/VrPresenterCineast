using System;

namespace CineastUnityInterface.CineastAPI.Query
{
    /*
     * Stuff and things
     */
    [Serializable]
    public class ObjectQuery
    {
        public string[] ids;

        public ObjectQuery(string[] ids)
        {
            this.ids = ids;
        }
    }
}