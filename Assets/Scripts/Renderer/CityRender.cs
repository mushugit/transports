using UnityEngine;
using UnityEngine.UI;

public class CityRender : MonoBehaviour
{

	public GUISkin GuiSkin;

	public Text UILabel;

	string label;

	public void Label(string name)
	{
		UILabel.text = " " + name + " ";
	}

	public static Component Build(Vector3 position, Component cityPrefab) //TODO:Déplacer dans générique Render
	{
		return Instantiate(cityPrefab, position, Quaternion.identity);
	}
}
