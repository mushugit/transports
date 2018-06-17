using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour {

    public GameObject PauseMenuObject;
    public static bool GameIsPaused = false;

	float timeScale;

	private void Start()
	{
		timeScale = Time.timeScale;
		Resume();
	}

	private void Update()
    {
        if (World.gameLoading)
            return;

        if (Input.GetButtonDown("Pause"))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        PauseMenuObject.SetActive(false);
		Time.timeScale = timeScale;
		GameIsPaused = false;
    }

    void Pause()
    {
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
