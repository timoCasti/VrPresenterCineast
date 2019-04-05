﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Valve.VR
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class SteamVR_DefaultActionSet : Attribute
    {
        public string actionSetName;
        public bool overrideExistingOnGeneration;

        /// <summary>
        ///     Sets up a default action set to be assigned to this field or property on action set generation. Must be on a prefab
        ///     or in a scene in build settings.
        /// </summary>
        /// <param name="defaultActionSetName">The name of the action set to assign to this field/property</param>
        /// <param name="overrideExistingActionDuringGeneration">
        ///     Set to true if you want to always set this action to this field/property (even if set to something else)
        /// </param>
        public SteamVR_DefaultActionSet(string defaultActionSetName,
            bool overrideExistingActionDuringGeneration = false)
        {
            actionSetName = defaultActionSetName;
            overrideExistingOnGeneration = overrideExistingActionDuringGeneration;
        }

        public void AssignDefault(FieldInfo field, object onObject)
        {
            var actionSet = GetActionSet((MonoBehaviour) onObject);

            if (actionSet != null)
            {
                var currentActionSet = field.GetValue(onObject);

                if (currentActionSet == null || overrideExistingOnGeneration)
                    field.SetValue(onObject, actionSet);
            }
        }

        public void AssignDefault(PropertyInfo property, object onObject)
        {
            var actionSet = GetActionSet((MonoBehaviour) onObject);

            if (actionSet != null)
            {
                var currentActionSet = property.GetValue(onObject, null);

                if (currentActionSet == null || overrideExistingOnGeneration)
                    property.SetValue(onObject, actionSet, null);
            }
        }

        private SteamVR_ActionSet GetActionSet(MonoBehaviour monobehaviour)
        {
            var regex = GetRegex();

            var actionSet = SteamVR_Input_References.instance.actionSetObjects.FirstOrDefault(matchAction =>
                Regex.IsMatch(matchAction.fullPath, regex, RegexOptions.IgnoreCase));

            if (actionSet == null)
                Debug.Log("[SteamVR Input] Could not find action set matching path: " +
                          regex.Replace("\\", "").Replace(".+", "*"));

            return actionSet;
        }

        private string GetInputSource(MonoBehaviour monoBehaviour, string inputSourceFieldName)
        {
            if (inputSourceFieldName != null)
            {
                var monoBehaviourType = monoBehaviour.GetType();
                var inputSourceField = monoBehaviourType.GetField(inputSourceFieldName);

                if (inputSourceField != null)
                {
                    var source = (SteamVR_Input_Sources) inputSourceField.GetValue(monoBehaviour);
                    return source.ToString();
                }
            }

            return null;
        }

        private string GetRegex()
        {
            var regex = "\\/actions\\/";

            if (actionSetName != null)
                regex += actionSetName;
            else
                regex += ".+";

            return regex;
        }
    }
}