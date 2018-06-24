using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Depot : Construction
{
	private Component depotRender;
	public int Direction { get; }

	public Depot(Coord position, Component cityPrefab, int direction)
	{
		Point = position;
		depotRender = DepotRender.Build(new Vector3(Point.X, 0f, Point.Y), cityPrefab, direction);
		var objectRenderer = depotRender.GetComponentInChildren<DepotObjectRender>();

		objectRenderer.Depot(this);
		Direction = direction;
	}
}

