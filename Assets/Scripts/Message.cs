using UnityEngine;

public class Message
{
	public static readonly Color ErrorColor = new Color(1, 0, 0, 0.2f);

	public static void ShowError(string errorTitle, string errorText)
	{
		var center = new Vector3(Screen.width / 2, Screen.height / 2, 0);
		WindowFactory.BuildTextInfo(errorTitle, center, errorText, ErrorColor);
	}
}

