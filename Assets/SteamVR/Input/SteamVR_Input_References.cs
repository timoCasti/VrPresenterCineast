//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Valve.VR
{
    public class SteamVR_Input_References : ScriptableObject
    {
        [NonSerialized] private static SteamVR_Input_References _instance;

        public string[] actionNames;
        public SteamVR_Action[] actionObjects;

        public string[] actionSetNames;
        public SteamVR_ActionSet[] actionSetObjects;

        public static SteamVR_Input_References instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<SteamVR_Input_References>("SteamVR_Input_References");

#if UNITY_EDITOR
                    if (_instance == null)
                    {
                        _instance = CreateInstance<SteamVR_Input_References>();

                        var folderPath = SteamVR_Input.GetResourcesFolderPath(true);
                        var assetPath = Path.Combine(folderPath, "SteamVR_Input_References.asset");

                        AssetDatabase.CreateAsset(_instance, assetPath);
                        AssetDatabase.SaveAssets();
                    }
#endif
                }

                return _instance;
            }
        }

        public static SteamVR_Action GetAction(string name)
        {
            for (var nameIndex = 0; nameIndex < instance.actionNames.Length; nameIndex++)
                if (string.Equals(instance.actionNames[nameIndex], name, StringComparison.CurrentCultureIgnoreCase))
                    return instance.actionObjects[nameIndex];

            return null;
        }

        public static SteamVR_ActionSet GetActionSet(string set)
        {
            for (var setIndex = 0; setIndex < instance.actionSetNames.Length; setIndex++)
                if (string.Equals(instance.actionSetNames[setIndex], set, StringComparison.CurrentCultureIgnoreCase))
                    return instance.actionSetObjects[setIndex];
            return null;
        }
    }
}