using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road : Construction
{
    readonly Component roadRender;

    Point Point { get; }

    public Road(int x, int y, Component[] roadPrefabs)
    {
        Point = new Point(x, y);

        roadRender = RoadRender.Build(new Vector3(x + 0.5f, 0f, y + 0.5f), roadPrefabs);
    }

    public Road(Point point,Component[] roadPrefabs)
    {
        Point = point;
        roadRender = RoadRender.Build(new Vector3(Point.X + 0.5f, 0f, Point.Y + 0.5f), roadPrefabs);
    }
}
