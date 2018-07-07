using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicInfo : MonoBehaviour {

	public Text MusicNameLabel;
	public GameObject MusicNameDisplay;

	public static float MusicNameStayDelay { get; private set; } = 5;

	private static MusicInfo instance;

	private string currentName;

	private void Awake()
	{
		instance = this;
	}

	private void Update()
	{
		if (Input.GetButton("MusicName"))
			ShowName();
	}

	public static void DisplayName(string name)
	{
		instance.currentName = name;
		instance.ShowName();
	}

	private void ShowName()
	{
		instance.MusicNameDisplay.SetActive(true);
		instance.MusicNameLabel.text = $" ♫ {currentName} ♫ ";
		instance.StartCoroutine(instance.HideName());

	}

	private IEnumerator HideName()
	{
		yield return new WaitForSeconds(MusicNameStayDelay);
		MusicNameDisplay.SetActive(false);
		yield return null;
	}
}
