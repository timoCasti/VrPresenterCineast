using System;
using System.Collections.Generic;
using Unibas.DBIS.VREP;
using UnityEngine;

namespace DefaultNamespace.VREM.Model
{
  /// <summary>
  ///     ch.unibas.dmi.dbis.vrem.model.Ehibit
  /// </summary>
  [Serializable]
    public class Exhibit
    {
        public string audio;
        public string description;
        public string id;
        public bool light;

        public Dictionary<string, string> metadata;
        public string name;
        public string path;

        public Vector3 position;
        public Vector3 size;
        public string type;

        public string GetURLEncodedPath()
        {
            return path;
            //return "C:/Users/timoc/Desktop/BA/vitrivrdb/data/gary.png";
            //return VREPController.Instance.Settings.VREMAddress+"content/get/"+path.Substring(0).Replace("/", "%2F").Replace(" ", "%20");
        }

        public string GetURLEncodedAudioPath()
        {
            if (!string.IsNullOrEmpty(audio))
                return VREPController.Instance.Settings.VREMAddress + "content/get/" +
                       audio.Substring(0).Replace("/", "%2F").Replace(" ", "%20");
            return null;
        }
    }
}