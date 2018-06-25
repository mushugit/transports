using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowOperation : MonoBehaviour {

	public void Close()
	{
		var referencer = GetComponentInParent<WindowReferencer>();
		referencer.Window.Close();
		DestroyImmediate(gameObject);
	}
}
