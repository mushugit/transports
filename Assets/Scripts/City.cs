using UnityEngine;
using System.Collections.Generic;

public class City : Construction
{
    Component cityRender;
    public string Name { get; }
    public float CargoLevel { get; }
    public List<City> LinkedCities { get; }

    private static List<string> cityNames = null;

    public City(Point position, Component cityPrefab)
    {
        Point = position;
        cityRender = CityRender.Build(new Vector3(Point.X, 0f, Point.Y), cityPrefab);
        Name = RandomName();
        cityRender.SendMessage("Label", Name);

        CargoLevel = 0.3f;
        LinkedCities = new List<City>();
    }
    static void InitNames()
    {
        if (cityNames == null)
        {
            TextAsset allCityNames = Resources.Load("Text/CityNames/françaises") as TextAsset;
            cityNames = new List<string>(allCityNames.text.Split('\n'));
        }
    }

    public int ManhattanDistance(City city)
    {
        return Point.ManhattanDistance(city.Point);
    }

    public int ManhattanDistance(Point point)
    {
        return Point.ManhattanDistance(point);
    }

    public void AddLinkTo(City c)
    {
        if(!LinkedCities.Contains(c))
            LinkedCities.Add(c);
    }

    public bool IsLinkedTo(City c)
    {
        return LinkedCities.Contains(c);
    }

    public static string RandomName()
    {
        InitNames();
        int r = Random.Range(0, cityNames.Count - 1);
        string name = cityNames[r];
        cityNames.RemoveAt(r);
        return name;
    }

    public static int Quantity(int w, int h)
    {
        var averageSquareSize = Mathf.Sqrt((float)(w * h));
        return Mathf.RoundToInt(Mathf.Sqrt(averageSquareSize*2)) + 1;
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


