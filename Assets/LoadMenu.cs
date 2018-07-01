using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadMenu : MonoBehaviour
{
	public GameObject PauseMenuObject;
	public GameObject LoadMenuObject;

	public Transform savegameList;
	public GameObject saveGameItemPrefab;

	private void Start()
	{
		AddAllItems(SaveHandler.ListSaveGames());
	}

	private void Update()
	{
		if (Input.GetButtonDown("Pause"))
		{
			Back();
		}
	}

	public void Back()
	{
		LoadMenuObject.SetActive(false);
		PauseMenuObject.SetActive(true);
	}

	private void AddItem(string text)
	{
		var savegameItem = Instantiate(saveGameItemPrefab, savegameList);
		var item = savegameItem?.GetComponent<SaveGameItem>();
		item?.Text(text,$"{text}{SaveHandler.Extention}");
	}

	private void AddAllItems(string[] texts)
	{
		foreach(string t in texts)
		{
			AddItem(t);
		}
	}
}
