//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR
{
    /// <summary>
    ///     This is the base level action for SteamVR Input. All SteamVR_Action_In and SteamVR_Action_Out inherit from this.
    ///     Initializes the ulong handle for the action and has helper references.
    /// </summary>
    public abstract class SteamVR_Action : ScriptableObject
    {
        public SteamVR_ActionSet actionSet;

        [NonSerialized] private string cachedShortName;

        public float changeTolerance = 0.000001f;

        public SteamVR_ActionDirections direction;

        public string fullPath;

        [NonSerialized] protected ulong handle;

        [NonSerialized] protected Dictionary<SteamVR_Input_Sources, float> lastChanged =
            new Dictionary<SteamVR_Input_Sources, float>(new SteamVR_Input_Sources_Comparer());

        public float GetTimeLastChanged(SteamVR_Input_Sources inputSource)
        {
            return lastChanged[inputSource];
        }

        /// <summary>
        ///     Initializes the dictionaries used by this action
        /// </summary>
        public virtual void PreInitialize()
        {
            var sources = SteamVR_Input_Source.GetUpdateSources();
            for (var sourceIndex = 0; sourceIndex < sources.Length; sourceIndex++)
                InitializeDictionaries(sources[sourceIndex]);
        }

        /// <summary>
        ///     Initializes the handle for the action
        /// </summary>
        public virtual void Initialize()
        {
            var err = OpenVR.Input.GetActionHandle(fullPath.ToLower(), ref handle);

            if (err != EVRInputError.None)
                Debug.LogError("GetActionHandle (" + fullPath + ") error: " + err);
            //else Debug.Log("handle: " + handle);
        }

        protected virtual void InitializeDictionaries(SteamVR_Input_Sources source)
        {
            lastChanged.Add(source, 0);
        }

        /// <summary>Gets the last part of the path for this action. Removes action set.</summary>
        public string GetShortName()
        {
            if (cachedShortName == null) cachedShortName = SteamVR_Input_ActionFile.GetShortName(fullPath);

            return cachedShortName;
        }
    }
}