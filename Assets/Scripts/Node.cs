using System;
using System.Collections;
using System.Collections.Generic;

public class Node : IComparable<Node>
{
	public Point Point { get; }
	public float Cost { get; private set; }
	public int Heuristic { get; private set; }

	public World World { get; }

	public Node(World world, Point position, float cost, int heuristic)
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

	public void Distance(int d)
	{
		Heuristic = d;
	}

	public int Compare(Node n)
	{
		if (Heuristic < n.Heuristic) return 1;
		if (Heuristic == n.Heuristic) return 0;
		return -1;
	}

	public IEnumerable<Node> Neighbors(bool avoidCities, bool onlyRoads)
	{
		var neighbors = new List<Node>();

		if (!onlyRoads)
		{
			//TODO parcourir une enum
			//left
			if (Point.X > 0)
				if (!avoidCities || !(World.Constructions[Point.X - 1, Point.Y] is City))
				{
					float cost = float.MaxValue; //TODO Dans une méthode générique
					if (World.Constructions[Point.X - 1, Point.Y] is Road) cost = cost / 2;
					neighbors.Add(new Node(World, new Point(Point.X - 1, Point.Y), cost, 0));
				}
			//right
			if (Point.X < World.width - 1)
				if (!avoidCities || !(World.Constructions[Point.X + 1, Point.Y] is City))
				{
					float cost = float.MaxValue;
					if (World.Constructions[Point.X + 1, Point.Y] is Road) cost = cost / 2;
					neighbors.Add(new Node(World, new Point(Point.X + 1, Point.Y), cost, 0));
				}
			//up
			if (Point.Y < World.height - 1)
				if (!avoidCities || !(World.Constructions[Point.X, Point.Y + 1] is City))
				{
					float cost = float.MaxValue;
					if (World.Constructions[Point.X, Point.Y + 1] is Road) cost = cost / 2;
					neighbors.Add(new Node(World, new Point(Point.X, Point.Y + 1), cost, 0));
				}
			//down
			if (Point.Y > 0)
				if (!avoidCities || !(World.Constructions[Point.X, Point.Y - 1] is City))
				{
					float cost = float.MaxValue;
					if (World.Constructions[Point.X, Point.Y - 1] is Road) cost = cost / 2;
					neighbors.Add(new Node(World, new Point(Point.X, Point.Y - 1), cost, 0));
				}
		}
		else
		{
			//left
			if (Point.X > 0)
				if (World.Constructions[Point.X - 1, Point.Y] != null && (World.Constructions[Point.X - 1, Point.Y] is Road || World.Constructions[Point.X - 1, Point.Y] is City))
					neighbors.Add(new Node(World, new Point(Point.X - 1, Point.Y), float.MaxValue, 0));
			//right
			if (Point.X < World.width - 1)
				if (World.Constructions[Point.X + 1, Point.Y] != null && (World.Constructions[Point.X + 1, Point.Y] is Road || World.Constructions[Point.X + 1, Point.Y] is City))
					neighbors.Add(new Node(World, new Point(Point.X + 1, Point.Y), float.MaxValue, 0));
			//up
			if (Point.Y < World.height - 1)
				if (World.Constructions[Point.X, Point.Y + 1] != null && (World.Constructions[Point.X, Point.Y + 1] is Road || World.Constructions[Point.X, Point.Y + 1] is City))
					neighbors.Add(new Node(World, new Point(Point.X, Point.Y + 1), float.MaxValue, 0));
			//down
			if (Point.Y > 0)
				if (World.Constructions[Point.X, Point.Y - 1] != null && (World.Constructions[Point.X, Point.Y - 1] is Road || World.Constructions[Point.X, Point.Y - 1] is City))
					neighbors.Add(new Node(World, new Point(Point.X, Point.Y - 1), float.MaxValue, 0));
		}


		return neighbors;
	}

	public override bool Equals(object obj)
	{
		var n = obj as Node;
		return Point.X == n?.Point?.X && Point.Y == n?.Point?.Y;
	}

	public override int GetHashCode()
	{
		return Point.GetHashCode() ^ Cost.GetHashCode() ^ Heuristic.GetHashCode();
	}

	public int CompareTo(Node n)
	{
		if (n == null)
			return 1;

		return this.Heuristic.CompareTo(n.Heuristic);
	}

	/*
	public static bool operator ==(Node a, Node b) {
		if(a.p==null||b.p==null)
			return false;
		
		return a.p.x==b.p.x && a.p.y==b.p.y;
	}
	
	public static bool operator !=(Node a, Node b) {
		return !(a == b);
	}*/
}

