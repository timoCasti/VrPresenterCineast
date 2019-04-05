﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Valve.VR
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class SteamVR_DefaultAction : Attribute
    {
        public string actionName;
        public string actionSetName;
        public SteamVR_ActionDirections? direction;
        public string inputSourceFieldName;
        public bool overrideExistingOnGeneration;

        /// <summary>
        ///     Sets up a default action to be assigned to this field or property on action generation. Must be on a prefab or in a
        ///     scene in build settings.
        /// </summary>
        /// <param name="defaultActionName">The name of the action to assign to this field/property</param>
        /// <param name="overrideExistingActionDuringGeneration">
        ///     Set to true if you want to always set this action to this field/property (even if set to something else)
        /// </param>
        public SteamVR_DefaultAction(string defaultActionName, bool overrideExistingActionDuringGeneration = false)
        {
            actionName = defaultActionName;
            overrideExistingOnGeneration = overrideExistingActionDuringGeneration;
        }

        /// <summary>
        ///     Sets up a default action to be assigned to this field or property on action generation. Must be on a prefab or in a
        ///     scene in build settings.
        /// </summary>
        /// <param name="defaultActionName">The name of the action to assign to this field/property</param>
        /// <param name="overrideExistingActionDuringGeneration">
        ///     Set to true if you want to always set this action to this field/property (even if set to something else)
        /// </param>
        /// <param name="defaultActionDirection">The direction of the action (input / output)</param>
        public SteamVR_DefaultAction(string defaultActionName, SteamVR_ActionDirections defaultActionDirection,
            bool overrideExistingActionDuringGeneration = false)
        {
            actionName = defaultActionName;
            direction = defaultActionDirection;
            overrideExistingOnGeneration = overrideExistingActionDuringGeneration;
        }

        /// <summary>
        ///     Sets up a default action to be assigned to this field or property on action generation. Must be on a prefab or in a
        ///     scene in build settings.
        /// </summary>
        /// <param name="defaultActionName">The name of the action to assign to this field/property</param>
        /// <param name="overrideExistingActionDuringGeneration">
        ///     Set to true if you want to always set this action to this field/property (even if set to something else)
        /// </param>
        /// <param name="defaultActionSetName">The name of the action set that the action to assign is a member of</param>
        public SteamVR_DefaultAction(string defaultActionName, string defaultActionSetName,
            bool overrideExistingActionDuringGeneration = false)
        {
            actionName = defaultActionName;
            actionSetName = defaultActionSetName;
            overrideExistingOnGeneration = overrideExistingActionDuringGeneration;
        }

        /// <summary>
        ///     Sets up a default action to be assigned to this field or property on action generation. Must be on a prefab or in a
        ///     scene in build settings.
        /// </summary>
        /// <param name="defaultActionName">The name of the action to assign to this field/property</param>
        /// <param name="overrideExistingActionDuringGeneration">
        ///     Set to true if you want to always set this action to this field/property (even if set to something else)
        /// </param>
        /// <param name="defaultActionSetName">The name of the action set that the action to assign is a member of</param>
        /// <param name="defaultActionDirection">The direction of the action (input / output)</param>
        public SteamVR_DefaultAction(string defaultActionName, string defaultActionSetName,
            SteamVR_ActionDirections defaultActionDirection, bool overrideExistingActionDuringGeneration = false)
        {
            actionName = defaultActionName;
            actionSetName = defaultActionSetName;
            direction = defaultActionDirection;
            overrideExistingOnGeneration = overrideExistingActionDuringGeneration;
        }

        /// <summary>
        ///     Sets up a default action to be assigned to this field or property on action generation. Must be on a prefab or in a
        ///     scene in build settings.
        /// </summary>
        /// <param name="defaultActionName">The name of the action to assign to this field/property</param>
        /// <param name="overrideExistingActionDuringGeneration">
        ///     Set to true if you want to always set this action to this field/property (even if set to something else)
        /// </param>
        /// <param name="defaultActionSetName">The name of the action set that the action to assign is a member of</param>
        /// <param name="inputSourceFieldName">
        ///     If this is an action that is handed (Skeleton actions for example) you can add the value of an input source field
        ///     to the end of the name of the action.
        ///     ex. To match the SkeletonLeft action you put "Skeleton" as the action name and then "inputSource" as the
        ///     inputSourceFieldName.
        ///     On generation tt will get the value for the input source and applies it to the end of the action name.
        /// </param>
        public SteamVR_DefaultAction(string defaultActionName, string defaultActionSetName, string inputSourceFieldName,
            bool overrideExistingActionDuringGeneration = false)
        {
            actionName = defaultActionName;
            actionSetName = defaultActionSetName;
            this.inputSourceFieldName = inputSourceFieldName;
            overrideExistingOnGeneration = overrideExistingActionDuringGeneration;
        }

        /// <summary>
        ///     Sets up a default action to be assigned to this field or property on action generation. Must be on a prefab or in a
        ///     scene in build settings.
        /// </summary>
        /// <param name="defaultActionName">The name of the action to assign to this field/property</param>
        /// <param name="overrideExistingActionDuringGeneration">
        ///     Set to true if you want to always set this action to this field/property (even if set to something else)
        /// </param>
        /// <param name="defaultActionSetName">The name of the action set that the action to assign is a member of</param>
        /// <param name="inputSourceFieldName">
        ///     If this is an action that is handed (Skeleton actions for example) you can add the value of an input source field
        ///     to the end of the name of the action.
        ///     ex. To match the SkeletonLeft action you put "Skeleton" as the action name and then "inputSource" as the
        ///     inputSourceFieldName.
        ///     On generation tt will get the value for the input source and applies it to the end of the action name.
        /// </param>
        /// <param name="defaultActionDirection">The direction of the action (input / output)</param>
        public SteamVR_DefaultAction(string defaultActionName, string defaultActionSetName, string inputSourceFieldName,
            SteamVR_ActionDirections defaultActionDirection, bool overrideExistingActionDuringGeneration = false)
        {
            actionName = defaultActionName;
            actionSetName = defaultActionSetName;
            this.inputSourceFieldName = inputSourceFieldName;
            direction = defaultActionDirection;
            overrideExistingOnGeneration = overrideExistingActionDuringGeneration;
        }

        public bool ShouldAssign(FieldInfo field, object onObject)
        {
            var action = GetAction((MonoBehaviour) onObject);

            if (action != null)
            {
                var currentAction = field.GetValue(onObject);

                if (currentAction == null || overrideExistingOnGeneration)
                {
                    if (action.GetType() != field.FieldType)
                        Debug.LogWarning("[SteamVR] Could not assign default action for " +
                                         ((MonoBehaviour) onObject).gameObject.name + "::" +
                                         ((MonoBehaviour) onObject).name + "::" + field.Name
                                         + ". Expected type: " + field.FieldType.Name + ", found action type: " +
                                         action.GetType().Name);
                    else
                        return true;
                }
            }

            return false;
        }

        public void AssignDefault(FieldInfo field, object onObject)
        {
            var action = GetAction((MonoBehaviour) onObject);

            if (action != null)
                if (ShouldAssign(field, onObject))
                    field.SetValue(onObject, action);
        }

        public void AssignDefault(PropertyInfo property, object onObject)
        {
            var action = GetAction((MonoBehaviour) onObject);

            if (action != null)
                if (ShouldAssign(property, onObject))
                    property.SetValue(onObject, action, null);
        }

        public bool ShouldAssign(PropertyInfo property, object onObject)
        {
            var action = GetAction((MonoBehaviour) onObject);

            if (action != null)
            {
                var currentAction = property.GetValue(onObject, null);

                if (currentAction == null || overrideExistingOnGeneration)
                {
                    if (action.GetType() != property.PropertyType)
                        Debug.LogWarning("[SteamVR] Could not assign default action for " +
                                         ((MonoBehaviour) onObject).gameObject.name + "::" +
                                         ((MonoBehaviour) onObject).name + "::" + property.Name
                                         + ". Expected type: " + property.PropertyType.Name + ", found action type: " +
                                         action.GetType().Name);
                    else
                        return true;
                }
                else
                {
                    Debug.LogWarning(
                        "[SteamVR] Not assigning default because current action is not null. Could not assign default action for " +
                        ((MonoBehaviour) onObject).gameObject.name + "::" + ((MonoBehaviour) onObject).name + "::" +
                        property.Name
                        + ". Expected type: " + property.PropertyType.Name + ", found action type: " +
                        action.GetType().Name + ". " + ((SteamVR_Action) currentAction).fullPath);
                }
            }

            return false;
        }

        private SteamVR_Action GetAction(MonoBehaviour monobehaviour)
        {
            var inputSource = GetInputSource(monobehaviour, inputSourceFieldName);
            var regex = GetRegex(inputSource);

            var action = SteamVR_Input_References.instance.actionObjects.FirstOrDefault(matchAction =>
                Regex.IsMatch(matchAction.fullPath, regex, RegexOptions.IgnoreCase));

            if (action == null)
                Debug.LogWarning("[SteamVR Input] Could not find action matching path: " +
                                 regex.Replace("\\", "").Replace(".+", "*"));
            //else Debug.Log("Looking for: " + regex + ". Found: " + action.fullPath);

            return action;
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

        private string GetRegex(string inputSource)
        {
            var regex = "\\/actions\\/";

            if (actionSetName != null)
                regex += actionSetName;
            else
                regex += ".+";

            regex += "\\/";

            if (direction != null)
                regex += direction.ToString();
            else
                regex += ".+";

            regex += "\\/" + actionName;

            if (inputSource != null) regex += inputSource;

            return regex;
        }
    }
}