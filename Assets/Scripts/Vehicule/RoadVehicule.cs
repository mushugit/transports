using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;


[JsonObject(MemberSerialization.OptIn)]
public class RoadVehicule
{
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

    private readonly VehiculeRender vr;
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

    #region constructor
    [JsonConstructor]
    public RoadVehicule(float speed, IFluxSource source, IFluxTarget target, Cell currentCell, Cell targetCell,
        bool hasArrived, float position, float distance, double ticks)
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
            vr = vrComponent.GetComponent<VehiculeRender>();
            vr.InitColor(source.Color, target.Color);
            vr.Init(CurrentCell, TargetCell, Speed, Position / Distance);
        }

        HasArrived = hasArrived;
        this.Ticks = ticks;

        UpdatePath();
    }

    public RoadVehicule(RoadVehicule dummy, Flux flux)
        : this(dummy.Speed, dummy.Source, dummy.Target, dummy.CurrentCell, dummy.TargetCell,
             dummy.HasArrived, dummy.Position, dummy.Distance, dummy.Ticks)
    {
        Debug.Log($"Loaded truck at {dummy.CurrentCell}");
        this.flux = flux;
    }

    public RoadVehicule(float speed, Path<Cell> initialPath, IFluxSource source, IFluxTarget target, Flux flux)
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
            vr = vrComponent.GetComponent<VehiculeRender>();
            vr.InitColor(source.Color, target.Color);
        }

        Position = 0;
        Ticks = 0;
        if (path != null)
            pathPosition = path.GetEnumerator();
        else
            pathPosition = null;

        HasArrived = false;
        MoveCell();
    }
    #endregion

    public void MoveCell()
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

                vr.Init(CurrentCell, TargetCell, Speed);
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
            GameObject.Destroy(vrComponent.gameObject);
        }
    }

    public void Tick()
    {
        Ticks++;
    }

    public void Move()
    {
        if (path != null)
        {
            Position += Speed;
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


    }
}

