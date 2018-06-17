using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class WindowFactory : MonoBehaviour
{
	static WindowFactory instance;

	static int uid = int.MinValue;
	static Dictionary<int, Window> windows;

	private void Start()
	{
		instance = this;
		windows = new Dictionary<int, Window>();
	}

	public static Window Build(string title, Vector3 position)
	{
		var w = new Window(position);
		windows.Add(uid, w);
		uid++;

		return w;
	}

	void DisplayWindow(int windowUid)
	{
		//Draw content
	}

	private void OnGUI()
	{
		foreach (KeyValuePair<int,Window> w in windows)
		{
			GUI.Window(w.Key, w.Value.Rect(), DisplayWindow, w.Value.Title);
		}
	}
}

