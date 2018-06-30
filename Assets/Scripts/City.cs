using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class City : Construction
{
	public static readonly Vector2 CargoChanceRange = new Vector2(.1f, 1f);
	public static readonly Vector2 CargoProductionRange = new Vector2(0.002f, 0.02f);

	Component cityRender; //TODO : déplacer dans Construction
	public string Name { get; } //TODO : variable globale commence par maj

	public float CargoChance { get; }
	public float CargoProduction { get; }
	private float _cargo;
	public int Cargo { get; private set; } = 0;

	private List<Flux> incomingFlux;
	private List<Flux> outgoingFlux;

	public List<City> LinkedCities { get; }
	public WindowTextInfo InfoWindow = null;

	private static List<string> cityNames = null;
	

	public City(Coord position, Component cityPrefab)
	{
		Point = position;
		cityRender = CityRender.Build(new Vector3(Point.X, 0f, Point.Y), cityPrefab);
		var objectRenderer = cityRender.GetComponentInChildren<CityObjectRender>();
		objectRenderer.City(this);

		Name = RandomName();
		UpdateLabel();

		Cargo = 0;
		CargoChance = Random.Range(CargoChanceRange.x, CargoChanceRange.y);
		CargoProduction = Random.Range(CargoProductionRange.x, CargoProductionRange.y);
		LinkedCities = new List<City>();
		incomingFlux = new List<Flux>();
		outgoingFlux = new List<Flux>();
	}

	public void UpdateLabel()
	{
		var label = $"{Name} [{Cargo}]";
		cityRender?.SendMessage(nameof(CityRender.Label), label);
	}

	public void Destroy()
	{
		InfoWindow.Close();
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
			UpdateInformations();
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
		UpdateInformations();
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

	public void GenerateCargo()
	{
		if (Random.value > CargoChance)
		{
			_cargo += CargoProduction;
			Cargo = Mathf.FloorToInt(_cargo);
			UpdateInformations();
		}
	}

	public void ReferenceFlux(Flux flux, Flux.Direction direction)
	{
		if (direction == Flux.Direction.incoming)
			incomingFlux.Add(flux);
		else
			outgoingFlux.Add(flux);

	}

	public bool DistributeCargo(int quantity)
	{
		if (_cargo >= quantity)
		{
			_cargo -= quantity;
			Cargo = Mathf.FloorToInt(_cargo);
			UpdateInformations();
			return true;
		}
		else
			return false;
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
		if (InfoWindow == null)
		{
			var screenPosition = Camera.main.WorldToScreenPoint(cityRender.transform.position);
			InfoWindow = WindowFactory.BuildTextInfo(Name, screenPosition, this);
		}
	}

	public string InfoText()
	{
		StringBuilder sb = new StringBuilder();

		sb.Append($"<b>Stock</b>: {Cargo} caisse{((Cargo > 1)?"s":"")} de cargo ({Mathf.Round(100*_cargo)/100})\n");
		sb.Append($"<b>Génération de cargo</b>:\n");
		sb.Append($"\tProbabilité de {(int)(CargoChance*100f)}%\n\tProduction à {Mathf.Round(100*CargoProduction*(1f/Simulation.TickFrequency))/100}/s\n");
		sb.Append($"<b>Position</b>: {Point}\n");
		sb.Append("<b>Production:</b>\n");
		if (outgoingFlux.Count == 0)
			sb.Append("\tExport: aucun\n");
		else
		{
			sb.Append("\tExport:\n");
			foreach (Flux f in outgoingFlux)
				sb.Append($"\t\t{f.TotalMoved} vers {f.Target} \r({ManhattanDistance(f.Target)} cases)\n");
		}
		if (outgoingFlux.Count == 0)
			sb.Append("\tImport: aucun\n");
		else
		{
			sb.Append("\tImport:\n");
			foreach (Flux f in incomingFlux)
				sb.Append($"\t\t{f.TotalMoved} depuis {f.Source} \r({ManhattanDistance(f.Source)} cases)\n");
		}
		sb.Append("<b>Lié aux villes</b>:\n");
		var linkedCities = LinkedCities.OrderBy(c => ManhattanDistance(c));
		foreach (City c in linkedCities)
		{
			sb.Append($"\t{c.Name} \r({ManhattanDistance(c)} cases)\n");
		}

		return sb.ToString().Replace("\r","");
	}

	public void UpdateInformations()
	{
		if (InfoWindow != null)
		{
			InfoWindow.TextContent(InfoText());
		}
		UpdateLabel();
	}

	public override string ToString()
	{
		return Name;
	}
}


