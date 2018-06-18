using TMPro;
using UnityEngine;

public class WindowUI : MonoBehaviour {

	public TextMeshProUGUI title;

	public void Title(string text)
	{
		title.text = text;
	}
}
