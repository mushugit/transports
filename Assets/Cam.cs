using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cam : MonoBehaviour {

	public float cameraMoveSpeed = 30f;
	public float cameraZoomSpeed = 100f;

	Vector3 defaultPosition;

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

		if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.Q))
			transform.Translate(new Vector3(-cameraMoveSpeed*Time.deltaTime,0,cameraMoveSpeed*Time.deltaTime));

		if(Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
			transform.Translate(new Vector3(cameraMoveSpeed*Time.deltaTime,0,-cameraMoveSpeed*Time.deltaTime));

		if (Input.GetKey (KeyCode.UpArrow) || Input.GetKey (KeyCode.Z))
			transform.Translate(new Vector3 (cameraMoveSpeed * Time.deltaTime, 0, cameraMoveSpeed * Time.deltaTime));

		if(Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
			transform.Translate(new Vector3(-cameraMoveSpeed*Time.deltaTime,0,-cameraMoveSpeed*Time.deltaTime));

		if (Input.GetKey (KeyCode.R))
			transform.position = defaultPosition;

		transform.rotation = r;

		if (Input.GetAxis ("Mouse ScrollWheel") < 0)
			transform.Translate (new Vector3 (0f, 0f, -cameraZoomSpeed * Time.deltaTime));

		if (Input.GetAxis ("Mouse ScrollWheel") > 0)
			transform.Translate (new Vector3 (0f, 0f, cameraZoomSpeed * Time.deltaTime));
	}
}
