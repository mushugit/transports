using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfoText : MonoBehaviour
{

	public TextMeshProUGUI textHolder;
	private static readonly float delay = 2f;
	private static IEnumerator clearCoroutine = null;

	public static InfoText instance;

	private void Start()
	{
		instance = this;
	}
	public static void Display(string message)
	{
		Display(message, delay);
	}

	public static void Display(string message, float displayDelay)
	{
		if (clearCoroutine != null)
			instance.StopCoroutine(clearCoroutine);
		Clear();
		instance.textHolder.gameObject.SetActive(true);
		instance.textHolder.text = message;
		clearCoroutine = instance.RemoteClear(displayDelay);
		instance.StartCoroutine(clearCoroutine);
	}

	public static void Clear()
	{
		instance.textHolder.gameObject.SetActive(false);
		instance.textHolder.text = "";
		clearCoroutine = null;
	}

	IEnumerator RemoteClear(float displayDelay)
	{
		yield return new WaitForSeconds(displayDelay);
		Clear();
		yield return null;
	}


}
