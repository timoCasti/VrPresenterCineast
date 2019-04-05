//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Valve.VR
{
    /// <summary>
    ///     Action sets are logical groupings of actions. Multiple sets can be active at one time.
    /// </summary>
    public class SteamVR_ActionSet : ScriptableObject
    {
        [NonSerialized] protected static VRActiveActionSet_t[] activeActionSets;

        [NonSerialized]
        protected static List<VRActiveActionSet_t> activeActionSetsList = new List<VRActiveActionSet_t>();

        [NonSerialized] protected static uint activeActionSetSize;

        [NonSerialized] protected static int lastFrameUpdated;

        [NonSerialized] protected VRActiveActionSet_t actionSet;

        /// <summary>All actions within this set (including out actions)</summary>
        public SteamVR_Action[] allActions;

        [NonSerialized] private string cachedShortName;


        /// <summary>The full path to this action set (ex: /actions/in/default)</summary>
        public string fullPath;

        [NonSerialized] public ulong handle;

        [NonSerialized] protected float lastChanged = -1;

        /// <summary>All IN actions within this set that are NOT pose or skeleton actions</summary>
        public SteamVR_Action_In[] nonVisualInActions;

        /// <summary>All out actions within this set</summary>
        public SteamVR_Action_Out[] outActionArray;

        /// <summary>All pose actions within this set</summary>
        public SteamVR_Action_Pose[] poseActions;

        [NonSerialized] protected bool setIsActive;

        /// <summary>All skeleton actions within this set</summary>
        public SteamVR_Action_Skeleton[] skeletonActions;

        public string usage;

        /// <summary>All pose and skeleton actions within this set</summary>
        public SteamVR_Action_In[] visualActions;

        public void Initialize()
        {
            var err = OpenVR.Input.GetActionSetHandle(fullPath.ToLower(), ref handle);

            if (err != EVRInputError.None)
                Debug.LogError("GetActionSetHandle (" + fullPath + ") error: " + err);

            activeActionSetSize = (uint) Marshal.SizeOf(typeof(VRActiveActionSet_t));
        }

        /// <summary>
        ///     Returns whether the set is currently active or not.
        /// </summary>
        public bool IsActive()
        {
            return setIsActive;
        }

        /// <summary>
        ///     Returns the last time this action set was changed (set to active or inactive)
        /// </summary>
        /// <returns></returns>
        public float GetTimeLastChanged()
        {
            return lastChanged;
        }

        /// <summary>
        ///     Activate this set as a primary action set so its actions can be called
        /// </summary>
        /// <param name="disableAllOtherActionSets">Disable all other action sets at the same time</param>
        public void ActivatePrimary(bool disableAllOtherActionSets = false)
        {
            if (disableAllOtherActionSets)
                DisableAllActionSets();

            actionSet.ulActionSet = handle;

            if (activeActionSetsList.Contains(actionSet) == false)
                activeActionSetsList.Add(actionSet);

            setIsActive = true;
            lastChanged = Time.time;

            UpdateActionSetArray();
        }

        /// <summary>
        ///     Activate this set as a secondary action set so its actions can be called
        /// </summary>
        /// <param name="disableAllOtherActionSets">Disable all other action sets at the same time</param>
        public void ActivateSecondary(bool disableAllOtherActionSets = false)
        {
            if (disableAllOtherActionSets)
                DisableAllActionSets();

            actionSet.ulSecondaryActionSet = handle;

            if (activeActionSetsList.Contains(actionSet) == false)
                activeActionSetsList.Add(actionSet);

            setIsActive = true;
            lastChanged = Time.time;

            UpdateActionSetArray();
        }

        /// <summary>
        ///     Deactivate the action set so its actions can no longer be called
        /// </summary>
        public void Deactivate()
        {
            setIsActive = false;
            lastChanged = Time.time;

            if (actionSet.ulActionSet == handle)
                actionSet.ulActionSet = 0;
            if (actionSet.ulSecondaryActionSet == handle)
                actionSet.ulActionSet = 0;

            if (actionSet.ulActionSet == 0 && actionSet.ulSecondaryActionSet == 0)
            {
                activeActionSetsList.Remove(actionSet);

                UpdateActionSetArray();
            }
        }

        /// <summary>
        ///     Disable all known action sets.
        /// </summary>
        public static void DisableAllActionSets()
        {
            for (var actionSetIndex = 0; actionSetIndex < SteamVR_Input.actionSets.Length; actionSetIndex++)
            {
                var set = SteamVR_Input.actionSets[actionSetIndex];
                set.Deactivate();
            }
        }

        protected static void UpdateActionSetArray()
        {
            activeActionSets = activeActionSetsList.ToArray();
        }

        public static void UpdateActionSetsState(bool force = false)
        {
            if (force || Time.frameCount != lastFrameUpdated)
            {
                lastFrameUpdated = Time.frameCount;

                if (activeActionSets != null && activeActionSets.Length > 0)
                {
                    var err = OpenVR.Input.UpdateActionState(activeActionSets, activeActionSetSize);
                    if (err != EVRInputError.None)
                        Debug.LogError("UpdateActionState error: " + err);
                    //else Debug.Log("Action sets activated: " + activeActionSets.Length);
                }
            }
        }

        /// <summary>Gets the last part of the path for this action. Removes "actions" and direction.</summary>
        public string GetShortName()
        {
            if (cachedShortName == null) cachedShortName = SteamVR_Input_ActionFile.GetShortName(fullPath);

            return cachedShortName;
        }
    }
}