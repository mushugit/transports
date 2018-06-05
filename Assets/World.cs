using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour {
	public Material grassMaterial;
	// Use this for initialization
	void Start () {
		var cellSize = new Vector3(1F, 0.05F, 1F);
		const float width = 10F;
		const float height = 10F;

		Debug.Log (grassMaterial.name);

		for (float x = 0.5F; x < width; x++) {
			for (float y = 0.5F; y < height; y++) {
				GameObject cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
				cube.transform.position = new Vector3 (x,0F,y);
				cube.transform.localScale = cellSize;
				cube.GetComponent<Renderer>().material = grassMaterial;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	}
}
