using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SaveGameItem : MonoBehaviour {

	public TextMeshProUGUI textItem;

	private string fullName;

	public void Text(string displayName, string fullName)
	{
		textItem.text = displayName;
		this.fullName = fullName;
	}

	public void Load()
	{
		SaveHandler.Load(fullName);
	}
}
