using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulation
{
	public static readonly float TickFrequency = 0.02f;

	private static List<Flux> flux;

	static Simulation()
	{
		flux = new List<Flux>();
	}

	public static IEnumerator Run()
	{
		while (true)
		{
			foreach (City c in World.Instance.Cities)
			{
				c.GenerateCargo();
			}
			foreach(Flux f in flux)
			{
				f.Move();
			}
			yield return new WaitForSeconds(TickFrequency);
		}
	}

	public static void AddFlux(City source, City target)
	{
		var f = new Flux(source, target);
		flux.Add(f);
	}
}

