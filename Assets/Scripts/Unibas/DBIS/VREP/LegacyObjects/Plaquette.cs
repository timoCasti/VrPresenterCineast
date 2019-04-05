using UnityEngine;
using UnityEngine.UI;

public class Plaquette : MonoBehaviour
{
    public Font font;
    public Text text;

    // Use this for initialization
    private void Start()
    {
        if (font != null) text.font = font;
    }
}