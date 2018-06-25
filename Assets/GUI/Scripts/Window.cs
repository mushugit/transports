using UnityEngine;


public class Window
{
	private string title;
	public string Title
	{
		get { return title; }
		set
		{
			title = value;
			if (ui != null)
				ui.Title(title);
		}
	}

	public City City { get; private set; }

	private readonly GameObject windowObject;
	private readonly WindowUI ui;

	readonly Vector2 startingPosition;

	public Window(GameObject windowObject, Vector3 initialPosition, City c)
	{
		this.windowObject = windowObject;
		City = c;
		ui = windowObject.GetComponent<WindowUI>();

		windowObject.transform.position = initialPosition;
	}

	public void TextContent(string richTextContent)
	{
		if (windowObject != null)
		{
			var wtc = windowObject.GetComponent<WindowTextContent>();
			if (wtc != null)
			{
				wtc.ContentText(richTextContent);
			}
		}
	}

	public void Close()
	{
		City.InfoWindow = null;
	}

}

