using TMPro;
using UnityEngine;

public class SaveGameItemMainMenu : MonoBehaviour {

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
			var mainMenu = loadMenu.PauseMenuObject.GetComponent<MainMenu>();
			mainMenu.ErrorLoading(errorMessage);
		}
	}
}
