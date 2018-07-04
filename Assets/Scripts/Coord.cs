using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;


public class Coord
{
	public static int minX;
	public static int maxX;
	public static int minY;
	public static int maxY;

	[JsonProperty]
	public int X { get; }
	[JsonProperty]
	public int Y { get; }

	static Coord()
	{
		minX = 0;
		maxX = (int)World.width - 1;
		minY = 0;
		maxY = (int)World.height - 1;
	}

	private Coord()
	{
		X = 0;
		Y = 0;
	}

	public Coord(int x, int y)
	{
		this.X = x;
		this.Y = y;
	}

	public int ManhattanDistance(Coord c)
	{
		int d = Mathf.Abs(c.X - X) + Mathf.Abs(c.Y - Y) - 1;
		//Debug.Log("ManhattanDistance entre " + ToString() + " et " + c.ToString() + " = " + d);
		return d;
	}

	public float FlyDistance(Coord c)
	{
		float d = Mathf.Sqrt((c.X - X) * (c.X - X) + (c.Y - Y) * (c.Y - Y));
		//Debug.Log("Distance entre " + ToString() + " et " + c.ToString() + " = " + d);
		return d;
	}

	public override bool Equals(object obj)
	{
		var n = obj as Coord;
		return X == n?.X && Y == n?.Y;
	}

	public override int GetHashCode()
	{
		return X.GetHashCode() ^ Y.GetHashCode();
	}

	public override string ToString()
	{
		return $"[{X},{Y}]";
	}

	public Coord Left()
	{
		if (X - 1 < minX) return null;
		else return new Coord(X - 1, Y);
	}

	public Coord Right()
	{
		if (X + 1 > maxX) return null;
		else return new Coord(X + 1, Y);
	}

	public Coord Up()
	{
		if (Y + 1 > maxY) return null;
		else return new Coord(X, Y + 1);
	}

	public Coord UpLeft()
	{
		if (Y + 1 > maxY || X - 1 < minX) return null;
		else return new Coord(X - 1, Y + 1);
	}

	public Coord UpRight()
	{
		if (Y + 1 > maxY || X + 1 > maxX) return null;
		else return new Coord(X + 1, Y + 1);
	}

	public Coord Down()
	{
		if (Y - 1 < minY) return null;
		else return new Coord(X, Y - 1);
	}

	public Coord DownLeft()
	{
		if (Y - 1 < minY || X - 1 < minX) return null;
		else return new Coord(X - 1, Y - 1);
	}

	public Coord DownRight()
	{
		if (Y - 1 < minY || X + 1 > maxX) return null;
		else return new Coord(X + 1, Y - 1);
	}

	public List<Coord> Directions()
	{
		return new List<Coord>(4)
		{
			Left(),
			Right(),
			Up(),
			Down()
		};
	}

	public List<Coord> ExtendedDirections()
	{
		var d = Directions();
		d.Add(UpLeft());
		d.Add(UpRight());
		d.Add(DownLeft());
		d.Add(DownRight());

		return d;
	}
}

