using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
public class Flux
{
	[JsonProperty]
	public City Source { get; private set; }
	[JsonProperty]
	public City Target { get; private set; }

	public bool IsWaitingForInput { get; private set; } = false;
	public bool IsWaitingForDelivery { get; private set; } = false;

	private readonly float speed;
	[JsonProperty]
	public float Position { get; private set; }
	public readonly float Distance;

	[JsonProperty]
	public int TotalCargoMoved { get; private set; }

	public enum Direction
	{
		incoming,
		outgoing
	}

	public static List<Flux> AllFlux = new List<Flux>();

	[JsonConstructor]
	public Flux(City source, City target)
	{
		Source = source;
		Target = target;
		Distance = RoadDistance(Source.Point, Target.Point);
		speed = Simulation.TickFrequency * 2;
		Position = 0;
		TotalCargoMoved = 0;

		Source.ReferenceFlux(this, Flux.Direction.outgoing);
		Target.ReferenceFlux(this, Flux.Direction.incoming);

		AllFlux.Add(this);
	}

	private float RoadDistance(Coord a, Coord b)
	{
		var path = new List<Coord>();
		var searchParameters = new World.SearchParameter(a, b, 0, 0, false, path, true, false);
		World.Instance.StartCoroutine(World.Instance.SearchPath(searchParameters));
		return searchParameters.Path.Count - 2;
	}

	public Flux(Flux dummyFlux)
	{
		var trueSource = World.Instance.Constructions[dummyFlux.Source.Point.X, dummyFlux.Source.Point.Y] as City;
		var trueTarget = World.Instance.Constructions[dummyFlux.Target.Point.X, dummyFlux.Target.Point.Y] as City;
		Source = trueSource;
		Target = trueTarget;
		Distance = RoadDistance(Source.Point, Target.Point);
		speed = Simulation.TickFrequency * 2;
		Position = dummyFlux.Position;
		TotalCargoMoved = dummyFlux.TotalCargoMoved;

		Source.ReferenceFlux(this, Flux.Direction.outgoing);
		Target.ReferenceFlux(this, Flux.Direction.incoming);

		AllFlux.Add(this);
	}

	private bool Consume()
	{
		return Source.DistributeCargo(1);
	}

	private bool Distribute()
	{
		Position = Distance;
		var delivered = true;
		if (delivered)
		{
			var flyDistance = Source.FlyDistance(Target);
			var optimumGain = World.LocalEconomy.GetGain("flux_deliver_optimum_percell");
			var obtainedGain = World.LocalEconomy.GetGain("flux_deliver_percell");
			var gain = (int) Math.Round(optimumGain * flyDistance - obtainedGain * Distance);
			World.LocalEconomy.Credit(gain);
			TotalCargoMoved++;
			Position = 0;
		}
		return delivered;
	}

	public void Move()
	{
		int cost;
		World.LocalEconomy.ForcedCost("flux_running", out cost);
		IsWaitingForInput = false;
		IsWaitingForDelivery = false;

		if (Position == 0)
		{
			if (!Consume())
			{
				IsWaitingForInput = true;
				return;
			}
		}

		Position += speed;

		if (Position > Distance)
		{
			if (!Distribute())
			{
				IsWaitingForDelivery = true;
				return;
			}
		}
	}

	public static void RemoveFlux(Flux f)
	{
		AllFlux.Remove(f);
	}
}

