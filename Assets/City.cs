using UnityEngine;
using System.Collections.Generic;

public class City : Construction
{
    Component cityRender;
    public Point Point { get; }
    public string Name { get; }
    public float CargoLevel { get; }

    public static int Quantity(int w, int h)
    {
        return Mathf.RoundToInt(Mathf.Sqrt(Mathf.Sqrt((float)(w * h)))) + 1;
    }

    public City(Point position, Component cityPrefab)
    {
        Point = position;
        cityRender = CityRender.Build(new Vector3(Point.X + 0.5f, 0.5f, Point.Y + 0.5f), cityPrefab);
        Name = "Ville [" + Point.X + "," + Point.Y + "]";
        cityRender.SendMessage("Label", Name);

        CargoLevel = 0.3f;
    }

    public bool IsLinked(City c)
    {
        return true;
    }

    public int GenerateCargo()
    {
        int quantity = 0;
        if (Random.value > CargoLevel)
        {
            //Debug.Log ("Cargo généré pour " + name);
            quantity++;
        }
        return quantity;
    }
}


