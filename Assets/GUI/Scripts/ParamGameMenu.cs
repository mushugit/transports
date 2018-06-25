using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ParamGameMenu : MonoBehaviour {

	public GameObject mainMenu;
	public Dropdown mapSize;

	public void Back()
	{
		gameObject.SetActive(false);
		mainMenu.SetActive(true);
	}

	public void Play()
	{
		SetSize();
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
	}

	public void SetSize()
	{
		switch (mapSize.value)
		{
			case 0:
				World.width = 10f;
				break;
			case 1:
				World.width = 15f;
				break;
			case 2:
				World.width = 20f;
				break;
		}
		World.height = World.width;
	}
}
