using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class Builder
{
	private static Builder instance;

	public static bool IsBuilding { get; private set; }
	public static bool IsDestroying { get; private set; }

	private Type TypeOfBuild;

	static Builder()
	{
		instance = new Builder();
	}

	private void Building()
	{
		IsBuilding = true;
		IsDestroying = false;
	}

	private void Destroying()
	{
		IsDestroying = true;
		IsBuilding = false;
	}

	private void _City()
	{
		Building();
		TypeOfBuild = typeof(City);
	}



	private void _Road()
	{
		Building();
		TypeOfBuild = typeof(Road);
	}

	private void _Bulldoze()
	{
		Destroying();
	}

	public static void City()
	{
		instance._City();
	}

	public static void Road()
	{
		instance._Road();
	}

	public static void Bulldoze()
	{
		instance._Bulldoze();
	}
}

