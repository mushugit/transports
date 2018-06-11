using System;
using UnityEngine;


public class Point
{
	public int X { get; }
	public int Y { get; }

	public Point(int x, int y)
	{
		this.X = x;
		this.Y = y;
	}

	public int ManhattanDistance(Point c)
	{
		int d = Mathf.Abs(c.X - X) + Mathf.Abs(c.Y - Y) - 1;
		//Debug.Log("ManhattanDistance entre " + ToString() + " et " + c.ToString() + " = " + d);
		return d;
	}

	public float Distance(Point c)
	{
		return Mathf.Sqrt((c.X - X) * (c.X - X) + (c.Y - Y) * (c.Y - Y));
	}

	public override bool Equals(object obj)
	{
		var n = obj as Point;
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
}

