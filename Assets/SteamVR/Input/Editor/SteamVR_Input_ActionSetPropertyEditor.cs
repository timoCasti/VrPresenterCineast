﻿using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Valve.VR
{
    [CustomPropertyDrawer(typeof(SteamVR_ActionSet))]
    public class SteamVR_Input_ActionSetPropertyEditor : PropertyDrawer
    {
        protected const int notInitializedIndex = -1;
        protected const int noneIndex = 0;
        protected SteamVR_ActionSet[] actionSets;
        protected int addIndex = 1;
        protected string[] enumItems;
        public int selectedIndex = notInitializedIndex;

        protected void Awake()
        {
            actionSets = SteamVR_Input.GetActionSets();
            if (actionSets != null && actionSets.Length > 0)
            {
                var enumList = actionSets.Select(actionSet => actionSet.fullPath).ToList();

                enumList.Insert(noneIndex, "None");

                //replace forward slashes with backslack instead
                for (var index = 0; index < enumList.Count; index++)
                    enumList[index] = enumList[index].Replace('/', '\\');

                enumList.Add("Add...");
                enumItems = enumList.ToArray();
            }
            else
            {
                enumItems = new[] {"None", "Add..."};
            }

            addIndex = enumItems.Length - 1;

            /*
            //keep sub menus:
            for (int index = 0; index < enumItems.Length; index++)
                if (enumItems[index][0] == '/')
                    enumItems[index] = enumItems[index].Substring(1);
            */
        }

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (enumItems == null || enumItems.Length == 0) Awake();

            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);


            if (property.objectReferenceValue != null)
            {
                var actionSet = (SteamVR_ActionSet) property.objectReferenceValue;

                if (string.IsNullOrEmpty(actionSet.fullPath) == false)
                    for (var actionSetIndex = 0; actionSetIndex < actionSets.Length; actionSetIndex++)
                        if (actionSets[actionSetIndex].fullPath == actionSet.fullPath)
                        {
                            selectedIndex = actionSetIndex + 1;
                            break;
                        }
            }

            if (selectedIndex == notInitializedIndex)
                selectedIndex = 0;


            var labelPosition = position;
            labelPosition.width = EditorGUIUtility.labelWidth;
            EditorGUI.LabelField(labelPosition, label);

            var fieldPosition = position;
            fieldPosition.x = labelPosition.x + labelPosition.width;
            fieldPosition.width = EditorGUIUtility.currentViewWidth - (labelPosition.x + labelPosition.width) - 5 - 16;

            var objectRect = position;
            objectRect.x = fieldPosition.x + fieldPosition.width + 15;
            objectRect.width = 10;

            if (property.objectReferenceValue != null)
            {
                var selectObject = EditorGUI.Foldout(objectRect, false, GUIContent.none);
                if (selectObject) Selection.activeObject = property.objectReferenceValue;
            }


            var wasSelected = selectedIndex;
            selectedIndex = EditorGUI.Popup(fieldPosition, selectedIndex, enumItems);
            if (selectedIndex != wasSelected)
            {
                if (selectedIndex == noneIndex || selectedIndex == notInitializedIndex)
                {
                    selectedIndex = noneIndex;
                    property.objectReferenceValue = null;
                }
                else if (selectedIndex == addIndex)
                {
                    selectedIndex = wasSelected; // don't change the index
                    SteamVR_Input_EditorWindow.ShowWindow(); //show the input window so they can add one
                }
                else
                {
                    property.objectReferenceValue = actionSets[selectedIndex - 1];
                }
            }

            EditorGUI.EndProperty();
        }
    }
}