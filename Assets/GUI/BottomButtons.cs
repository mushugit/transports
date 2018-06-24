using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomButtons : MonoBehaviour {

	public void BuildCity()
	{
		Builder.City();
	}

	public void BuildRoad()
	{
		Builder.Road();
	}

	public void Bulldoze()
	{
		Builder.Bulldoze();
	}

	public void BuildDepot()
	{
		Builder.Depot();
	}
}
