using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepotObjectRender : MonoBehaviour {

	private Depot _depot;

	public void Depot(Depot depot)
	{
		_depot = depot;
	}

	void Start () {
		var c = Random.ColorHSV();

		var renderers = GetComponentsInChildren<Renderer>();
		foreach (Renderer r in renderers)
		{
			r.material.color = c;
		}
	}

}
