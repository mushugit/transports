using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

	public GameObject paramGameMenu;
	public GameObject loadMenu;
	public TextMeshProUGUI errorLoadingText;

    public void Play()
    {
		ErrorLoading("");
		gameObject.SetActive(false);
		paramGameMenu.SetActive(true);
	}

	public void Load()
	{
		ErrorLoading("");
		gameObject.SetActive(false);
		loadMenu.SetActive(true);
	}

	public void Quit()
    {
		ErrorLoading("");
		Debug.Log("Quit");
        Application.Quit();
    }

	public void ErrorLoading(string message)
	{
		errorLoadingText.text = message;
	}
}
