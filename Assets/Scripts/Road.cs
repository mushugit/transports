using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road : Construction
{
    readonly Component roadRender;

    public Road(int x, int y, Component roadPrefab)
    {
        Point = new Point(x, y);
        roadRender = RoadRender.Build(new Vector3(x, 0f, y), roadPrefab);
    }

    public Road(Point point, Component roadPrefab)
    {
        Point = point;
        roadRender = RoadRender.Build(new Vector3(Point.X, 0f, Point.Y), roadPrefab);
    }

    public void UpdateConnexions(bool north, bool east, bool south, bool west)
    {
        //Debug.Log("N=" + north + " E=" + east + " S=" + south + " E=" + east + " W=" + west);
        roadRender.SendMessage(nameof(RoadRender.SetRoadNorth),	north);
        roadRender.SendMessage(nameof(RoadRender.SetRoadEast),	east);
        roadRender.SendMessage(nameof(RoadRender.SetRoadSouth), south);
        roadRender.SendMessage(nameof(RoadRender.SetRoadWest),	west);
    }
}
