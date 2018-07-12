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
		Coord = position;
		if (depotPrefab != null)
		{
			depotRender = DepotRender.Build(new Vector3(Coord.X, 0f, Coord.Y), depotPrefab, direction);
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

