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

	private readonly GameObject windowObject;
	private readonly WindowUI ui;

	readonly Vector2 startingPosition;

	public Window(GameObject windowObject, Vector3 initialPosition)
	{
		this.windowObject = windowObject;

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

}

