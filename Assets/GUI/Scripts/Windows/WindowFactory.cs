using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

class WindowFactory : MonoBehaviour
{
	public GameObject windowTextInfoPrefab;
	public GameObject windowFluxSetupPrefab;
	public GameObject parent;

	private static WindowFactory instance;
	private static List<Window> windows;

	private void Start()
	{
		windows = new List<Window>();
		instance = this;
	}

	public static WindowTextInfo BuildTextInfo(string title, Vector3 position, City c)
	{
		return instance._BuildTextInfo(title,position, c);
	}

	public static WindowTextInfo BuildTextInfo(string title, Vector3 position, string message, Color tint)
	{
		return instance._BuildTextInfo(title, position, message,tint);
	}

	public static WindowFluxSetup BuildFluxSetup(string title, Vector3 position)
	{
		return instance._BuildFluxSetup(title, position);
	}

	public WindowFluxSetup _BuildFluxSetup(string title, Vector3 position)
	{
		position.z = 0;
		var windowCanvasObject = Instantiate(windowFluxSetupPrefab, parent.transform);

		var windowObjectRef = windowCanvasObject.GetComponentInChildren<WindowReferencer>();
		var windowObject = windowObjectRef.GameObjectWindow;

		var fluxContent = windowObject.GetComponent<WindowFluxContent>();
		var cityNames = World.Instance.Cities.Select(c => c.Name).ToList();
		fluxContent.source.AddOptions(cityNames);
		fluxContent.target.AddOptions(cityNames);

		fluxContent.target.value = 1;
		var managerTarget = fluxContent.target.GetComponent<DropdownManager>();
		managerTarget.DisableItem(0);
		var managerSource = fluxContent.source.GetComponent<DropdownManager>();
		managerSource.DisableItem(1);


		var w = new WindowFluxSetup(windowObject, position)
		{
			Title = title
		};
		windowObjectRef.Window = w;

		windows.Add(w);

		return w;
	}

	public WindowTextInfo _BuildTextInfo(string title, Vector3 position, City c)
	{
		position.z = 0;
		var windowCanvasObject = Instantiate(windowTextInfoPrefab, parent.transform);

		var windowObjectRef = windowCanvasObject.GetComponentInChildren<WindowReferencer>();
		var windowObject = windowObjectRef.GameObjectWindow;

		var wtc = windowObject.GetComponent<WindowTextContent>();
		wtc.ContentText(c.InfoText());

		var w = new WindowTextInfo(windowObject, position, c)
		{
			Title = title
		};
		windowObjectRef.Window = w;

		windows.Add(w);

		return w;
	}

	public WindowTextInfo _BuildTextInfo(string title, Vector3 position, string message, Color tint)
	{
		position.z = 0;
		var windowCanvasObject = Instantiate(windowTextInfoPrefab, parent.transform);

		var windowObjectRef = windowCanvasObject.GetComponentInChildren<WindowReferencer>();
		var windowObject = windowObjectRef.GameObjectWindow;

		var tintObject = windowCanvasObject.GetComponentInChildren<Tint>(true);
		tintObject.SetColor(tint);
		tintObject.SetActive(true);

		var wtc = windowObject.GetComponent<WindowTextContent>();
		wtc.ContentText(message);

		var w = new WindowTextInfo(windowObject, position, null)
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

