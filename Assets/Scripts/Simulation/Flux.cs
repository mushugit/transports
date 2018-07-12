using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
public class Flux
{
    [JsonProperty]
    public IFluxSource Source { get; private set; }

    [JsonProperty]
    public IFluxTarget Target { get; private set; }

    public bool IsWaitingForInput { get; set; } = false;
    public bool IsWaitingForDelivery { get; set; } = false;
    public bool IsWaitingForPath { get; set; } = false;

    private static readonly float defaultSpeed = 0.1f;

    private readonly float speed;

    [JsonProperty]
    public bool InTransit { get; private set; }
    [JsonProperty]
    public int TotalCargoMoved { get; private set; }

    private RoadVehicule truck;
    public Path<Cell> Path { get; private set; }

    public enum Direction
    {
        incoming,
        outgoing
    }

    public static List<Flux> AllFlux = new List<Flux>();

    [JsonConstructor]
    public Flux(IFluxSource source, IFluxTarget target)
    {
        Source = source;
        Target = target;
        speed = defaultSpeed;
        TotalCargoMoved = 0;

        GetPath();

        if (Path != null)
        {
            Source.ReferenceFlux(this);
            Target.ReferenceFlux(this);
            AllFlux.Add(this);
        }
    }

    public Flux(Flux dummyFlux)
    {
        var trueSource = World.Instance.Constructions[dummyFlux.Source.Coord.X, dummyFlux.Source.Coord.Y] as IFluxSource;
        var trueTarget = World.Instance.Constructions[dummyFlux.Target.Coord.X, dummyFlux.Target.Coord.Y] as IFluxTarget;
        Source = trueSource;
        Target = trueTarget;
        speed = defaultSpeed;
        TotalCargoMoved = dummyFlux.TotalCargoMoved;

        GetPath();
        if (Path != null)
        {
            Source.ReferenceFlux(this);
            Target.ReferenceFlux(this);
            AllFlux.Add(this);
        }
    }

    public void UpdateTruckPath()
    {
        truck?.UpdatePath();
    }

    private Path<Cell> GetPath()
    {
        var pf = new Pathfinder<Cell>(speed, 0, new List<Type>() { typeof(Road), typeof(City) });
        pf.FindPath(Target.Coord, Source.Coord);
        Path = pf.Path;
        return pf.Path;
    }

    private bool Consume()
    {
        if (Source.ProvideCargo(1))
        {
            truck = new RoadVehicule(World.Instance.truckPrefab, speed, GetPath(), Source, Target, this);
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool Distribute(double ticks, double actualDistance)
    {
        var delivered = true;
        if (delivered)
        {
            InTransit = false;
            var walkingDistance = Source.ManhattanDistance(Target) * Pathfinder<Cell>.WalkingSpeed;
            var obtainedGain = World.LocalEconomy.GetGain("flux_deliver_percell");
            var gain = (int)Math.Round((walkingDistance - actualDistance) * obtainedGain);
            World.LocalEconomy.Credit(gain);
            TotalCargoMoved++;
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

        truck?.Tick();

        if (!InTransit)
        {
            if (!Consume())
            {
                IsWaitingForInput = true;
                return;
            }
            else
                InTransit = true;
        }

        truck.Move();
    }

    public static void RemoveFlux(Flux f)
    {
        AllFlux.Remove(f);
    }

    public override string ToString()
    {
        return $"[{Source} => {Target}]";
    }
}

