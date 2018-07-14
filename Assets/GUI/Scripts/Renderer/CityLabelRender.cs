using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityLabelRender : MonoBehaviour {

	public GameObject city;

	void OnGUI()
	{
		var point = Camera.main.WorldToScreenPoint(city.transform.position);
		transform.position = point;
	}
}
