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
    public partial class SteamVR_Input
    {
        public static SteamVR_Input_ActionSet_default _default;

        public static SteamVR_Input_ActionSet_platformer platformer;

        public static SteamVR_Input_ActionSet_buggy buggy;

        public static void Dynamic_InitializeActionSets()
        {
            _default.Initialize();
            platformer.Initialize();
            buggy.Initialize();
        }

        public static void Dynamic_InitializeInstanceActionSets()
        {
            _default = (SteamVR_Input_ActionSet_default) SteamVR_Input_References.GetActionSet("_default");
            platformer = (SteamVR_Input_ActionSet_platformer) SteamVR_Input_References.GetActionSet("platformer");
            buggy = (SteamVR_Input_ActionSet_buggy) SteamVR_Input_References.GetActionSet("buggy");
            actionSets = new SteamVR_ActionSet[]
            {
                _default,
                platformer,
                buggy
            };
        }
    }
}