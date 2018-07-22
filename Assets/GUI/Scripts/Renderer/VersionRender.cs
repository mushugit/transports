using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VersionRender : MonoBehaviour {

	void OnGUI()
	{
		GUI.Label(new Rect(Screen.width - 80, Screen.height - 30, 75, 25), Application.version);
	}
}
