using System;
using System.Collections.Generic;
using UnityEngine;

public class RoadVehicule
{
    public float Speed;

    public bool HasArrived;

    private Path<Cell> path;
	private IEnumerator<Cell> pathPosition;
	
	private readonly IFluxSource source;
	private readonly IFluxTarget target;
	private readonly Flux flux;

	private readonly VehiculeRender vr;
	private readonly Component vrComponent;

	private Cell currentCell;
	private Cell targetCell;

	private float distance;
	private float position;

	private double ticks;


	public RoadVehicule(Component prefab, float speed, Path<Cell> initialPath, IFluxSource source, IFluxTarget target, Flux flux)
	{
		this.Speed = speed;
		//Debug.Log($"Truck speed = {this.speed}");
		this.source = source;
		this.target = target;
		this.flux = flux;

		path = initialPath;

		vrComponent = VehiculeRender.Build(prefab, new Vector3(source._Cell.X, 0, source._Cell.Y), this);
		vr = vrComponent.GetComponent<VehiculeRender>();
		vr.InitColor(source.Color, target.Color);

		position = 0;
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
				//Debug.Log($"Distribute t={ticks} s={Speed} pathD={path.TotalCost}");
				if (flux.Distribute(ticks, (ticks+1)*Speed, this))
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
            Debug.Log($"New path found from {targetCell} to {target._Cell}");
            pathPosition = path.GetEnumerator();
            targetCell = null;
        }
        else
        {
            Debug.Log($"No path found from {targetCell} to {target._Cell}");
            pathPosition = null;
        }

		
	}
}

