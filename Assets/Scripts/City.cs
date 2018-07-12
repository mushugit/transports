using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System;

[JsonObject(MemberSerialization.OptIn)]
public class City : Construction, IEquatable<City>, IFluxSource, IFluxTarget, ICargoStocker
{

    public Vector2 CargoChanceRange { get; private set; } = new Vector2(.1f, 1f);
    public Vector2 CargoProductionRange { get; private set; } = new Vector2(0.002f, 0.02f);

    private Component CityRenderComponent; //TODO : déplacer dans Construction

    [JsonProperty]
    public string Name { get; private set; } //TODO : variable globale commence par maj

    [JsonProperty]
    public float CargoChance { get; private set; }
    [JsonProperty]
    public float CargoProduction { get; private set; }
    [JsonProperty]
    public float ExactCargo { get; private set; }

    public int Cargo { get; private set; } = 0;

    public Dictionary<ICargoProvider, Flux> IncomingFlux { get; private set; }
    public Dictionary<ICargoAccepter, Flux> OutgoingFlux { get; private set; }

    public List<City> LinkedCities { get; private set; }
    public List<City> UnreachableCities { get; private set; }
    public WindowTextInfo InfoWindow = null;


    public Color Color
    {
        get
        {
            var cityObjectRender = CityRenderComponent.GetComponentInChildren<CityObjectRender>();
            var internalRenderer = cityObjectRender.GetComponentInChildren<Renderer>();
            return internalRenderer.material.color;
        }
    }

    private static List<string> cityNames = null;

    private void SetupCity(Cell position, Component cityPrefab, string name, float cargoChance, float cargoProduction, float exactCargo)
    {
        Coord = position;
        if (cityPrefab != null)
        {
            CityRenderComponent = CityRender.Build(new Vector3(Coord.X, 0f, Coord.Y), cityPrefab);
            var objectRenderer = CityRenderComponent.GetComponentInChildren<CityObjectRender>();
            objectRenderer.City(this);
        }

        Name = name;
        UpdateLabel();

        this.ExactCargo = exactCargo;
        UpdateCargo();
        CargoChance = cargoChance;
        CargoProduction = cargoProduction;

        LinkedCities = new List<City>();
        UnreachableCities = new List<City>();
        IncomingFlux = new Dictionary<ICargoProvider, Flux>();
        OutgoingFlux = new Dictionary<ICargoAccepter, Flux>();
    }

    [JsonConstructor]
    public City(Cell position, Component cityPrefab, string name, float cargoChance, float cargoProduction, float exactCargo)
    {
        SetupCity(position, cityPrefab, name, cargoChance, cargoProduction, exactCargo);
    }

    public City(City dummyCity, Component cityPrefab)
    {
        SetupCity(dummyCity.Coord, cityPrefab, dummyCity.Name, dummyCity.CargoChance, dummyCity.CargoProduction, dummyCity.ExactCargo);
    }

    public City(Cell position, Component cityPrefab)
    {
        var name = RandomName();
        var cargo = 0f;

        var cargoChance = UnityEngine.Random.Range(CargoChanceRange.x, CargoChanceRange.y);
        var cargoProduction = UnityEngine.Random.Range(CargoProductionRange.x, CargoProductionRange.y);

        SetupCity(position, cityPrefab, name, cargoChance, cargoProduction, cargo);
    }

    public void UpdateLabel()
    {
        var label = $"{Name} [{Cargo}]";
        CityRenderComponent?.SendMessage(nameof(CityRender.Label), label);
    }

    public override void Destroy()
    {
        InfoWindow.Close();
        var r = CityRenderComponent?.GetComponent<CityRender>();
        r?.Destroy();

        //TODO : update links
    }

    static City()
    {
        var allCityNames = Resources.Load("Text/CityNames/françaises") as TextAsset;
        cityNames = allCityNames.text.Split('\n').ToList();
    }

    public int ManhattanDistance(IHasCell target)
    {
        return Coord.ManhattanDistance(target.Coord);
    }

    public double FlyDistance(IHasCell target)
    {
        return Coord.FlyDistance(target.Coord);
    }

    public double FlyDistance(Cell point)
    {
        return Coord.FlyDistance(point);
    }

    public void ClearLinks()
    {
        LinkedCities.Clear();
        UnreachableCities.Clear();
    }

    public void AddUnreachable(City c)
    {
        if (!UnreachableCities.Contains(c))
        {
            UnreachableCities.Add(c);
            UpdateInformations();
        }
    }

    public void AddUnreachable(List<City> list)
    {
        var addedACity = false;
        foreach (City c in list)
        {
            if (!UnreachableCities.Contains(c))
            {
                addedACity = true;
                UnreachableCities.Add(c);
            }
        }
        if (addedACity)
            UpdateInformations();
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

    public bool IsUnreachable(City c)
    {
        return UnreachableCities.Contains(c);
    }

    public bool IsLinkedTo(City c)
    {
        return LinkedCities.Contains(c);
    }

    public static string RandomName()
    {
        var r = UnityEngine.Random.Range(0, cityNames.Count - 1);
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
        if (UnityEngine.Random.value <= CargoChance)
        {
            ExactCargo += CargoProduction;
            Cargo = Mathf.FloorToInt(ExactCargo);
            UpdateInformations();
        }
    }

    public void ReferenceFlux(Flux flux)
    {
        //Debug.Log($"Add {direction} to {this} : {flux}");
        if (flux.Target == this)
            IncomingFlux.Add(flux.Source, flux);
        else
            OutgoingFlux.Add(flux.Target, flux);

    }

    public void UpdateCargo()
    {
        Cargo = Mathf.FloorToInt(ExactCargo);
    }

    public bool ProvideCargo(int quantity)
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
        return Coord.Equals(n?.Coord);
    }

    public override int GetHashCode()
    {
        return Coord.GetHashCode() ^ Name.GetHashCode();
    }

    public void ShowInfo()
    {
        if (InfoWindow == null)
        {
            var screenPosition = Camera.main.WorldToScreenPoint(CityRenderComponent.transform.position);
            InfoWindow = WindowFactory.BuildTextInfo(Name, screenPosition, this);
        }
    }

    public void RemoveFlux(Flux f)
    {
        if (OutgoingFlux.ContainsKey(f.Target))
            OutgoingFlux.Remove(f.Target);

        if (IncomingFlux.ContainsKey(f.Source))
            IncomingFlux.Remove(f.Source);
    }

    public void UpdateAllOutgoingFlux()
    {
        foreach (var k in OutgoingFlux)
        {
            k.Value.UpdateTruckPath();
        }
    }

    public int RoadInDirection(Cell target)
    {

        var values = new int[9, 9];
        var multiplier = new int[9, 9];


        if (target.X > Coord.X)
        {
            for (int y = 0; y < 3; y++)
            {
                values[1, y] += 1;
                values[2, y] += 2;
            }
        }
        if (Coord.X < target.X)
        {
            for (int y = 0; y < 3; y++)
            {
                values[1, y] += 1;
                values[0, y] += 2;
            }
        }
        if (target.X == Coord.X)
        {
            for (int y = 0; y < 3; y++)
            {
                multiplier[1, y] = 2;
            }
        }

        if (target.Y > Coord.Y)
        {
            for (int x = 0; x < 3; x++)
            {
                values[x, 1] += 1;
                values[x, 2] += 2;
            }
        }
        if (Coord.Y < target.Y)
        {
            for (int x = 0; x < 3; x++)
            {
                values[x, 1] += 1;
                values[x, 0] += 2;
            }
        }
        if (target.Y == Coord.Y)
        {
            for (int x = 0; x < 3; x++)
            {
                multiplier[x, 1] = 2;
            }
        }

        var g = 0;
        values[1, 1] = 0;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                g += values[i, j] * ((multiplier[i, j] != 0) ? multiplier[i, j] : 1);
            }
        }
        return g;
    }

    public string InfoText()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append($"<b>Stock</b>: {Cargo} caisse{((Cargo > 1) ? "s" : "")} de cargo ({Mathf.Round(100 * ExactCargo) / 100})\n");
        sb.Append($"<b>Génération de cargo</b>:\n");
        sb.Append($"\tProbabilité de {(int)(CargoChance * 100f)}%\n\tProduction à {Mathf.Round(100 * CargoProduction * (1f / Simulation.TickFrequency)) / 100}/s\n");
        sb.Append($"<b>Position</b>: {Coord}\n");
        sb.Append("<b>Production:</b>\n");

        if (OutgoingFlux.Count == 0)
            sb.Append("\tExport: aucun\n");
        else
        {
            sb.Append("\tExport:\n");
            foreach (var k in OutgoingFlux)
            {
                sb.Append($"\t\t{k.Value.TotalCargoMoved} vers {k.Key} \r");
                if (k.Value.IsWaitingForDelivery)
                    sb.Append($"\n\t\t\t<color=\"red\">Attente d'espace pour livrer</color>\n\t\t\t");
                if (k.Value.IsWaitingForInput)
                    sb.Append($"\n\t\t\t<color=\"red\">Attente de marchandise à livrer</color>\n\t\t\t");
                if (k.Value.IsWaitingForPath)
                    sb.Append($"\n\t\t\t<color=\"red\">Pas de chemin !</color>\n\t\t\t");
            }
        }

        if (IncomingFlux.Count == 0)
            sb.Append("\tImport: aucun\n");
        else
        {
            sb.Append("\tImport:\n");
            foreach (var k in IncomingFlux)
            {
                sb.Append($"\t\t{k.Value.TotalCargoMoved} depuis {k.Key} \r");
                if (k.Value.IsWaitingForDelivery)
                    sb.Append($"\n\t\t\t<color=\"red\">Attente d'espace pour livrer</color>\n\t\t\t");
                if (k.Value.IsWaitingForInput)
                    sb.Append($"\n\t\t\t<color=\"red\">Attente de marchandise à livrer</color>\n\t\t\t");
                if (k.Value.IsWaitingForPath)
                    sb.Append($"\n\t\t\t<color=\"red\">Pas de chemin !</color>\n\t\t\t");
            }
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

    public bool Equals(City other)
    {
        var e = Coord.Equals(other?.Coord);
        //Debug.Log($"Testing if {this} equals {other} : {e}");
        return e;
    }

    public bool DistributeCargo(int quantity)
    {
        return true;
    }
}


