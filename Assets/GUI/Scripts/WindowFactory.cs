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

	public static Window BuildTextInfo(string title, Vector3 position, City c)
	{
		return instance._BuildTextInfo(title,position, c);
	}

	public Window _BuildTextInfo(string title, Vector3 position, City c)
	{
		position.z = 0;
		var windowCanvasObject = Instantiate(windowTextInfoPrefab, parent.transform);

		var windowObjectRef = windowCanvasObject.GetComponentInChildren<WindowReferencer>();
		var windowObject = windowObjectRef.GameObjectWindow;

		var wtc = windowObject.GetComponent<WindowTextContent>();
		wtc.ContentText(c.InfoText());

		var w = new Window(windowObject, position, c)
		{
			Title = title
		};
		windowObjectRef.Window = w;

		windows.Add(w);

		return w;
	}

	public void CloseAll()
	{

	}
}

