using UnityEngine;
using UnityEngine.UI;

public class IndustryRender : MonoBehaviour, IUnityLabelable
{

    public GUISkin GuiSkin;

    public Text UILabel;

    public void Label(string name)
    {
        UILabel.text = " " + name + " ";
    }
}
