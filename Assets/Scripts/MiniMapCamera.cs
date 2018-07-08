using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCamera : MonoBehaviour {

	public Shader minimapShader;

	private static MiniMapCamera instance;

	public void Awake()
	{
		instance = this;
	}

	public static void UpdateRender()
	{
		instance.transform.position = new Vector3(World.width / 2f, 20f, World.height / 2f);
		Camera c = instance.GetComponent<Camera>();
		c.SetReplacementShader(instance.minimapShader, "");
		c.orthographicSize = Mathf.Max(World.width, World.height) / 2f;
	}

	void Start () {
		UpdateRender();
	}
	
	void Update () {
		if (Input.GetButtonDown("Screenshot"))
		{
			Screenshot.Take(Screen.width, Screen.height);
		}
	}
}
