using UnityEngine;

public class Flux
{
	public City Source { get; private set; }
	public City Target { get; private set; }

	public bool IsWaitingForInput { get; private set; } = false;
	public bool IsWaitingForDelivery { get; private set; } = false;

	private readonly float speed;
	private float position;
	private readonly float distance;

	public int TotalMoved { get; private set; }

	public enum Direction
	{
		incoming,
		outgoing
	}

	public Flux(City source, City target)
	{
		Source = source;
		Target = target;
		distance = Source.ManhattanDistance(Target);
		speed = Simulation.TickFrequency * 2;
		position = 0;
		TotalMoved = 0;

		Source.ReferenceFlux(this, Flux.Direction.outgoing);
		Target.ReferenceFlux(this, Flux.Direction.incoming);
	}

	private bool Consume()
	{
		Debug.Log($"[Flux] {Source}=>{Target}, CONSUME");
		return Source.DistributeCargo(1);
	}

	private bool Distribute()
	{
		position = distance;
		var delivered = true;
		if (delivered)
		{
			Debug.Log($"[Flux] {Source}=>{Target}, DISTRIBUTE");
			TotalMoved++;
			position = 0;
		}
		return delivered;
	}

	public void Move()
	{
		IsWaitingForInput = false;
		IsWaitingForDelivery = false;

		if (position == 0)
		{
			if (!Consume())
			{
				IsWaitingForInput = true;
				return;
			}
		}

		position += speed;

		if (position > distance)
		{
			if (!Distribute())
			{
				IsWaitingForDelivery = true;
				return;
			}
		}
	}
}

