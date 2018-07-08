using Newtonsoft.Json;
using UnityEngine;

public class Depot : Construction
{
	private Component depotRender;

	[JsonProperty]
	public int Direction { get; }

	private Depot() { }

	public Depot(Cell position, Component depotPrefab, int direction)
	{
		Point = position;
		if (depotPrefab != null)
		{
			depotRender = DepotRender.Build(new Vector3(Point.X, 0f, Point.Y), depotPrefab, direction);
			var objectRenderer = depotRender.GetComponentInChildren<DepotObjectRender>();
			objectRenderer.Depot(this);
		}
				
		Direction = direction;
	}

	public override void Destroy()
	{
		var r = depotRender?.GetComponentInChildren<DepotRender>();
		r?.Destroy();
	}
}

