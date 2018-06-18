using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class WindowFactory : MonoBehaviour
{
	public GameObject windowTextInfoPrefab;
	public GameObject parent;

	private static WindowFactory instance;
	private static List<Window> windows;

	private void Start()
	{
		windows = new List<Window>();
		instance = this;
	}

	public static Window BuildTextInfo(string title, Vector3 position, string richTextContent)
	{
		return instance._BuildTextInfo(title,position, richTextContent);
	}

	public Window _BuildTextInfo(string title, Vector3 position, string richTextContent)
	{
		position.z = 0;
		var windowCanvasObject = Instantiate(windowTextInfoPrefab, parent.transform);

		var windowObjectRef = windowCanvasObject.GetComponentInChildren<WindowReferencer>();
		var windowObject = windowObjectRef.window;

		var wtc = windowObject.GetComponent<WindowTextContent>();
		wtc.ContentText(richTextContent);

		var w = new Window(windowObject, position)
		{
			Title = title
		};

		windows.Add(w);

		return w;
	}

	public void CloseAll()
	{

	}
}

