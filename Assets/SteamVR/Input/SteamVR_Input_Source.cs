//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Valve.VR
{
    public static class SteamVR_Input_Source
    {
        private static Dictionary<SteamVR_Input_Sources, ulong> inputSourceHandlesBySource =
            new Dictionary<SteamVR_Input_Sources, ulong>(new SteamVR_Input_Sources_Comparer());

        private static Dictionary<ulong, SteamVR_Input_Sources> inputSourceSourcesByHandle =
            new Dictionary<ulong, SteamVR_Input_Sources>();

        private static readonly Type enumType = typeof(SteamVR_Input_Sources);
        private static readonly Type descriptionType = typeof(DescriptionAttribute);

        private static readonly SteamVR_Input_Sources[] updateSources =
            {SteamVR_Input_Sources.LeftHand, SteamVR_Input_Sources.RightHand, SteamVR_Input_Sources.Any};

        public static ulong GetHandle(SteamVR_Input_Sources inputSource)
        {
            if (inputSourceHandlesBySource.ContainsKey(inputSource))
                return inputSourceHandlesBySource[inputSource];

            return 0;
        }

        public static SteamVR_Input_Sources[] GetUpdateSources()
        {
            return updateSources;
        }

        private static string GetPath(string inputSourceEnumName)
        {
            return ((DescriptionAttribute) enumType.GetMember(inputSourceEnumName)[0]
                .GetCustomAttributes(descriptionType, false)[0]).Description;
        }

        public static void Initialize()
        {
            var enumNames = Enum.GetNames(enumType);
            inputSourceHandlesBySource =
                new Dictionary<SteamVR_Input_Sources, ulong>(new SteamVR_Input_Sources_Comparer());
            inputSourceSourcesByHandle = new Dictionary<ulong, SteamVR_Input_Sources>();

            for (var enumIndex = 0; enumIndex < enumNames.Length; enumIndex++)
            {
                var path = GetPath(enumNames[enumIndex]);

                ulong handle = 0;
                var err = OpenVR.Input.GetInputSourceHandle(path, ref handle);

                if (err != EVRInputError.None)
                    Debug.LogError("GetInputSourceHandle (" + path + ") error: " + err);

                if (enumNames[enumIndex] == SteamVR_Input_Sources.Any.ToString()) //todo: temporary hack
                {
                    inputSourceHandlesBySource.Add((SteamVR_Input_Sources) enumIndex, 0);
                    inputSourceSourcesByHandle.Add(0, (SteamVR_Input_Sources) enumIndex);
                }
                else
                {
                    inputSourceHandlesBySource.Add((SteamVR_Input_Sources) enumIndex, handle);
                    inputSourceSourcesByHandle.Add(handle, (SteamVR_Input_Sources) enumIndex);
                }
            }
        }
    }
}