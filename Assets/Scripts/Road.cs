using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road : Construction
{
    readonly RoadRender roadRender;

	private Road() { }

    public Road(int x, int y, Component roadPrefab)
    {
        Point = new Coord(x, y);
        var r = RoadRender.Build(new Vector3(x, 0f, y), roadPrefab);
		roadRender = r.GetComponent<RoadRender>();
    }

	[JsonConstructor]
	public Road(Coord point, Component roadPrefab)
    {
        Point = point;
		if (roadPrefab != null)
		{
			var r = RoadRender.Build(new Vector3(Point.X, 0f, Point.Y), roadPrefab);
			roadRender = r.GetComponent<RoadRender>();
		}
    }

    public void UpdateConnexions(bool north, bool east, bool south, bool west)
    {
        //Debug.Log("N=" + north + " E=" + east + " S=" + south + " E=" + east + " W=" + west);
        roadRender.SetRoadNorth(north);
		roadRender.SetRoadEast(east);
		roadRender.SetRoadSouth(south);
		roadRender.SetRoadWest(west);

		roadRender.UpdateRender();
    }

	public override void Destroy()
	{
		var r = roadRender.GetComponent<RoadRender>();
		r.Destroy();
	}
}
