using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationalHighlight : MonoBehaviour {

	public void Rotate(bool clockwise)
	{
		var p = transform.position;
		//p.x += 0.5f;
		//p.z += 0.5f;
		Debug.Log(p);
		var angle = -90;
		if (!clockwise)
			angle = -angle;
		transform.RotateAround(p, Vector3.up, angle);
	}
}
