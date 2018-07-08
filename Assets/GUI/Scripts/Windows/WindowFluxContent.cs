using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindowFluxContent : MonoBehaviour {

	public Dropdown source;
	public Dropdown target;

	public void AddFlux()
	{
		var citySource = World.Instance.Cities[source.value];
		var cityTarget = World.Instance.Cities[target.value];

		Simulation.AddFlux(citySource, cityTarget);
	}

}
