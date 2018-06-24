using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class CityObjectRender : MonoBehaviour
{
	public City _City{ get; private set; }

	public void City(City city)
	{
		_City = city;
	}

	void Start()
    {
        var c = Random.ColorHSV();

		var renderers = GetComponentsInChildren<Renderer>();
		foreach(Renderer r in renderers)
		{
			r.material.color = c;
		}
    }
}
