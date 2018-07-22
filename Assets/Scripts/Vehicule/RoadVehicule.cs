using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;


[JsonObject(MemberSerialization.OptIn)]
public class RoadVehicule
{
    public const int frameskipToNextVehicule = 50;

    [JsonProperty]
    public bool HasArrived;

    private Path<Cell> path;
    private IEnumerator<Cell> pathPosition;

    [JsonProperty]
    public IFluxSource Source { get; private set; }
    [JsonProperty]
    public IFluxTarget Target { get; private set; }

    private readonly Flux _flux;

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
    public RoadVehiculeCharacteristics Characteristics { get; private set; }

    #region constructor
    [JsonConstructor]
    public RoadVehicule(RoadVehiculeCharacteristics characteristics, IFluxSource source, IFluxTarget target, Cell currentCell, Cell targetCell,
        bool hasArrived, float position, float distance, double ticks)
    {
        Characteristics = characteristics;
        if (Characteristics == null)
            Characteristics = new RoadVehiculeCharacteristics(1, Flux.DefaultSpeed);

        Source = source;
        Target = target;
        _flux = null;

        CurrentCell = currentCell;
        TargetCell = targetCell;

        Position = position;
        Distance = distance;


        var prefab = World.Instance?.TruckPrefab;

        if (prefab != null)
        {
            vrComponent = VehiculeRender.Build(prefab, new Vector3(source._Cell.X, 0, source._Cell.Y), this);
            VehiculeObjetRenderer = vrComponent.GetComponent<VehiculeRender>();
            VehiculeObjetRenderer.InitColor(source.Color, target.Color);
            VehiculeObjetRenderer.Init(CurrentCell, TargetCell, Characteristics, Position / Distance);
        }

        HasArrived = hasArrived;
        Ticks = ticks;

        UpdatePath();
    }

    public RoadVehicule(RoadVehicule dummy, Flux flux)
        : this(dummy.Characteristics, dummy.Source, dummy.Target, dummy.CurrentCell, dummy.TargetCell,
             dummy.HasArrived, dummy.Position, dummy.Distance, dummy.Ticks)
    {
        //Debug.Log($"Loaded truck at {dummy.CurrentCell}");
        _flux = flux;
    }

    public RoadVehicule(float speed, int capacity,
        Path<Cell> initialPath, IFluxSource source, IFluxTarget target, Flux flux)
        : this(new RoadVehiculeCharacteristics(capacity, speed), initialPath, source, target, flux)
    {
    }

    public RoadVehicule(RoadVehiculeCharacteristics characteristics, Path<Cell> initialPath, IFluxSource source, IFluxTarget target, Flux flux)
    {
        Characteristics = characteristics;
        //Debug.Log($"Truck speed = {Characteristics.speed}");
        Source = source;
        Target = target;
        _flux = flux;

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

                VehiculeObjetRenderer.Init(CurrentCell, TargetCell, Characteristics);
                //Debug.Log($"Truck has to move from {currentCell} to {targetCell} (d={distance})");
            }
            else
            {
                if (_flux != null)
                {
                    if (_flux.Distribute(Ticks, (Ticks + 1) * Characteristics.Speed, this, Characteristics.Capacity))
                    {
                        //Debug.Log($"Distribute t={Ticks} s={Speed} pathD={path.TotalCost}");
                        HasArrived = true;
                        //GameObject.Destroy(vrComponent.gameObject);
                    }
                    else
                    {
                        _flux.IsWaitingForDelivery = true;
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
            Position += Characteristics.Speed;
            //Debug.Log($"Truck tracer {position} for distance {distance} (s={speed})");
            if (Position >= Distance)
                MoveCell();
        }

    }

    public void UpdatePath()
    {
        var pf = new Pathfinder<Cell>(new List<Type>() { typeof(Road), typeof(City), typeof(Industry) }, Characteristics.Speed);
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

