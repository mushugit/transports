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
	public bool IsWaitingForPath { get; private set; } = false;

	private readonly float speed;
	[JsonProperty]
	public double Position { get; private set; }
	public double Distance { get; private set; }

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
		Distance = (float) RoadDistance(Source.Point, Target.Point);
		speed = Simulation.TickFrequency * 2;
		Position = 0;
		TotalCargoMoved = 0;

		Source.ReferenceFlux(this, Flux.Direction.outgoing);
		Target.ReferenceFlux(this, Flux.Direction.incoming);

		AllFlux.Add(this);
	}

	private double RoadDistance(Cell a, Cell b)
	{
		var path = new List<Cell>();
		
		var pf = new Pathfinder<Cell>(0, 0, new List<Type>(2) { typeof(Road), typeof(City) });
		pf.FindPath(a, b);
		if (pf.Path != null)
			return pf.Path.TotalCost;
		else
			return -1;
	}

	public Flux(Flux dummyFlux)
	{
		var trueSource = World.Instance.Constructions[dummyFlux.Source.Point.X, dummyFlux.Source.Point.Y] as City;
		var trueTarget = World.Instance.Constructions[dummyFlux.Target.Point.X, dummyFlux.Target.Point.Y] as City;
		Source = trueSource;
		Target = trueTarget;
		Distance = (float) RoadDistance(Source.Point, Target.Point);
		speed = Simulation.TickFrequency * 2;
		Position = dummyFlux.Position;
		TotalCargoMoved = dummyFlux.TotalCargoMoved;

		Source.ReferenceFlux(this, Flux.Direction.outgoing);
		Target.ReferenceFlux(this, Flux.Direction.incoming);

		AllFlux.Add(this);
	}

	public void ResetDistance(double distance)
	{
		Distance = distance;
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
			var gain = (int)Math.Round(optimumGain * flyDistance - obtainedGain * Distance);
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
		IsWaitingForPath = false;

		if (!Source.IsLinkedTo(Target))
		{
			IsWaitingForPath = true;
			return;
		}

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

