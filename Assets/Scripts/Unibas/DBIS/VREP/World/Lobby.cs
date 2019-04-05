using DefaultNamespace;
using UnityEngine;
using World;

public class Lobby : MonoBehaviour
{
    // Use this for initialization
    private void Start()
    {
        var tp = SteamVRTeleportButton.Create(gameObject, new Vector3(0, 0, 4.5f), Vector3.zero,
            new SteamVRTeleportButton.TeleportButtonModel(0.1f, .02f, 1f, TexturingUtility.LoadMaterialByName("none"),
                TexturingUtility.LoadMaterialByName("NMetal"), TexturingUtility.LoadMaterialByName("NPlastic")),
            "Text");
        var tp2 = SteamVRTeleportButton.Create(gameObject, new Vector3(.5f, 0, 4.5f), Vector3.zero,
            new SteamVRTeleportButton.TeleportButtonModel(0.1f, .02f, 1f, TexturingUtility.LoadMaterialByName("NWood"),
                TexturingUtility.LoadMaterialByName("NMetal"), TexturingUtility.LoadMaterialByName("NPlastic")),
            Resources.Load<Sprite>("Sprites/UI/chevron-right"));
        var tp3 = SteamVRTeleportButton.Create(gameObject, new Vector3(0, 1.5f, 4.98f), Vector3.zero,
            new SteamVRTeleportButton.TeleportButtonModel(0.1f, .02f, 1f, TexturingUtility.LoadMaterialByName("none"),
                TexturingUtility.LoadMaterialByName("NMetal"), TexturingUtility.LoadMaterialByName("NPlastic"), false),
            "Wall");
    }


    // Update is called once per frame
    private void Update()
    {
    }
}