using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCamera : MonoBehaviour {

	public Shader minimapShader;

	// Use this for initialization
	void Start () {
		transform.position = new Vector3(World.width / 2f, 20f, World.height / 2f);
		Camera c = GetComponent<Camera>();
		c.SetReplacementShader(minimapShader,"");
		c.orthographicSize = Mathf.Max(World.width, World.height) / 2f;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown("Screenshot"))
		{
			Screenshot.Take(Screen.width, Screen.height);
		}
	}
}
