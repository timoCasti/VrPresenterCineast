//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Valve.VR
{
    public class SteamVR_Settings : ScriptableObject
    {
        private static SteamVR_Settings _instance;

        [Tooltip("Filename local to the project root (or executable, in a build)")]
        public string actionsFilePath = "actions.json";

        public bool activateFirstActionSetOnStart = true;

        [Tooltip(
            "This is the app key the unity editor will use to identify your application. (can be \"steam.app.[appid]\" to persist bindings between editor steam)")]
        public string editorAppKey;

        public SteamVR_UpdateModes inputUpdateMode = SteamVR_UpdateModes.OnUpdate;
        public bool lockPhysicsUpdateRateToRenderFrequency = true;

        public bool pauseGameWhenDashboardVisible = true;
        public SteamVR_UpdateModes poseUpdateMode = SteamVR_UpdateModes.OnPreCull;

        [Tooltip("Path local to the Assets folder")]
        public string steamVRInputPath = "SteamVR_Input";

        public ETrackingUniverseOrigin trackingSpace = ETrackingUniverseOrigin.TrackingUniverseStanding;

        public static SteamVR_Settings instance
        {
            get
            {
                LoadInstance();

                return _instance;
            }
        }

        private static void LoadInstance()
        {
            if (_instance == null)
            {
                _instance = Resources.Load<SteamVR_Settings>("SteamVR_Settings");

                if (_instance == null)
                {
                    _instance = CreateInstance<SteamVR_Settings>();

#if UNITY_EDITOR
                    var folderPath = SteamVR.GetResourcesFolderPath(true);
                    var assetPath = Path.Combine(folderPath, "SteamVR_Settings.asset");

                    AssetDatabase.CreateAsset(_instance, assetPath);
                    AssetDatabase.SaveAssets();
#endif
                }

                if (string.IsNullOrEmpty(_instance.editorAppKey))
                {
                    _instance.editorAppKey = SteamVR.GenerateAppKey();
                    Debug.Log("[SteamVR] Generated you an editor app key of: " + _instance.editorAppKey +
                              ". This lets the editor tell SteamVR what project this is. Has no effect on builds. This can be changed in Assets/SteamVR/Resources/SteamVR_Settings");
#if UNITY_EDITOR
                    EditorUtility.SetDirty(_instance);
                    AssetDatabase.SaveAssets();
#endif
                }
            }
        }

        public bool IsInputUpdateMode(SteamVR_UpdateModes tocheck)
        {
            return (inputUpdateMode & tocheck) == tocheck;
        }

        public bool IsPoseUpdateMode(SteamVR_UpdateModes tocheck)
        {
            return (poseUpdateMode & tocheck) == tocheck;
        }

        public static void VerifyScriptableObject()
        {
            LoadInstance();
        }
    }
}