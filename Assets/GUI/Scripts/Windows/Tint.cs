using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tint : MonoBehaviour {

	public void SetColor(Color color)
	{
		var r = GetComponent<Image>();
		r.color = color;
		Debug.Log(r.color);
	}

	public void SetActive(bool activeState)
	{
		gameObject.SetActive(activeState);
	}
}
