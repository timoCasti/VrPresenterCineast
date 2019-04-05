//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using UnityEngine;

namespace Valve.VR
{
    /// <summary>
    ///     Automatically activates an action set on Start() and deactivates the set on OnDestroy(). Optionally deactivating
    ///     all other sets as well.
    /// </summary>
    public class SteamVR_ActivateActionSetOnLoad : MonoBehaviour
    {
        [SteamVR_DefaultActionSet("default")] public SteamVR_ActionSet actionSet;

        public bool activateOnStart = true;
        public bool deactivateOnDestroy = true;

        public bool disableAllOtherActionSets;


        private void Start()
        {
            if (actionSet != null && activateOnStart) actionSet.ActivatePrimary(disableAllOtherActionSets);
        }

        private void OnDestroy()
        {
            if (actionSet != null && deactivateOnDestroy) actionSet.Deactivate();
        }
    }
}