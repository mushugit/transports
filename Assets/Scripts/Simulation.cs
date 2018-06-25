using System;
using System.Collections;
using UnityEngine;

public class Simulation
{
	public static readonly float TickFrequency = 0.02f;

	public static IEnumerator Run()
	{
		while (true)
		{
			foreach (City c in World.Instance.Cities)
			{
				c.GenerateCargo();
			}
			yield return new WaitForSeconds(TickFrequency);
		}
	}
}

