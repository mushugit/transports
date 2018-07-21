using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Pathfinder<Node> where Node : IHasNeighbours<Node>, IHasRelativeDistance, IHasCoord, IHasCell
{
    public Path<Node> Path { get; private set; }

    public readonly float Speed;
    public readonly float WaitAtTheEnd;

    public static readonly float WalkingSpeed = 2.42f;

    public readonly List<Type> Passable;

    private readonly double searchSpeed = 200d;

    public Pathfinder(List<Type> passable = null, float speed = 0, float waitAtTheEnd = 0)
    {
        Speed = speed;
        WaitAtTheEnd = waitAtTheEnd;
        Passable = passable;
    }

    public Path<Node> FindPath(Node start, Node destination, Func<Node, Node, double> distance, Func<Node, double> estimate)
    {
        var e = RoutineFindPath(start, destination, distance, estimate);
        while (e.MoveNext()) ;
        return Path;
    }

    public Path<Node> FindPath(Node start, Node destination)
    {
        var e = RoutineFindPath(start, destination, (m, n) => TrueDistance(m, n), n => n.FlyDistance(destination));
        while (e.MoveNext()) ;
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
            foreach (var n in foundPath.LastStep.Neighbours(Passable))
            {
                var d = distance(foundPath.LastStep, n);
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

        var multiplierCost = WalkingSpeed;

        /*if (constructionOrigin is Road || constructionOrigin is City)
            multiplierCost = 2f;*/
        if (constructionTarget is Road || constructionTarget is City)
            multiplierCost = 1f;

        //UnityEngine.Debug.Log($"TrueDistance between {origin} and {target} is {origin.ManhattanDistance(target)} / {dividerCost}");

        return ((float)origin.ManhattanDistance(target)) * multiplierCost;
        //return origin.ManhattanDistance(target);
    }
}
