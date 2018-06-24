using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class City : Construction
{
	Component cityRender; //TODO : déplacer dans Construction
	public string Name { get; } //TODO : variable globale commence par maj
	public float CargoLevel { get; }
	public List<City> LinkedCities { get; }
	public Window infoWindow = null;

	private static List<string> cityNames = null;

	public City(Coord position, Component cityPrefab)
	{
		Point = position;
		cityRender = CityRender.Build(new Vector3(Point.X, 0f, Point.Y), cityPrefab);
		var objectRenderer = cityRender.GetComponentInChildren<CityObjectRender>();
		Name = RandomName();
		objectRenderer.City(this);
		cityRender.SendMessage(nameof(CityRender.Label), Name);

		CargoLevel = 0.3f;
		LinkedCities = new List<City>();
	}

	public void Destroy()
	{
		var r = cityRender.GetComponent<CityRender>();
		r.Destroy();

		//TODO : update links
	}

	static City()
	{
		var allCityNames = Resources.Load("Text/CityNames/françaises") as TextAsset;
		cityNames = allCityNames.text.Split('\n').ToList();
	}

	public int ManhattanDistance(City city)
	{
		return Point.ManhattanDistance(city.Point);
	}

	public int ManhattanDistance(Coord point)
	{
		return Point.ManhattanDistance(point);
	}

	public void AddLinkTo(City c)
	{
		if (!LinkedCities.Contains(c))
		{
			LinkedCities.Add(c);
			UpdateInfoText();
		}
	}

	public void AddLinkTo(List<City> list)
	{
		var addedACity = false;
		foreach (City c in list)
		{
			if (!LinkedCities.Contains(c))
			{
				addedACity = true;
				LinkedCities.Add(c);
			}
		}
		if(addedACity)
		UpdateInfoText();
	}

	public bool IsLinkedTo(City c)
	{
		return LinkedCities.Contains(c);
	}

	public static string RandomName()
	{
		var r = Random.Range(0, cityNames.Count - 1);
		var name = cityNames[r];
		cityNames.RemoveAt(r);
		return name;
	}

	public static int Quantity(int w, int h)
	{
		var averageSquareSize = Mathf.Sqrt(w * h);
		return Mathf.RoundToInt(Mathf.Sqrt(averageSquareSize * 2)) + 1;
	}

	public int GenerateCargo()
	{
		var quantity = 0;
		if (Random.value > CargoLevel)
		{
			//Debug.Log ("Cargo généré pour " + name);
			quantity++;
		}
		return quantity;
	}

	public override bool Equals(object obj)
	{
		var n = obj as City;
		return Point.Equals(n.Point);
	}

	public override int GetHashCode()
	{
		return Point.GetHashCode() ^ Name.GetHashCode();
	}

	public void ShowInfo()
	{
		var screenPosition = Camera.main.WorldToScreenPoint(cityRender.transform.position);
		infoWindow = WindowFactory.BuildTextInfo(Name, screenPosition, InfoText());
	}

	private string InfoText()
	{
		StringBuilder sb = new StringBuilder();

		sb.Append($"<b>Position</b>: {Point}\n");
		sb.Append("<b>Lié aux villes:\n");
		var linkedCities = LinkedCities.OrderBy(c => ManhattanDistance(c));
		foreach (City c in linkedCities)
		{
			sb.Append($"\t{c.Name} \r({ManhattanDistance(c)} cases)\n");
		}

		return sb.ToString().Replace("\r","");
	}

	public void UpdateInfoText()
	{
		if (infoWindow != null)
		{
			infoWindow.TextContent(InfoText());
		}
	}
}


