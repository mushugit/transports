using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class City : Construction
{

	public static readonly Vector2 CargoChanceRange = new Vector2(.1f, 1f);
	public static readonly Vector2 CargoProductionRange = new Vector2(0.002f, 0.02f);

	Component cityRender; //TODO : déplacer dans Construction

	[JsonProperty]
	public string Name { get; private set; } //TODO : variable globale commence par maj

	[JsonProperty]
	public float CargoChance { get; private set; }
	[JsonProperty]
	public float CargoProduction { get; private set; }
	[JsonProperty]
	public float ExactCargo { get; private set; }

	public int Cargo { get; private set; } = 0;

	private List<Flux> incomingFlux;
	private List<Flux> outgoingFlux;

	public List<City> LinkedCities { get; private set; }
	public WindowTextInfo InfoWindow = null;

	private static List<string> cityNames = null;

	private void SetupCity(Coord position, Component cityPrefab, string name, float cargoChance, float cargoProduction, float exactCargo)
	{
		Point = position;
		if (cityPrefab != null)
		{
			cityRender = CityRender.Build(new Vector3(Point.X, 0f, Point.Y), cityPrefab);
			var objectRenderer = cityRender.GetComponentInChildren<CityObjectRender>();
			objectRenderer.City(this);
		}

		Name = name;
		UpdateLabel();

		this.ExactCargo = exactCargo;
		UpdateCargo();
		CargoChance = cargoChance;
		CargoProduction = cargoProduction;

		LinkedCities = new List<City>();
		incomingFlux = new List<Flux>();
		outgoingFlux = new List<Flux>();
	}

	[JsonConstructor]
	public City(Coord position, Component cityPrefab, string name, float cargoChance, float cargoProduction, float exactCargo)
	{
		SetupCity(position, cityPrefab, name, cargoChance, cargoProduction, exactCargo);
	}

	public City(City dummyCity, Component cityPrefab)
	{
		SetupCity(dummyCity.Point, cityPrefab, dummyCity.Name, dummyCity.CargoChance, dummyCity.CargoProduction, dummyCity.ExactCargo);
	}

	public City(Coord position, Component cityPrefab)
	{
		var name = RandomName();
		var cargo = 0f;

		var cargoChance = Random.Range(CargoChanceRange.x, CargoChanceRange.y);
		var cargoProduction = Random.Range(CargoProductionRange.x, CargoProductionRange.y);

		SetupCity(position, cityPrefab, name, cargoChance, cargoProduction, cargo);
	}

	public void UpdateLabel()
	{
		var label = $"{Name} [{Cargo}]";
		cityRender?.SendMessage(nameof(CityRender.Label), label);
	}

	public void Destroy()
	{
		InfoWindow.Close();
		var r = cityRender?.GetComponent<CityRender>();
		r?.Destroy();

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

	public float FlyDistance(City city)
	{
		return Point.FlyDistance(city.Point);
	}

	public float FlyDistance(Coord point)
	{
		return Point.FlyDistance(point);
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
		if (addedACity)
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
			ExactCargo += CargoProduction;
			Cargo = Mathf.FloorToInt(ExactCargo);
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

	private void UpdateCargo()
	{
		Cargo = Mathf.FloorToInt(ExactCargo);
	}

	public bool DistributeCargo(int quantity)
	{
		if (ExactCargo >= quantity)
		{
			ExactCargo -= quantity;
			UpdateCargo();
			UpdateInformations();
			return true;
		}
		else
			return false;
	}

	public override bool Equals(object obj)
	{
		var n = obj as City;
		return Point.Equals(n?.Point);
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

		sb.Append($"<b>Stock</b>: {Cargo} caisse{((Cargo > 1) ? "s" : "")} de cargo ({Mathf.Round(100 * ExactCargo) / 100})\n");
		sb.Append($"<b>Génération de cargo</b>:\n");
		sb.Append($"\tProbabilité de {(int)(CargoChance * 100f)}%\n\tProduction à {Mathf.Round(100 * CargoProduction * (1f / Simulation.TickFrequency)) / 100}/s\n");
		sb.Append($"<b>Position</b>: {Point}\n");
		sb.Append("<b>Production:</b>\n");
		if (outgoingFlux.Count == 0)
			sb.Append("\tExport: aucun\n");
		else
		{
			sb.Append("\tExport:\n");
			foreach (Flux f in outgoingFlux)
				sb.Append($"\t\t{f.TotalCargoMoved} vers {f.Target} \r({ManhattanDistance(f.Target)} cases)\n");
		}
		if (outgoingFlux.Count == 0)
			sb.Append("\tImport: aucun\n");
		else
		{
			sb.Append("\tImport:\n");
			foreach (Flux f in incomingFlux)
				sb.Append($"\t\t{f.TotalCargoMoved} depuis {f.Source} \r({ManhattanDistance(f.Source)} cases)\n");
		}
		sb.Append("<b>Lié aux villes</b>:\n");
		var linkedCities = LinkedCities.OrderBy(c => ManhattanDistance(c));
		foreach (City c in linkedCities)
		{
			sb.Append($"\t{c.Name} \r({ManhattanDistance(c)} cases)\n");
		}

		return sb.ToString().Replace("\r", "");
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


