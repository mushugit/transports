using UnityEngine;


class Window
{
	static readonly Vector2 size = new Vector2(150f, 100f);

	public string Title { get; set; }

	readonly Vector2 startingPosition;

	public Window(Vector3 startingPosition)
	{
		this.startingPosition = new Vector2(startingPosition.x, startingPosition.y);
	}

	public Window(Vector2 startingPosition)
	{
		this.startingPosition = startingPosition;
	}

	public Rect Rect()
	{
		return new Rect(startingPosition, size);
	}
}

