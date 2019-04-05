﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using UnityEngine;

namespace Valve.VR
{
    /// <summary>
    ///     A list of the actions in an action set. Restricted per Action Direction.
    /// </summary>
    public abstract class SteamVR_Action_List : ScriptableObject
    {
        public SteamVR_Action[] actions;
        public SteamVR_ActionSet actionSet;
        public SteamVR_ActionDirections listDirection;
    }
}