using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour {
	public Component cellPrefab;
	public Component cityPrefab;

	public float width = 10f;
	public float height = 10f;

	private Construction[,] map;
	private List<City> cities;

	void Start () {
		Generate ();
		StartCoroutine ("Simulation");
	}

	void Generate() {
		var w = (int)width;
		var h = (int)height;
		map = new Construction [w,h]; 

		Cities (w, h);

		for (float x = 0f; x < width; x++) {
			for (float y = 0f; y < height; y++) {
				Terrain (x, y);			
			}
		}
	}

	IEnumerator Simulation() {
		while (true) {
			foreach (City c in cities) {
				c.GenerateCargo ();
			}
			yield return new WaitForSeconds (0.02f);
		}
	}

	void Terrain(float x, float y) {
		Instantiate(cellPrefab,new Vector3 (x,-0.05f,y),Quaternion.identity);
	}

	void Cities(int w, int h){
		var quantity = City.Quantity(w,h);
		cities = new List<City> (quantity);
		var n = 0;
		while(n<quantity){
			int x = Random.Range (0, w);
			int y = Random.Range (0, h);
			if (map [x, y] == null) {
				City c = new City (x,y, cityPrefab);
				map [x, y] = c;
				cities.Add (c);
				n++;
			}
		}
	}
}
