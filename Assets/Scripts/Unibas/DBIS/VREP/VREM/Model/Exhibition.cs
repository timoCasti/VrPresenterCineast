using System;

namespace DefaultNamespace.VREM.Model
{
  /// <summary>
  ///     ch.unibas.dmi.dbis.vrem.model.exhibition.Exhibition
  /// </summary>
  [Serializable]
    public class Exhibition
    {
        public string description;

        public string id;
        public string name;

        public Room[] rooms;
    }
}