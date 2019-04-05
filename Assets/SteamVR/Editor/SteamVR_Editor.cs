﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Custom inspector display for SteamVR_Camera
//
//=============================================================================

using System.IO;
using UnityEditor;
using UnityEngine;
using Valve.VR;

[CustomEditor(typeof(SteamVR_Camera))]
[CanEditMultipleObjects]
public class SteamVR_Editor : Editor
{
    private readonly int bannerHeight = 150;
    private Texture logo;

    private SerializedProperty script, wireframe;

    private string GetResourcePath()
    {
        var ms = MonoScript.FromScriptableObject(this);
        var path = AssetDatabase.GetAssetPath(ms);
        path = Path.GetDirectoryName(path);
        return path.Substring(0, path.Length - "Editor".Length) + "Textures/";
    }

    private void OnEnable()
    {
        var resourcePath = GetResourcePath();

        logo = AssetDatabase.LoadAssetAtPath<Texture2D>(resourcePath + "logo.png");

        script = serializedObject.FindProperty("m_Script");

        wireframe = serializedObject.FindProperty("wireframe");

        foreach (SteamVR_Camera target in targets)
            target.ForceLast();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var rect = GUILayoutUtility.GetRect(Screen.width - 38, bannerHeight, GUI.skin.box);
        if (logo)
            GUI.DrawTexture(rect, logo, ScaleMode.ScaleToFit);

        if (!Application.isPlaying)
        {
            var expand = false;
            var collapse = false;
            foreach (SteamVR_Camera target in targets)
            {
                if (AssetDatabase.Contains(target))
                    continue;
                if (target.isExpanded)
                    collapse = true;
                else
                    expand = true;
            }

            if (expand)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Expand"))
                    foreach (SteamVR_Camera target in targets)
                    {
                        if (AssetDatabase.Contains(target))
                            continue;
                        if (!target.isExpanded)
                        {
                            target.Expand();
                            EditorUtility.SetDirty(target);
                        }
                    }

                GUILayout.Space(18);
                GUILayout.EndHorizontal();
            }

            if (collapse)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Collapse"))
                    foreach (SteamVR_Camera target in targets)
                    {
                        if (AssetDatabase.Contains(target))
                            continue;
                        if (target.isExpanded)
                        {
                            target.Collapse();
                            EditorUtility.SetDirty(target);
                        }
                    }

                GUILayout.Space(18);
                GUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.PropertyField(script);
        EditorGUILayout.PropertyField(wireframe);

        serializedObject.ApplyModifiedProperties();
    }

    public static void ExportPackage()
    {
        AssetDatabase.ExportPackage(new[]
        {
            "Assets/SteamVR",
            "Assets/Plugins/openvr_api.cs",
            "Assets/Plugins/openvr_api.bundle",
            "Assets/Plugins/x86/openvr_api.dll",
            "Assets/Plugins/x86/steam_api.dll",
            "Assets/Plugins/x86/libsteam_api.so",
            "Assets/Plugins/x86_64/openvr_api.dll",
            "Assets/Plugins/x86_64/steam_api.dll",
            "Assets/Plugins/x86_64/libsteam_api.so",
            "Assets/Plugins/x86_64/libopenvr_api.so"
        }, "steamvr.unitypackage", ExportPackageOptions.Recurse);
        EditorApplication.Exit(0);
    }
}