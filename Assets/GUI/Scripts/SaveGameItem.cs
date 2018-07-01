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
		string errorMessage;
		if (!SaveHandler.Load(fullName, out errorMessage))
		{
			var loadMenu = GetComponentInParent<LoadMenu>();
			loadMenu.Back();
			var pauseMenu = GetComponentInParent<PauseMenu>();
			pauseMenu.Resume();
			Message.ShowError("Erreur de chargement de la sauvegarde", errorMessage);
		}
	}
}
