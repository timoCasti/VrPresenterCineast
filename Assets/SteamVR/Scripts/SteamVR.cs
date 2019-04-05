﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Access to SteamVR system (hmd) and compositor (distort) interfaces.
//
//=============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Valve.Newtonsoft.Json;
using Debug = UnityEngine.Debug;
#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;

#else
using XRSettings = UnityEngine.VR.VRSettings;
using XRDevice = UnityEngine.VR.VRDevice;
#endif

namespace Valve.VR
{
    public class SteamVR : IDisposable
    {
        public const string defaultUnityAppKeyTemplate = "application.generated.unity.{0}.exe";
        public const string defaultAppKeyTemplate = "application.generated.{0}";

        // Set this to false to keep from auto-initializing when calling SteamVR.instance.
        private static bool _enabled = true;

        private static SteamVR _instance;

        public static bool[] connected = new bool[OpenVR.k_unMaxTrackedDeviceCount];
        public ETextureType textureType;

        private SteamVR()
        {
            hmd = OpenVR.System;
            Debug.Log("Connected to " + hmd_TrackingSystemName + ":" + hmd_SerialNumber);

            compositor = OpenVR.Compositor;
            overlay = OpenVR.Overlay;

            // Setup render values
            uint w = 0, h = 0;
            hmd.GetRecommendedRenderTargetSize(ref w, ref h);
            sceneWidth = w;
            sceneHeight = h;

            float l_left = 0.0f, l_right = 0.0f, l_top = 0.0f, l_bottom = 0.0f;
            hmd.GetProjectionRaw(EVREye.Eye_Left, ref l_left, ref l_right, ref l_top, ref l_bottom);

            float r_left = 0.0f, r_right = 0.0f, r_top = 0.0f, r_bottom = 0.0f;
            hmd.GetProjectionRaw(EVREye.Eye_Right, ref r_left, ref r_right, ref r_top, ref r_bottom);

            tanHalfFov = new Vector2(
                Mathf.Max(-l_left, l_right, -r_left, r_right),
                Mathf.Max(-l_top, l_bottom, -r_top, r_bottom));

            textureBounds = new VRTextureBounds_t[2];

            textureBounds[0].uMin = 0.5f + 0.5f * l_left / tanHalfFov.x;
            textureBounds[0].uMax = 0.5f + 0.5f * l_right / tanHalfFov.x;
            textureBounds[0].vMin = 0.5f - 0.5f * l_bottom / tanHalfFov.y;
            textureBounds[0].vMax = 0.5f - 0.5f * l_top / tanHalfFov.y;

            textureBounds[1].uMin = 0.5f + 0.5f * r_left / tanHalfFov.x;
            textureBounds[1].uMax = 0.5f + 0.5f * r_right / tanHalfFov.x;
            textureBounds[1].vMin = 0.5f - 0.5f * r_bottom / tanHalfFov.y;
            textureBounds[1].vMax = 0.5f - 0.5f * r_top / tanHalfFov.y;

            // Grow the recommended size to account for the overlapping fov
            sceneWidth = sceneWidth / Mathf.Max(textureBounds[0].uMax - textureBounds[0].uMin,
                             textureBounds[1].uMax - textureBounds[1].uMin);
            sceneHeight = sceneHeight / Mathf.Max(textureBounds[0].vMax - textureBounds[0].vMin,
                              textureBounds[1].vMax - textureBounds[1].vMin);

            aspect = tanHalfFov.x / tanHalfFov.y;
            fieldOfView = 2.0f * Mathf.Atan(tanHalfFov.y) * Mathf.Rad2Deg;

            eyes = new[]
            {
                new SteamVR_Utils.RigidTransform(hmd.GetEyeToHeadTransform(EVREye.Eye_Left)),
                new SteamVR_Utils.RigidTransform(hmd.GetEyeToHeadTransform(EVREye.Eye_Right))
            };

            switch (SystemInfo.graphicsDeviceType)
            {
#if (UNITY_5_4)
                case UnityEngine.Rendering.GraphicsDeviceType.OpenGL2:
#endif
                case GraphicsDeviceType.OpenGLCore:
                case GraphicsDeviceType.OpenGLES2:
                case GraphicsDeviceType.OpenGLES3:
                    textureType = ETextureType.OpenGL;
                    break;
#if !(UNITY_5_4)
                case GraphicsDeviceType.Vulkan:
                    textureType = ETextureType.Vulkan;
                    break;
#endif
                default:
                    textureType = ETextureType.DirectX;
                    break;
            }

            SteamVR_Events.Initializing.Listen(OnInitializing);
            SteamVR_Events.Calibrating.Listen(OnCalibrating);
            SteamVR_Events.OutOfRange.Listen(OnOutOfRange);
            SteamVR_Events.DeviceConnected.Listen(OnDeviceConnected);
            SteamVR_Events.NewPoses.Listen(OnNewPoses);
        }

        // Use this to check if SteamVR is currently active without attempting
        // to activate it in the process.
        public static bool active
        {
            get { return _instance != null; }
        }

        public static bool enabled
        {
            get
            {
                if (!XRSettings.enabled)
                    enabled = false;
                return _enabled;
            }
            set
            {
                _enabled = value;

                if (_enabled)
                    Initialize();
                else
                    SafeDispose();
            }
        }

        public static SteamVR instance
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    return null;
#endif
                if (!enabled)
                    return null;

                if (_instance == null)
                {
                    _instance = CreateInstance();

                    // If init failed, then auto-disable so scripts don't continue trying to re-initialize things.
                    if (_instance == null)
                        _enabled = false;
                }

                return _instance;
            }
        }

        public static bool usingNativeSupport
        {
            get { return XRDevice.GetNativePtr() != IntPtr.Zero; }
        }

        public static SteamVR_Settings settings { get; private set; }

        // native interfaces
        public CVRSystem hmd { get; private set; }
        public CVRCompositor compositor { get; private set; }
        public CVROverlay overlay { get; private set; }

        // tracking status
        public static bool initializing { get; private set; }
        public static bool calibrating { get; private set; }
        public static bool outOfRange { get; private set; }

        // render values
        public float sceneWidth { get; private set; }
        public float sceneHeight { get; private set; }
        public float aspect { get; private set; }
        public float fieldOfView { get; private set; }
        public Vector2 tanHalfFov { get; private set; }
        public VRTextureBounds_t[] textureBounds { get; private set; }
        public SteamVR_Utils.RigidTransform[] eyes { get; private set; }

        // hmd properties
        public string hmd_TrackingSystemName
        {
            get { return GetStringProperty(ETrackedDeviceProperty.Prop_TrackingSystemName_String); }
        }

        public string hmd_ModelNumber
        {
            get { return GetStringProperty(ETrackedDeviceProperty.Prop_ModelNumber_String); }
        }

        public string hmd_SerialNumber
        {
            get { return GetStringProperty(ETrackedDeviceProperty.Prop_SerialNumber_String); }
        }

        public float hmd_SecondsFromVsyncToPhotons
        {
            get { return GetFloatProperty(ETrackedDeviceProperty.Prop_SecondsFromVsyncToPhotons_Float); }
        }

        public float hmd_DisplayFrequency
        {
            get { return GetFloatProperty(ETrackedDeviceProperty.Prop_DisplayFrequency_Float); }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public static void Initialize(bool forceUnityVRMode = false)
        {
            if (forceUnityVRMode)
            {
                SteamVR_Behaviour.instance.InitializeSteamVR(true);
                return;
            }

            if (_instance == null)
            {
                _instance = CreateInstance();
                if (_instance == null)
                    _enabled = false;
            }

            if (_enabled)
                SteamVR_Behaviour.Initialize();
        }

        private static SteamVR CreateInstance()
        {
            try
            {
                var error = EVRInitError.None;
                if (!usingNativeSupport)
                {
                    var errorLog = "[SteamVR] Initialization failed. ";

                    if (XRSettings.enabled == false)
                        errorLog +=
                            "VR may be disabled in player settings. Go to player settings in the editor and check the 'Virtual Reality Supported' checkbox'. ";
                    if (XRSettings.supportedDevices.Contains("OpenVR") == false)
                        errorLog +=
                            "OpenVR is not in your list of supported virtual reality SDKs. Add it to the list in player settings. ";

                    errorLog += "To force OpenVR initialization call SteamVR.Initialize(true). ";

                    Debug.Log(errorLog);
                    SteamVR_Events.Initialized.Send(false);
                    return null;
                }

                // Verify common interfaces are valid.

                OpenVR.GetGenericInterface(OpenVR.IVRCompositor_Version, ref error);
                if (error != EVRInitError.None)
                {
                    ReportError(error);
                    SteamVR_Events.Initialized.Send(false);
                    return null;
                }

                OpenVR.GetGenericInterface(OpenVR.IVROverlay_Version, ref error);
                if (error != EVRInitError.None)
                {
                    ReportError(error);
                    SteamVR_Events.Initialized.Send(false);
                    return null;
                }

                settings = SteamVR_Settings.instance;

                SteamVR_Input.PreInitialize();

                if (Application.isEditor)
                    IdentifyApplication();

                SteamVR_Input.IdentifyActionsFile();

                if (SteamVR_Settings.instance.inputUpdateMode != SteamVR_UpdateModes.Nothing ||
                    SteamVR_Settings.instance.poseUpdateMode != SteamVR_UpdateModes.Nothing) SteamVR_Input.Initialize();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                SteamVR_Events.Initialized.Send(false);
                return null;
            }

            _enabled = true;
            SteamVR_Events.Initialized.Send(true);
            return new SteamVR();
        }

        private static void ReportError(EVRInitError error)
        {
            switch (error)
            {
                case EVRInitError.None:
                    break;
                case EVRInitError.VendorSpecific_UnableToConnectToOculusRuntime:
                    Debug.Log(
                        "[SteamVR] Initialization Failed!  Make sure device is on, Oculus runtime is installed, and OVRService_*.exe is running.");
                    break;
                case EVRInitError.Init_VRClientDLLNotFound:
                    Debug.Log(
                        "[SteamVR] Drivers not found!  They can be installed via Steam under Library > Tools.  Visit http://steampowered.com to install Steam.");
                    break;
                case EVRInitError.Driver_RuntimeOutOfDate:
                    Debug.Log("[SteamVR] Initialization Failed!  Make sure device's runtime is up to date.");
                    break;
                default:
                    Debug.Log(OpenVR.GetStringForHmdError(error));
                    break;
            }
        }

        public string GetTrackedDeviceString(uint deviceId)
        {
            var error = ETrackedPropertyError.TrackedProp_Success;
            var capacity = hmd.GetStringTrackedDeviceProperty(deviceId,
                ETrackedDeviceProperty.Prop_AttachedDeviceId_String, null, 0, ref error);
            if (capacity > 1)
            {
                var result = new StringBuilder((int) capacity);
                hmd.GetStringTrackedDeviceProperty(deviceId, ETrackedDeviceProperty.Prop_AttachedDeviceId_String,
                    result, capacity, ref error);
                return result.ToString();
            }

            return null;
        }

        public string GetStringProperty(ETrackedDeviceProperty prop, uint deviceId = OpenVR.k_unTrackedDeviceIndex_Hmd)
        {
            var error = ETrackedPropertyError.TrackedProp_Success;
            var capactiy = hmd.GetStringTrackedDeviceProperty(deviceId, prop, null, 0, ref error);
            if (capactiy > 1)
            {
                var result = new StringBuilder((int) capactiy);
                hmd.GetStringTrackedDeviceProperty(deviceId, prop, result, capactiy, ref error);
                return result.ToString();
            }

            return error != ETrackedPropertyError.TrackedProp_Success ? error.ToString() : "<unknown>";
        }

        public float GetFloatProperty(ETrackedDeviceProperty prop, uint deviceId = OpenVR.k_unTrackedDeviceIndex_Hmd)
        {
            var error = ETrackedPropertyError.TrackedProp_Success;
            return hmd.GetFloatTrackedDeviceProperty(deviceId, prop, ref error);
        }

        public static string GenerateAppKey()
        {
            var productName = Application.productName;
            if (string.IsNullOrEmpty(productName))
            {
                productName = "unnamed_product";
            }
            else
            {
                productName = Regex.Replace(Application.productName, "[^\\w\\._]", "");
                productName = productName.ToLower();
            }

            return string.Format(defaultUnityAppKeyTemplate, productName);
        }

        private static string GetManifestFile()
        {
            var currentPath = Application.dataPath;
            var lastIndex = currentPath.LastIndexOf('/');
            currentPath = currentPath.Remove(lastIndex, currentPath.Length - lastIndex);

            var fullPath = Path.Combine(currentPath, "unityProject.vrmanifest");

            if (File.Exists(fullPath))
            {
                var jsonText = File.ReadAllText(fullPath);
                var existingFile = JsonConvert.DeserializeObject<SteamVR_Input_ManifestFile>(jsonText);
                if (existingFile != null && existingFile.applications != null && existingFile.applications.Count > 0 &&
                    existingFile.applications[0].app_key != SteamVR_Settings.instance.editorAppKey)
                {
                    Debug.Log("[SteamVR] Deleting existing VRManifest because it has a different app key.");
                    var existingInfo = new FileInfo(fullPath);
                    if (existingInfo.IsReadOnly)
                        existingInfo.IsReadOnly = false;
                    existingInfo.Delete();
                }
            }

            if (File.Exists(fullPath) == false)
            {
                var manifestFile = new SteamVR_Input_ManifestFile();
                manifestFile.source = "Unity";
                var manifestApplication = new SteamVR_Input_ManifestFile_Application();
                manifestApplication.app_key = SteamVR_Settings.instance.editorAppKey;
                //manifestApplication.action_manifest_path = SteamVR_Settings.instance.actionsFilePath;
                manifestApplication.launch_type = "url";
                //manifestApplication.binary_path_windows = SteamVR_Utils.ConvertToForwardSlashes(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                //manifestApplication.binary_path_linux = SteamVR_Utils.ConvertToForwardSlashes(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                //manifestApplication.binary_path_osx = SteamVR_Utils.ConvertToForwardSlashes(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                manifestApplication.url = "steam://launch/";
                manifestApplication.strings.Add("en_us",
                    new SteamVR_Input_ManifestFile_ApplicationString
                        {name = string.Format("{0} [Testing]", Application.productName)});

                /*
                var bindings = new System.Collections.Generic.List<SteamVR_Input_ManifestFile_Application_Binding>();

                SteamVR_Input.InitializeFile();
                if (SteamVR_Input.actionFile != null)
                {
                    string[] bindingFiles = SteamVR_Input.actionFile.GetFilesToCopy(true);
                    if (bindingFiles.Length == SteamVR_Input.actionFile.default_bindings.Count)
                    {
                        for (int bindingIndex = 0; bindingIndex < bindingFiles.Length; bindingIndex++)
                        {
                            SteamVR_Input_ManifestFile_Application_Binding binding = new SteamVR_Input_ManifestFile_Application_Binding();
                            binding.binding_url = bindingFiles[bindingIndex];
                            binding.controller_type = SteamVR_Input.actionFile.default_bindings[bindingIndex].controller_type;
                            bindings.Add(binding);
                        }
                        manifestApplication.bindings = bindings;
                    }
                    else
                    {
                        Debug.LogError("[SteamVR] Mismatch in available binding files.");
                    }
                }
                else
                {
                    Debug.LogError("[SteamVR] Could not load actions file.");
                }
                */

                manifestFile.applications = new List<SteamVR_Input_ManifestFile_Application>();
                manifestFile.applications.Add(manifestApplication);

                var json = JsonConvert.SerializeObject(manifestFile, Formatting.Indented,
                    new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});

                File.WriteAllText(fullPath, json);
            }

            return fullPath;
        }

        private static void IdentifyApplication()
        {
            var isInstalled = OpenVR.Applications.IsApplicationInstalled(SteamVR_Settings.instance.editorAppKey);

            if (isInstalled == false)
            {
                var manifestPath = GetManifestFile();

                var addManifestErr = OpenVR.Applications.AddApplicationManifest(manifestPath, true);
                if (addManifestErr != EVRApplicationError.None)
                    Debug.LogError("Error adding vr manifest file: " + addManifestErr);
                else
                    Debug.Log("Successfully added vr manifest");
            }

            var processId = Process.GetCurrentProcess().Id;
            var applicationIdentifyErr =
                OpenVR.Applications.IdentifyApplication((uint) processId, SteamVR_Settings.instance.editorAppKey);

            if (applicationIdentifyErr != EVRApplicationError.None)
                Debug.LogError("Error identifying application: " + applicationIdentifyErr);
            else
                Debug.Log("Successfully identified application");
        }

        ~SteamVR()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            SteamVR_Events.Initializing.Remove(OnInitializing);
            SteamVR_Events.Calibrating.Remove(OnCalibrating);
            SteamVR_Events.OutOfRange.Remove(OnOutOfRange);
            SteamVR_Events.DeviceConnected.Remove(OnDeviceConnected);
            SteamVR_Events.NewPoses.Remove(OnNewPoses);

            _instance = null;
        }

        // Use this interface to avoid accidentally creating the instance in the process of attempting to dispose of it.
        public static void SafeDispose()
        {
            if (_instance != null)
                _instance.Dispose();
        }

#if UNITY_EDITOR
        public static void ShowBindingsForEditor()
        {
            var initOpenVR = !active && !usingNativeSupport;
            if (initOpenVR)
            {
                var error = EVRInitError.None;
                OpenVR.Init(ref error, EVRApplicationType.VRApplication_Utility);

                if (error != EVRInitError.None)
                    Debug.LogError("[SteamVR] Error during OpenVR Init: " + error);
            }

            var bindingFlagError = EVRSettingsError.None;
            OpenVR.Settings.SetBool(OpenVR.k_pch_SteamVR_Section, OpenVR.k_pch_SteamVR_DebugInputBinding, true,
                ref bindingFlagError);

            if (bindingFlagError != EVRSettingsError.None)
                Debug.LogError(
                    "[SteamVR] Error turning on the debug input binding flag in steamvr: " + bindingFlagError);

            if (Application.isPlaying == false)
            {
                IdentifyApplication();

                SteamVR_Input.IdentifyActionsFile();
            }

            /*
            GameObject tempObject = new GameObject("[Temp] [SteamVR Input]");
            SteamVR_Input.Initialize(tempObject);

            VRActiveActionSet_t[] sets = new VRActiveActionSet_t[SteamVR_Input.actionSets.Length];
            for (int setIndex = 0; setIndex < SteamVR_Input.actionSets.Length; setIndex++)
            {
                sets[setIndex].ulActionSet = SteamVR_Input.actionSets[setIndex].handle;
            }

            EVRInputError showBindingsError = OpenVR.Input.ShowBindingsForActionSet(sets, (uint)(sets.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(VRActiveActionSet_t))), 0);

            if (showBindingsError != EVRInputError.None)
            {
                Debug.LogError("Error showing bindings ui: " + showBindingsError.ToString());
            }
            */

            if (initOpenVR)
                OpenVR.Shutdown();

            Application.OpenURL("http://localhost:8998/dashboard/controllerbinding.html?app=" +
                                SteamVR_Settings.instance.editorAppKey.ToLower()); //todo: update with the actual call
        }

        public static string GetResourcesFolderPath(bool fromAssetsDirectory = false)
        {
            var asset = ScriptableObject.CreateInstance<SteamVR_Input_References>();
            var scriptAsset = MonoScript.FromScriptableObject(asset);

            var scriptPath = AssetDatabase.GetAssetPath(scriptAsset);

            var fi = new FileInfo(scriptPath);
            var rootPath = fi.Directory.Parent.ToString();

            var resourcesPath = Path.Combine(rootPath, "Resources");

            resourcesPath = resourcesPath.Replace("//", "/");
            resourcesPath = resourcesPath.Replace("\\\\", "\\");
            resourcesPath = resourcesPath.Replace("\\", "/");

            if (fromAssetsDirectory)
            {
                var assetsIndex = resourcesPath.IndexOf("/Assets/");
                resourcesPath = resourcesPath.Substring(assetsIndex + 1);
            }

            return resourcesPath;
        }
#endif

        #region Event callbacks

        private void OnInitializing(bool initializing)
        {
            SteamVR.initializing = initializing;
        }

        private void OnCalibrating(bool calibrating)
        {
            SteamVR.calibrating = calibrating;
        }

        private void OnOutOfRange(bool outOfRange)
        {
            SteamVR.outOfRange = outOfRange;
        }

        private void OnDeviceConnected(int i, bool connected)
        {
            SteamVR.connected[i] = connected;
        }

        private void OnNewPoses(TrackedDevicePose_t[] poses)
        {
            // Update eye offsets to account for IPD changes.
            eyes[0] = new SteamVR_Utils.RigidTransform(hmd.GetEyeToHeadTransform(EVREye.Eye_Left));
            eyes[1] = new SteamVR_Utils.RigidTransform(hmd.GetEyeToHeadTransform(EVREye.Eye_Right));

            for (var i = 0; i < poses.Length; i++)
            {
                var connected = poses[i].bDeviceIsConnected;
                if (connected != SteamVR.connected[i]) SteamVR_Events.DeviceConnected.Send(i, connected);
            }

            if (poses.Length > OpenVR.k_unTrackedDeviceIndex_Hmd)
            {
                var result = poses[OpenVR.k_unTrackedDeviceIndex_Hmd].eTrackingResult;

                var initializing = result == ETrackingResult.Uninitialized;
                if (initializing != SteamVR.initializing) SteamVR_Events.Initializing.Send(initializing);

                var calibrating =
                    result == ETrackingResult.Calibrating_InProgress ||
                    result == ETrackingResult.Calibrating_OutOfRange;
                if (calibrating != SteamVR.calibrating) SteamVR_Events.Calibrating.Send(calibrating);

                var outOfRange =
                    result == ETrackingResult.Running_OutOfRange ||
                    result == ETrackingResult.Calibrating_OutOfRange;
                if (outOfRange != SteamVR.outOfRange) SteamVR_Events.OutOfRange.Send(outOfRange);
            }
        }

        #endregion
    }
}