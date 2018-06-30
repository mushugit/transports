using UnityEngine;


public abstract class Window
{
	protected string title;
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

	protected readonly GameObject windowObject;
	protected readonly WindowUI ui;

	protected readonly Vector2 startingPosition;

	public Window(GameObject windowObject, Vector3 initialPosition)
	{
		this.windowObject = windowObject;
		ui = windowObject.GetComponent<WindowUI>();

		windowObject.transform.position = initialPosition;
	}



	abstract public void Close();

}

