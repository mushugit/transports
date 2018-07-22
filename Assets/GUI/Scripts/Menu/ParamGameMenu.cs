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
			default:
			case 0:
				World.width = 10;
				break;
			case 1:
				World.width = 20;
				break;
			case 2:
				World.width = 50;
				break;
			case 3:
				World.width = 100;
				break;
			case 4:
				World.width = 200;
				break;
			case 5:
				World.width = 500;
				break;

		}
		World.height = World.width;
	}
}
