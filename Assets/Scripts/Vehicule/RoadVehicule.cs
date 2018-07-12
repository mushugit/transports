using System;
using System.Collections.Generic;
using UnityEngine;

public class RoadVehicule<Ts,Tt>
    where Ts : ICargoGenerator, IHasCoord, IHasColor
    where Tt : IHasCoord, IHasColor
{
	private Path<Cell> path;
	private IEnumerator<Cell> pathPosition;
	public float Speed;
	private readonly Ts source;
	private readonly Tt target;
	private readonly Flux flux;

	private readonly VehiculeRender vr;
	private readonly Component vrComponent;

	private Cell currentCell;
	private Cell targetCell;

	private float distance;
	private float position;

	private double ticks;


	public RoadVehicule(Component prefab, float speed, Path<Cell> initialPath, Ts source, Tt target, Flux flux)
	{
		this.Speed = speed;
		//Debug.Log($"Truck speed = {this.speed}");
		this.source = source;
		this.target = target;
		this.flux = flux;

		path = initialPath;

		vrComponent = VehiculeRender.Build(prefab, new Vector3(source.X, 0, source.Y), this);
		vr = vrComponent.GetComponent<VehiculeRender>();

		var sourceCityRender = source.CityRenderComponent.GetComponentInChildren<CityObjectRender>().GetComponentInChildren<Renderer>();
		var sourceCityColor = sourceCityRender.material.color;
		var targetCityRender = target.CityRenderComponent.GetComponentInChildren<CityObjectRender>().GetComponentInChildren<Renderer>();
		var targetCityColor = targetCityRender.material.color;
		vr.InitColor(sourceCityColor, targetCityColor);

		position = 0;
		pathPosition = path.GetEnumerator();

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
				if (flux.Distribute(ticks, (ticks+1)*Speed))
				{
					GameObject.Destroy(vrComponent.gameObject);
				}
			}
		}
	}

	public void Tick()
	{
		ticks++;
	}

	public void Move()
	{
		position += Speed;
		//Debug.Log($"Truck tracer {position} for distance {distance} (s={speed})");
		if (position >= distance)
			MoveCell();

	}

	public void UpdatePath()
	{
		var pf = new Pathfinder<Cell>(Speed, 0, new List<Type>() { typeof(Road), typeof(City) });
		pf.FindPath(target.Point, targetCell);
		path = pf.Path;

		if (path != null)
		{
			//Debug.Log($"New path found from {targetCell} to {target.Point}");
			pathPosition = path.GetEnumerator();
		}
		else
			pathPosition = null;

		targetCell = null;
	}
}

