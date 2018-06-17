using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleBarRender : MonoBehaviour {

	public GameObject window;

	Vector3 dragStartPosition;

	private void OnMouseDown()
	{
		Debug.Log("Start Draging");
		window.transform.position = dragStartPosition;
	}

	private void OnMouseDrag()
	{
		Debug.Log("Draging");
	}
}
