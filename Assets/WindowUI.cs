using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowUI : MonoBehaviour {

	public TextMeshProUGUI title;

	public void Title(string text)
	{
		title.text = text;
	}
}
