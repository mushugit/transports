using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Node : IEquatable<Node>, IComparable<Node>
{
	public Coord Point { get; }
	public float Cost { get; private set; }
	public float Heuristic { get; private set; }

	public World World { get; }

	public Node(World world, Coord position, float cost, float heuristic)
	{
		Point = position;
		Cost = cost;
		Heuristic = heuristic;
		World = world;
	}

	public void Score(float s)
	{
		Cost = s;
	}

	public void Distance(float d)
	{
		Heuristic = d;
	}

	public int Compare(Node n)
	{
		if (Heuristic < n.Heuristic) return -1;
		if (Heuristic == n.Heuristic)
		{
			if (Cost < n.Cost) return -1;
			if (Cost == n.Cost) return 0;
			return 1;
		}
		return 1;
	}

	public IEnumerable<Node> Neighbors(bool avoidCities, bool onlyRoads, Coord target)
	{
		var neighbors = new List<Node>();
		var baseCost = 1f;
		var roadCoast = baseCost / 5f;
		var directions = Point.Directions();

		if (!onlyRoads)
		{
			//TODO parcourir une enum
			foreach (Coord p in directions)
			{
				if (p != null)
				{
					if (!avoidCities || !(World.Constructions[p.X, p.Y] is City))
					{
						float cost = baseCost; //TODO Dans une méthode 
						var c = World.Constructions[p.X, p.Y];
						if (c is Road || c is City) cost = roadCoast;
						neighbors.Add(new Node(World, p, float.MaxValue - cost, 0));
					}
				}
			}
		}
		else
		{
			var sb = new StringBuilder();
			foreach(Coord p in directions)
			{
				if (p != null)
				{
					var c = World.Constructions[p.X, p.Y];
					if (c is Road || c is City)
					{
						neighbors.Add(new Node(World, p, float.MaxValue, 0));
						sb.Append($"{p.ToString()} ");
					}
				}
			}
			Debug.Log($"Road neightbors of {Point} are {sb.ToString()}");
		}
		return neighbors;
	}

	public override bool Equals(object obj)
	{
		var n = obj as Node;
		return Equals(n);
	}

	public override int GetHashCode()
	{
		return Point.GetHashCode() ^ Cost.GetHashCode() ^ Heuristic.GetHashCode();
	}

	public int CompareTo(Node n)
	{
		if (n == null)
			return 1;

		if (this.Heuristic == n.Heuristic)
			return this.Cost.CompareTo(n.Cost);

		return this.Heuristic.CompareTo(n.Heuristic);
	}

	override public string ToString()
	{
		return $"Node{Point} H={Heuristic} c={Cost}";
	}

	public bool Equals(Node n)
	{
		return Point.Equals(n?.Point);
	}
}

