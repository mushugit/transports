using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;


public class Cell : IComparable<Cell>, IHasNeighbours<Cell>, IHasConstruction, IHasRelativeDistance<Cell>, IHasCoord
{
	public static int minX;
	public static int maxX;
	public static int minY;
	public static int maxY;

	[JsonProperty]
	public int X { get; }
	[JsonProperty]
	public int Y { get; }

	public Construction CellConstruction { get; }

	public Type Type { get; private set; }

	public IEnumerable<Cell> Neighbours(List<Type> passable)
	{
		var allNeighbours = Directions();
		var correcNeighbours = new List<Cell>(4);
		foreach (Cell c in allNeighbours)
		{
			if (c != null && (passable == null || passable.Contains(c.Type)))
				correcNeighbours.Add(c);
		}
		/*
		string sp = "";
		passable?.ForEach(c => sp += (c==null)?"": $" {c.ToString()}");
		string sAN = "";
		allNeighbours?.ForEach(c => sAN += (c == null) ? "" : $" {c.ToString()}");
		string sCN = "";
		correcNeighbours?.ForEach(c => sCN += (c == null) ? "" : $" {c.ToString()}");

		Debug.Log($"Voisin de {this} avec {sp}: {sAN}, corrects: {sCN}");*/
		return correcNeighbours;
	}

	static Cell()
	{
		ResetCellSystem();
	}

	public static void ResetCellSystem()
	{
		minX = 0;
		maxX = (int)World.width - 1;
		minY = 0;
		maxY = (int)World.height - 1;
	}

	private Cell()
	{
		X = 0;
		Y = 0;
		CellConstruction = null;
	}

	public Cell(int x, int y, Construction construction)
	{
		X = x;
		Y = y;
		CellConstruction = construction;
		Type = construction?.GetType();
	}

	public int ManhattanDistance(Cell c)
	{
		var diffX = Mathf.Abs(c.X - X);
		var diffY = Mathf.Abs(c.Y - Y);

		var d = diffX + diffY;
		//Debug.Log("ManhattanDistance entre " + ToString() + " et " + c.ToString() + " = " + d);
		return d;
	}

	public double FlyDistance(Cell c)
	{
		if(c==null) return 0;
		double d = Mathf.Sqrt((c.X - X) * (c.X - X) + (c.Y - Y) * (c.Y - Y));
		//Debug.Log("Distance entre " + ToString() + " et " + c.ToString() + " = " + d);
		return d;
	}

	public override bool Equals(object obj)
	{
		var n = obj as Cell;
		return X == n?.X && Y == n?.Y;
	}

	public override int GetHashCode()
	{
		return X.GetHashCode() ^ Y.GetHashCode();
	}

	public override string ToString()
	{
		return Type == null ? $"[{X},{Y}]" : $"[{X},{Y}] {{{Type}}}";
	}

	public Cell Left()
	{
		if (X - 1 < minX) return null;
		else
		{
			var c = World.Instance?.Constructions[X - 1, Y];
			//Debug.Log($"Point={ToString()} [{minX}-{maxX},{minY}-{maxY}] World[{World.width},{World.height}]");
			
			return new Cell(X - 1, Y, c);
		}
	}

	public Cell Right()
	{
		if (X + 1 > maxX) return null;
		else
		{
			var c = World.Instance?.Constructions[X + 1, Y];
			return new Cell(X + 1, Y, c);
		}
	}

	public Cell Up()
	{
		if (Y + 1 > maxY) return null;
		else
		{
			var c = World.Instance?.Constructions[X, Y + 1];
			return new Cell(X, Y + 1, c);
		}
	}

	public Cell UpLeft()
	{
		if (Y + 1 > maxY || X - 1 < minX) return null;
		else
		{
			var c = World.Instance?.Constructions[X - 1, Y + 1];
			return new Cell(X - 1, Y + 1, c);
		}
	}

	public Cell UpRight()
	{
		if (Y + 1 > maxY || X + 1 > maxX) return null;
		else
		{
			var c = World.Instance?.Constructions[X + 1, Y + 1];
			return new Cell(X + 1, Y + 1, c);
		}
	}

	public Cell Down()
	{
		if (Y - 1 < minY) return null;
		else
		{
			var c = World.Instance?.Constructions[X, Y - 1];
			return new Cell(X, Y - 1, c);
		}
	}

	public Cell DownLeft()
	{
		if (Y - 1 < minY || X - 1 < minX) return null;
		else
		{
			var c = World.Instance?.Constructions[X - 1, Y - 1];
			return new Cell(X - 1, Y - 1, c);
		}
	}

	public Cell DownRight()
	{
		if (Y - 1 < minY || X + 1 > maxX) return null;
		else
		{
			var c = World.Instance?.Constructions[X + 1, Y - 1];
			return new Cell(X + 1, Y - 1, c);
		}
	}

	public List<Cell> Directions()
	{
		return new List<Cell>(4)
		{
			Left(),
			Right(),
			Up(),
			Down()
		};
	}

	public List<Cell> ExtendedDirections()
	{
		var d = Directions();
		d.Add(UpLeft());
		d.Add(UpRight());
		d.Add(DownLeft());
		d.Add(DownRight());

		return d;
	}

	public int CompareTo(Cell other)
	{
		var t = Y * maxY + X;
		var tOther = other.Y * maxY + other.X;
		return t.CompareTo(tOther);
	}
}

