using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityObjectRender : MonoBehaviour
{
	City city;

	public void City(City city)
	{
		this.city = city;
	}

    void Start()
    {
        var c = Random.ColorHSV();
        GetComponent<Renderer>().material.color = c;
    }

	void OnMouseDown()
	{
		city.ShowInfo();
	}
}
