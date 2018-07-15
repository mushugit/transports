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
    private readonly IFluxSource source;
    [JsonProperty]
    private readonly IFluxTarget target;

    private readonly Flux flux;

    private readonly VehiculeRender vr;
    private readonly Component vrComponent;

    [JsonProperty]
    private Cell currentCell;
    [JsonProperty]
    private Cell targetCell;

    [JsonProperty]
    private float distance;
    [JsonProperty]
    private float position;

    [JsonProperty]
    private double ticks;


    [JsonConstructor]
    public RoadVehicule(float speed, IFluxSource source, IFluxTarget target, Cell currentCell, Cell targetCell,
        bool hasArrived, float position, float distance, double ticks)
    {
        this.Speed = speed;
        this.source = source;
        this.target = target;
        this.flux = null;

        this.currentCell = currentCell;
        this.targetCell = targetCell;
        UpdatePath();

        this.position = position;
        this.distance = distance;

        var prefab = World.Instance?.TruckPrefab;

        if (prefab != null)
        {
            vrComponent = VehiculeRender.Build(prefab, new Vector3(source._Cell.X, 0, source._Cell.Y), this);
            vr = vrComponent.GetComponent<VehiculeRender>();
            vr.InitColor(source.Color, target.Color);
            vr.Init(currentCell, targetCell, speed, position / distance);
        }

        
        HasArrived = hasArrived;
        this.ticks = ticks;
    }

    public RoadVehicule(float speed, Path<Cell> initialPath, IFluxSource source, IFluxTarget target, Flux flux)
    {
        this.Speed = speed;
        //Debug.Log($"Truck speed = {this.speed}");
        this.source = source;
        this.target = target;
        this.flux = flux;

        path = initialPath;

        var prefab = World.Instance?.TruckPrefab;

        if (prefab != null)
        {
            vrComponent = VehiculeRender.Build(prefab, new Vector3(source._Cell.X, 0, source._Cell.Y), this);
            vr = vrComponent.GetComponent<VehiculeRender>();
            vr.InitColor(source.Color, target.Color);
        }

        position = 0;
        ticks = 0;
        if (path != null)
            pathPosition = path.GetEnumerator();
        else
            pathPosition = null;

        HasArrived = false;
        MoveCell();
    }

    public void MoveCell()
    {
        if (path != null)
        {
            if (targetCell == null)
            {
                pathPosition.MoveNext();
                targetCell = pathPosition.Current;
            }

            if (pathPosition.MoveNext())
            {

                currentCell = targetCell;
                targetCell = pathPosition.Current;
                position = 0;

                distance = (float)currentCell.FlyDistance(targetCell);

                vr.Init(currentCell, targetCell, Speed);
                //Debug.Log($"Truck has to move from {currentCell} to {targetCell} (d={distance})");
            }
            else
            {
                if (flux != null)
                {
                    //Debug.Log($"Distribute t={ticks} s={Speed} pathD={path.TotalCost}");
                    if (flux.Distribute(ticks, (ticks + 1) * Speed, this))
                    {
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
        ticks++;
    }

    public void Move()
    {
        if (path != null)
        {
            position += Speed;
            //Debug.Log($"Truck tracer {position} for distance {distance} (s={speed})");
            if (position >= distance)
                MoveCell();
        }

    }

    public void UpdatePath()
    {
        var pf = new Pathfinder<Cell>(Speed, 0, new List<Type>() { typeof(Road), typeof(City), typeof(Industry) });
        if (targetCell == null)
            targetCell = source._Cell;
        pf.FindPath(target._Cell, targetCell);
        path = pf.Path;

        if (path != null)
        {
            //Debug.Log($"New path found from {targetCell} to {target._Cell}");
            pathPosition = path.GetEnumerator();
            targetCell = null;
        }
        else
        {
            //Debug.Log($"No path found from {targetCell} to {target._Cell}");
            pathPosition = null;
        }


    }
}

