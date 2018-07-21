using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;


[JsonObject(MemberSerialization.OptIn)]
public class RoadVehicule
{
    public static readonly int frameskipToNextVehicule = 50;

    [JsonProperty]
    public float Speed;

    [JsonProperty]
    public bool HasArrived;

    private Path<Cell> path;
    private IEnumerator<Cell> pathPosition;

    [JsonProperty]
    public IFluxSource Source { get; private set; }
    [JsonProperty]
    public IFluxTarget Target { get; private set; }

    private readonly Flux flux;

    public VehiculeRender VehiculeObjetRenderer { get; private set; }
    private readonly Component vrComponent;

    [JsonProperty]
    public Cell CurrentCell { get; private set; }
    [JsonProperty]
    public Cell TargetCell { get; private set; }

    [JsonProperty]
    public float Distance { get; private set; }
    [JsonProperty]
    public float Position { get; private set; }

    [JsonProperty]
    public double Ticks { get; private set; }

    [JsonProperty]
    public RoadVehicule NextVehicule { get; private set; }
    public RoadVehicule ParentVehicule { get; private set; }

    [JsonProperty]
    public int NextVehiculeSteps { get; private set; } = 0;

    #region constructor
    [JsonConstructor]
    public RoadVehicule(float speed, IFluxSource source, IFluxTarget target, Cell currentCell, Cell targetCell,
        bool hasArrived, float position, float distance, double ticks, RoadVehicule nextVehicule, int nextVehiculeSteps)
    {
        this.Speed = speed;
        this.Source = source;
        this.Target = target;
        this.flux = null;

        this.CurrentCell = currentCell;
        this.TargetCell = targetCell;

        this.Position = position;
        this.Distance = distance;

        var prefab = World.Instance?.TruckPrefab;

        if (prefab != null)
        {
            vrComponent = VehiculeRender.Build(prefab, new Vector3(source._Cell.X, 0, source._Cell.Y), this);
            VehiculeObjetRenderer = vrComponent.GetComponent<VehiculeRender>();
            VehiculeObjetRenderer.InitColor(source.Color, target.Color);
            VehiculeObjetRenderer.Init(CurrentCell, TargetCell, Speed, Position / Distance);
        }

        HasArrived = hasArrived;
        this.Ticks = ticks;

        UpdatePath();
        NextVehicule = nextVehicule;
        NextVehiculeSteps = nextVehiculeSteps;
    }

    public RoadVehicule(RoadVehicule dummy, Flux flux)
        : this(dummy.Speed, dummy.Source, dummy.Target, dummy.CurrentCell, dummy.TargetCell,
             dummy.HasArrived, dummy.Position, dummy.Distance, dummy.Ticks, dummy.NextVehicule, dummy.NextVehiculeSteps)
    {
        //Debug.Log($"Loaded truck at {dummy.CurrentCell}");
        this.flux = flux;
    }

    public RoadVehicule(float speed, Path<Cell> initialPath, IFluxSource source, IFluxTarget target, Flux flux,
        int followingTrucks, RoadVehicule parent = null)
    {
        this.Speed = speed;
        //Debug.Log($"Truck speed = {this.speed}");
        this.Source = source;
        this.Target = target;
        this.flux = flux;

        path = initialPath;

        var prefab = World.Instance?.TruckPrefab;

        if (prefab != null)
        {
            vrComponent = VehiculeRender.Build(prefab, new Vector3(source._Cell.X, 0, source._Cell.Y), this);
            VehiculeObjetRenderer = vrComponent.GetComponent<VehiculeRender>();
            VehiculeObjetRenderer.InitColor(source.Color, target.Color);
        }

        Position = 0;
        Ticks = 0;
        if (path != null)
            pathPosition = path.GetEnumerator();
        else
            pathPosition = null;

        HasArrived = false;
        MoveCell();

        NextVehiculeSteps = 0;
        ParentVehicule = parent;

        if (followingTrucks > 0)
        {
            NextVehicule = new RoadVehicule(speed, initialPath, source, target, flux, followingTrucks - 1, this);
        }
    }
    #endregion

    private void MoveCell()
    {
        if (path != null)
        {
            if (TargetCell == null)
            {
                pathPosition.MoveNext();
                TargetCell = pathPosition.Current;
            }

            if (pathPosition.MoveNext())
            {
                CurrentCell = TargetCell;
                TargetCell = pathPosition.Current;
                Position = 0;

                Distance = (float)CurrentCell.FlyDistance(TargetCell);

                VehiculeObjetRenderer.Init(CurrentCell, TargetCell, Speed);
                //Debug.Log($"Truck has to move from {currentCell} to {targetCell} (d={distance})");
            }
            else
            {
                if (flux != null)
                {
                    if (flux.Distribute(Ticks, (Ticks + 1) * Speed, this))
                    {
                        //Debug.Log($"Distribute t={Ticks} s={Speed} pathD={path.TotalCost}");
                        HasArrived = true;
                        //GameObject.Destroy(vrComponent.gameObject);
                    }
                    else
                    {
                        flux.IsWaitingForDelivery = true;
                    }
                }
            }
        }
    }

    public void CheckArrived()
    {
        if (HasArrived)
        {
            NextVehicule?.CheckArrived();
            GameObject.Destroy(vrComponent.gameObject);
        }
    }

    public void Tick()
    {
        Ticks++;
        NextVehiculeSteps++;
        NextVehicule?.Tick();
    }

    public void NextLeaving()
    {
        NextVehiculeSteps = 0;
    }

    public void Move()
    {
        if (path != null)
        {
            Position += Speed;
            NextVehicule?.Move();
            //Debug.Log($"Truck tracer {position} for distance {distance} (s={speed})");
            if (Position >= Distance)
                MoveCell();
        }

    }

    public void UpdatePath()
    {
        var pf = new Pathfinder<Cell>(Speed, 0, new List<Type>() { typeof(Road), typeof(City), typeof(Industry) });
        if (TargetCell == null)
            TargetCell = Source._Cell;
        pf.FindPath(Target._Cell, TargetCell);
        path = pf.Path;

        if (path != null)
        {
            //Debug.Log($"New path found from {targetCell} to {target._Cell}");
            pathPosition = path.GetEnumerator();
            pathPosition.MoveNext();
            TargetCell = pathPosition.Current;
        }
        else
        {
            //Debug.Log($"No path found from {targetCell} to {target._Cell}");
            pathPosition = null;
        }
        NextVehicule?.UpdatePath();
    }

    public int ConvoySize()
    {
        if (NextVehicule != null)
            return 1 + NextVehicule.ConvoySize();
        else
            return 1;
    }
}

