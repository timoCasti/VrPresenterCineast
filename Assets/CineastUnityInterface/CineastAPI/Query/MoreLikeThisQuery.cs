
// More like this 
namespace CineastUnityInterface.CineastAPI.Query
{
    public class MoreLikeThisQuery
    {
        public TermContainer[] containers;
        public string[] types = {"ID"};

        public MoreLikeThisQuery(TermContainer[] containers)
        {
            this.containers = containers;
        }
    }
}