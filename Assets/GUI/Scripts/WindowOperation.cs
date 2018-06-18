using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowOperation : MonoBehaviour {

	public void Close()
	{
		Debug.Log("Closing window");
		DestroyImmediate(gameObject);
	}
}
