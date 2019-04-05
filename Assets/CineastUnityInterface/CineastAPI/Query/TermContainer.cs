using System;

namespace CineastUnityInterface.CineastAPI.Query
{
    [Serializable]
    public class TermContainer
    {
        public TermsObject[] terms;

        public TermContainer(TermsObject[] terms)
        {
            this.terms = terms;
        }
    }
}