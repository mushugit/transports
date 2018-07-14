using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class CityObjectRenderer : MonoBehaviour, IUnityObjectRenderer
{
	public Construction NestedConstruction{ get; set; }

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
