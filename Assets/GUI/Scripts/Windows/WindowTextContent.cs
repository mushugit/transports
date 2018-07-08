using UnityEngine;
using TMPro;

public class WindowTextContent : MonoBehaviour {

	public TextMeshProUGUI contentText;

	public void ContentText(string text)
	{
		contentText.text = text;
	}
}
