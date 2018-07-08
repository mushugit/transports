using System;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class PauseMenu : MonoBehaviour
{

	public GameObject PauseMenuObject;
	public GameObject LoadMenuObject;
	public static bool GameIsPaused = false;

	private static PauseMenu instance;

	float timeScale;

	private void Start()
	{
		timeScale = Time.timeScale;
		Resume();
		instance = this;
	}

	private void Update()
	{
		if (World.gameLoading)
			return;


		if (Input.GetButtonDown("Pause"))
		{
			if (GameIsPaused)
			{
				if (!LoadMenuObject.activeSelf)
				{
					Resume();
				}
			}
			else
			{
				if (Builder.IsBuilding || Builder.IsDestroying)
					Builder.CancelAction();
				else
					Pause();
			}
		}
	}

	public static void ForceResume()
	{
		instance?.Resume();
	}

	public void Resume()
	{
		PauseMenuObject.SetActive(false);
		Time.timeScale = timeScale;
		GameIsPaused = false;
	}

	public void Save()
	{
		SaveHandler.Save();
		Resume();
	}

	public void Load()
	{
		PauseMenuObject.SetActive(false);
		LoadMenuObject.SetActive(true);
	}

	void Pause()
	{
		//Debug.Log("Pause");
		timeScale = Time.timeScale;
		PauseMenuObject.SetActive(true);
		Time.timeScale = 0f;
		GameIsPaused = true;

	}

	public void Quit()
	{
		Application.Quit();
	}
}
