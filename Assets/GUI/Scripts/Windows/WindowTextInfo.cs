using UnityEngine;

public class WindowTextInfo : Window
{
	public City City { get; private set; }

	public WindowTextInfo(GameObject windowObject, Vector3 initialPosition, City c) : base(windowObject, initialPosition)
	{
		City = c;
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

	override public void Close()
	{
		City.InfoWindow = null;
	}
}

