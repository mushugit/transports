using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CityObjectRender : MonoBehaviour, IPointerClickHandler
{
	public City _City{ get; private set; }

	public void City(City city)
	{
		_City = city;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Debug.Log($"City {_City.Name} clicked");
	}

	void Start()
    {
        var c = Random.ColorHSV();
        GetComponent<Renderer>().material.color = c;
    }
}
