using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Pathfinder<Node> where Node : IHasNeighbours<Node>, IHasConstruction, IHasRelativeDistance<Node>, IHasCoord
{
	public Path<Node> Path { get; private set; }

	public readonly float Speed;
	public readonly float WaitAtTheEnd;

	public readonly List<Type> Passable;

	private readonly double searchSpeed = 200d;

	public Pathfinder(float speed, float waitAtTheEnd, List<Type> passable)
	{
		Speed = speed;
		WaitAtTheEnd = waitAtTheEnd;
		Passable = passable;
	}

	public Path<Node> FindPath(Node start, Node destination)
	{
		return FindPath(start, destination, (m, n) => Pathfinder<Node>.TrueDistance(m, n), n => n.FlyDistance(destination));
	}

	public Path<Node> FindPath(
		Node start, Node destination,
		Func<Node, Node, double> distance, Func<Node, double> estimate)
	{
		//Debug.Log($"==Start pathfinding== {start} to {destination}");
		var closed = new HashSet<Node>();
		var queue = new PriorityQueue<double, Path<Node>>();
		queue.Enqueue(0, new Path<Node>(start));
		while (!queue.IsEmpty)
		{
			var path = queue.Dequeue();
			if (closed.Contains(path.LastStep))
				continue;
			if (path.LastStep.Equals(destination))
			{
				//Debug.Log($"\tFound of {path.TotalCost} ({start} to {destination})");
				Path = path;
				return Path;
			}
			closed.Add(path.LastStep);
			foreach (Node n in path.LastStep.Neighbours(Passable))
			{
				double d = distance(path.LastStep, n);
				var newPath = path.AddStep(n, d);
				queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
			}
		}
		//Debug.Log("\tNOT found ({start} to {destination})");
		Path = null;
		return Path;
	}

	public IEnumerator RoutineFindPath(Node start, Node destination)
	{
		yield return World.Instance.StartCoroutine(RoutineFindPath(start, destination, (m, n) => TrueDistance(m, n), n => n.FlyDistance(destination)));
	}


	public IEnumerator RoutineFindPath(
		Node start, Node destination,
		Func<Node, Node, double> distance, Func<Node, double> estimate)
	{
		//Debug.Log($"==Start pathfinding== {start} to {destination}");
		var countLoop = 0d;
		var closed = new HashSet<Node>();
		var queue = new PriorityQueue<double, Path<Node>>();
		queue.Enqueue(0, new Path<Node>(start));
		while (!queue.IsEmpty)
		{
			countLoop++;
			var foundPath = queue.Dequeue();
			if (closed.Contains(foundPath.LastStep))
				continue;
			if (foundPath.LastStep.Equals(destination))
			{
				if (WaitAtTheEnd > 0f)
					yield return new WaitForSeconds(WaitAtTheEnd);
				Path = foundPath;
				//Debug.Log($"\tFound of {foundPath.TotalCost} ({start} to {destination})");
				yield break;
			}
			closed.Add(foundPath.LastStep);
			foreach (Node n in foundPath.LastStep.Neighbours(Passable))
			{
				double d = distance(foundPath.LastStep, n);
				var newPath = foundPath.AddStep(n, d);
				queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
			}
			if (Speed > 0f)
				yield return new WaitForSeconds(Speed);
			else
			{
				if (countLoop % searchSpeed == 0)
					yield return null;
			}
		}
		Path = null;
		//Debug.Log($"\tNOT found ({start} to {destination})");
		yield break;
	}

	public static float TrueDistance(Node origin, Node target)
	{
		var constructionOrigin = World.Instance.Constructions[origin.X, origin.Y];
		var constructionTarget = World.Instance.Constructions[target.X, target.Y];

		var multiplierCost = 2.42f;

		/*if (constructionOrigin is Road || constructionOrigin is City)
			multiplierCost = 2f;*/
		if (constructionTarget is Road || constructionTarget is City)
			multiplierCost = 1f;

		//UnityEngine.Debug.Log($"TrueDistance between {origin} and {target} is {origin.ManhattanDistance(target)} / {dividerCost}");

		return ((float)origin.ManhattanDistance(target)) * multiplierCost;
		//return origin.ManhattanDistance(target);
	}
}
