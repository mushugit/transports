using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class Builder
{
	private static Builder instance;

	public static bool IsBuilding { get; private set; }
	public static bool CanRotateBuilding { get; private set; }
	public static int RotationDirection { get; set; } = 2;
	public static bool IsDestroying { get; private set; }
	public static Type TypeOfBuild { get; private set; }

	static Builder()
	{
		instance = new Builder();
	}

	public static void CancelAction()
	{
		IsBuilding = false;
		IsDestroying = false;
	}

	private void Building(bool canRotate = false)
	{
		IsBuilding = true;
		IsDestroying = false;
		CanRotateBuilding = canRotate;
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

	private void _Depot()
	{
		Building();
		CanRotateBuilding = true;
		TypeOfBuild = typeof(Depot);
	}

	private void _Bulldoze()
	{
		Destroying();
		CanRotateBuilding = false;
		TypeOfBuild = null;
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

	public static void Depot()
	{
		instance._Depot();
	}
}

