using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[JsonObject(MemberSerialization.OptIn)]
public class Flux
{
    public const float FrameDelayBetweenTrucks = 50;

    [JsonProperty]
    public IFluxSource Source { get; private set; }

    [JsonProperty]
    public IFluxTarget Target { get; private set; }

    public bool IsWaitingForInput { get; set; } = false;
    public bool IsWaitingForDelivery { get; set; } = false;
    public bool IsWaitingForPath { get; set; } = false;

    public const float DefaultSpeed = 0.1f;

    private readonly float _speed;

    [JsonProperty]
    public Queue<RoadVehiculeCharacteristics> AvailableTrucks { get; private set; }
    [JsonProperty]
    public int TotalCargoMoved { get; private set; }
    [JsonProperty]
    public List<RoadVehicule> Trucks { get; private set; }

    [JsonProperty]
    public float CurrentDelay { get; private set; }

    public Path<Cell> Path { get; private set; }

    public enum Direction
    {
        incoming,
        outgoing
    }

    public static List<Flux> AllFlux = new List<Flux>();

    public Flux(IFluxSource source, IFluxTarget target, int truckQuantity, RoadVehiculeCharacteristics type)
    {
        Source = source;
        Target = target;
        _speed = DefaultSpeed;
        TotalCargoMoved = 0;
        AvailableTrucks = new Queue<RoadVehiculeCharacteristics>(truckQuantity);
        Trucks = new List<RoadVehicule>(truckQuantity);
        CurrentDelay = FrameDelayBetweenTrucks;

        AddTrucks(truckQuantity, type);
        GetPath();

        if (Path != null)
        {
            Source.ReferenceFlux(this);
            Target.ReferenceFlux(this);
            AllFlux.Add(this);
        }
    }

    [JsonConstructor]
    public Flux(IFluxSource source, IFluxTarget target, int totalCargoMoved, float currentDelay,
        Queue<RoadVehiculeCharacteristics> availableTrucks, List<RoadVehicule> trucks)
    {
        Source = source;
        Target = target;
        _speed = DefaultSpeed;
        TotalCargoMoved = totalCargoMoved;
        AvailableTrucks = availableTrucks;
        Trucks = trucks;
        CurrentDelay = currentDelay;
    }

    public Flux(Flux dummy)
    {
        var trueSource = World.Instance.Constructions[dummy.Source._Cell.X, dummy.Source._Cell.Y] as IFluxSource;
        var trueTarget = World.Instance.Constructions[dummy.Target._Cell.X, dummy.Target._Cell.Y] as IFluxTarget;
        Source = trueSource;
        Target = trueTarget;
        _speed = DefaultSpeed;
        TotalCargoMoved = dummy.TotalCargoMoved;

        AvailableTrucks = dummy.AvailableTrucks;
        Trucks = new List<RoadVehicule>(dummy.Trucks.Capacity);

        CurrentDelay = FrameDelayBetweenTrucks;

        foreach (RoadVehicule truck in dummy.Trucks)
            Trucks.Add(new RoadVehicule(truck, this));

        GetPath();
        if (Path != null)
        {
            //Debug.Log($"Reference {trueSource} ({dummy.Source}) => {trueTarget} ({dummy.Target})");
            Source.ReferenceFlux(this);
            Target.ReferenceFlux(this);
            AllFlux.Add(this);
        }
        else
        {
            IsWaitingForPath = true;
        }

        if (AvailableTrucks.Count > 0 && AvailableTrucks.Peek().Capacity > Source.PeekCargo())
        {
            IsWaitingForInput = true;
        }
    }
    public void UpdateTruckPath()
    {
        foreach (var truck in Trucks)
            truck.UpdatePath();
    }

    private Path<Cell> GetPath()
    {
        var pf = new Pathfinder<Cell>(_speed, 0, new List<Type>() { typeof(Road), typeof(City), typeof(Industry) });
        pf.FindPath(Target._Cell, Source._Cell);
        Path = pf.Path;
        return pf.Path;
    }

    private bool Consume(int quantity)
    {
        if (Source.ProvideCargo(quantity))
        {
            var characteristics = AvailableTrucks.Dequeue();
            var truck = new RoadVehicule(characteristics, GetPath(), Source, Target, this);
            Trucks.Add(truck);
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool Distribute(double ticks, double actualDistance, RoadVehicule truck, int quantity)
    {
        var delivered = true;
        if (delivered)
        {
            truck.HasArrived = true;
            AvailableTrucks.Enqueue(truck.Characteristics);

            var walkingDistance = Source.ManhattanDistance(Target) * Pathfinder<Cell>.WalkingSpeed;
            var obtainedGain = World.LocalEconomy.GetGain("flux_deliver_percell");
            var gain = ((int)Math.Round((walkingDistance - actualDistance) * obtainedGain)) * quantity;
            World.LocalEconomy.Credit(gain);
            TotalCargoMoved++;
        }
        return delivered;
    }

    public void AddTrucks(int quantity, RoadVehiculeCharacteristics type)
    {
        for (int i = 0; i < quantity; i++)
        {
            AvailableTrucks.Enqueue(type);
        }
    }

    public void Move()
    {
        int cost;
        CurrentDelay++;
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

        //Debug.Log($"Check consume : a={AvailableTrucks.Count} d={CurrentDelay} n={((AvailableTrucks.Count > 0) ? AvailableTrucks.Peek().Capacity : 0)} c={Source.PeekCargo()}");
        if (AvailableTrucks.Count > 0 && CurrentDelay >= FrameDelayBetweenTrucks)
        {
            if (!Consume(AvailableTrucks.Peek().Capacity))
            {
                IsWaitingForInput = true;
            }
            else
            {
                CurrentDelay = 0;
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

