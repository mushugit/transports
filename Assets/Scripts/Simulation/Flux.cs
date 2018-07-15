using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
public class Flux
{
    public static float FrameDelayBetweenTrucks = 50;

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
    public int AvailableTrucks { get; private set; }
    [JsonProperty]
    public int TotalCargoMoved { get; private set; }
    [JsonProperty]
    public List<RoadVehicule> Trucks { get; private set; }
    [JsonProperty]
    private float currentDelay;

    public Path<Cell> Path { get; private set; }

    public enum Direction
    {
        incoming,
        outgoing
    }

    public static List<Flux> AllFlux = new List<Flux>();
 
    public Flux(IFluxSource source, IFluxTarget target, int truckQuantity)
    {
        Source = source;
        Target = target;
        speed = defaultSpeed;
        TotalCargoMoved = 0;
        AvailableTrucks = truckQuantity;
        Trucks = new List<RoadVehicule>(truckQuantity);
        currentDelay = FrameDelayBetweenTrucks;

        GetPath();

        if (Path != null)
        {
            Source.ReferenceFlux(this);
            Target.ReferenceFlux(this);
            AllFlux.Add(this);
        }
    }

    [JsonConstructor]
    public Flux(IFluxSource source, IFluxTarget target, int availableTrucks, int totalCargoMoved, List<RoadVehicule> trucks, float currentDelay)
    {
        Debug.Log("New flux from Json");

        Source = source;
        Target = target;
        speed = defaultSpeed;
        TotalCargoMoved = totalCargoMoved;
        AvailableTrucks = availableTrucks;
        this.Trucks = trucks;
        this.currentDelay = currentDelay;
    }

    public Flux(Flux dummy)
    {
        Debug.Log("New flux from dummy");
        var trueSource = World.Instance.Constructions[dummy.Source._Cell.X, dummy.Source._Cell.Y] as IFluxSource;
        var trueTarget = World.Instance.Constructions[dummy.Target._Cell.X, dummy.Target._Cell.Y] as IFluxTarget;
        Source = trueSource;
        Target = trueTarget;
        speed = defaultSpeed;
        TotalCargoMoved = dummy.TotalCargoMoved;
        AvailableTrucks = dummy.AvailableTrucks;
        Trucks = new List<RoadVehicule>(AvailableTrucks);
        foreach (RoadVehicule truck in dummy.Trucks)
            Trucks.Add(new RoadVehicule(truck));

        currentDelay = FrameDelayBetweenTrucks;

        GetPath();
        if (Path != null)
        {
            Source.ReferenceFlux(this);
            Target.ReferenceFlux(this);
            AllFlux.Add(this);
        }
    }

    public void AddTrucks(int quantity)
    {
        AvailableTrucks += quantity;
    }

    public void UpdateTruckPath()
    {
        foreach (RoadVehicule truck in Trucks)
            truck.UpdatePath();
    }

    private Path<Cell> GetPath()
    {
        var pf = new Pathfinder<Cell>(speed, 0, new List<Type>() { typeof(Road), typeof(City), typeof(Industry) });
        pf.FindPath(Target._Cell, Source._Cell);
        Path = pf.Path;
        return pf.Path;
    }

    private bool Consume()
    {
        if (Source.ProvideCargo(1))
        {
            currentDelay = 0;
            AvailableTrucks--;
            var truck = new RoadVehicule(speed, GetPath(), Source, Target, this);
            Trucks.Add(truck);
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool Distribute(double ticks, double actualDistance, RoadVehicule truck)
    {
        var delivered = true;
        if (delivered)
        {
            truck.HasArrived = true;
            AvailableTrucks++;
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
        currentDelay++;
        World.LocalEconomy.ForcedCost("flux_running", out cost);
        IsWaitingForInput = false;
        IsWaitingForDelivery = false;
        IsWaitingForPath = false;


        foreach (RoadVehicule truck in Trucks)
        {
            truck.Tick();
            truck.CheckArrived();
        }

        Trucks.RemoveAll(r => r.HasArrived);

        if (AvailableTrucks > 0 && currentDelay >= FrameDelayBetweenTrucks)
        {
            if (!Consume())
            {
                IsWaitingForInput = true;
            }
            else
            {
            }
        }

        foreach (RoadVehicule truck in Trucks)
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

