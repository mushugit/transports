using UnityEngine;
using System.Collections.Generic;

public class City : Construction
{
	Component cityRender;
	public Point p  {get;}
	public string name { get; }
	public float cargoLevel  {get;}

	public static int Quantity(int w, int h) {
		return Mathf.RoundToInt (Mathf.Sqrt (Mathf.Sqrt ((float)(w * h)))) + 1;
	}

	public City(Point position,Component cityPrefab){
		p = position;
		cityRender = CityRender.Build (new Vector3(p.x+0.5f,0.5f,p.y+0.5f), cityPrefab);
		name = "Ville [" + p.x + "," + p.y + "]";
		cityRender.SendMessage ("Label", name);

		cargoLevel = 0.3f;
	}

	public bool IsLinked(City c){
		return true;
	}

	public int GenerateCargo(){
		int quantity = 0;
		if (Random.value > cargoLevel) {
			//Debug.Log ("Cargo généré pour " + name);
			quantity++;
		}
		return quantity;
	}
}


