using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LabelOperation : MonoBehaviour, IPointerClickHandler
{
	public CityObjectRender cityObject;

	public void OnPointerClick(PointerEventData eventData)
	{
		cityObject._City.ShowInfo();
	}
}
