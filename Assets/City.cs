using UnityEngine;
using System.Collections.Generic;

public class City : Construction
{
    Component cityRender;
    public Point Point { get; }
    public string Name { get; }
    public float CargoLevel { get; }

    private static List<string> cityNames = null;

    static void InitNames()
    {
        if (cityNames == null)
        {
            TextAsset allCityNames = Resources.Load("Text/CityNames/françaises") as TextAsset;
            Debug.Log("allCityNames" + allCityNames);
            Debug.Log(allCityNames.text.Substring(0, 100));
            Debug.Log(allCityNames.text.Split('\n'));
            cityNames = new List<string>(allCityNames.text.Split('\n'));
        }
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
        return Mathf.RoundToInt(Mathf.Sqrt(Mathf.Sqrt((float)(w * h)))) + 1;
    }

    public City(Point position, Component cityPrefab)
    {
        Point = position;
        cityRender = CityRender.Build(new Vector3(Point.X + 0.5f, 0.5f, Point.Y + 0.5f), cityPrefab);
        Name = RandomName();
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


