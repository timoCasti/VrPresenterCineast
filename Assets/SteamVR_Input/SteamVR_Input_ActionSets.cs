// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 2.0.50727.1433
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace Valve.VR
{
    using System;
    using UnityEngine;
    
    
    public partial class SteamVR_Actions
    {
        
        private static SteamVR_Input_ActionSet_vrtk p_vrtk;
        
        public static SteamVR_Input_ActionSet_vrtk vrtk
        {
            get
            {
                return SteamVR_Actions.p_vrtk.GetCopy <SteamVR_Input_ActionSet_vrtk>();
            }
        }
        
        private static void StartPreInitActionSets()
        {
            SteamVR_Actions.p_vrtk = ((SteamVR_Input_ActionSet_vrtk)(SteamVR_ActionSet.Create <SteamVR_Input_ActionSet_vrtk>("/actions/vrtk")));
            Valve.VR.SteamVR_Input.actionSets = new Valve.VR.SteamVR_ActionSet[]
            {
                    SteamVR_Actions.vrtk};
        }
    }
}
