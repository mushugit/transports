using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cam : MonoBehaviour {
	Vector3 defaultPosition;

	public float moveSpeed = 30f;

	// Use this for initialization
	void Start () {
		defaultPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey (KeyCode.Escape))
			Application.Quit ();

		var r = transform.rotation;
		transform.rotation = new Quaternion(0f, 0f, 0f, 0f);

		float hMove =  Input.GetAxis ("Horizontal") + Input.GetAxis ("Vertical");
		float vMove = -Input.GetAxis ("Horizontal") + Input.GetAxis ("Vertical") ;

		transform.Translate(new Vector3(hMove*moveSpeed*Time.deltaTime,0,vMove*moveSpeed*Time.deltaTime));

		if (Input.GetButton ("Reset"))
			transform.position = defaultPosition;

		transform.rotation = r;

		transform.Translate (new Vector3 (0f, 0f, Input.GetAxis ("Zoom") * Time.deltaTime));

	}
}
