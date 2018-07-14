using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System;
using UnityEngine.EventSystems;

[JsonObject(MemberSerialization.OptIn)]
public class City : Construction, IEquatable<City>, IFluxSource, IFluxTarget, ICargoStocker, ICargoGenerator, ILinkable, IHasName
{
    private static List<string> cityNames = null;

    #region IHasName
    [JsonProperty]
    public string Name { get; private set; }
    #endregion

    HCargoGenerator cargoGenerator;
    HLinkHandler linkHandler;

    #region ICargoProvider Properties
    [JsonProperty]
    public float CargoChance { get { return cargoGenerator.CargoChance; } }
    [JsonProperty]
    public float CargoProduction { get { return cargoGenerator.CargoProduction; } }
    [JsonProperty]
    public float ExactCargo { get { return cargoGenerator.ExactCargo; } }

    public int Cargo { get { return cargoGenerator.Cargo; } }

    public Dictionary<ICargoAccepter, Flux> OutgoingFlux { get { return cargoGenerator.OutgoingFlux; } }
    #endregion

    public Dictionary<ICargoProvider, Flux> IncomingFlux { get; private set; }

    #region ILinkable Properties
    public List<ILinkable> Linked { get { return linkHandler.Linked; } }
    public List<ILinkable> Unreachable { get { return linkHandler.Unreachable; } }
    #endregion  

    public WindowTextInfo InfoWindow = null;

    #region IHasColor
    public Color Color
    {
        get
        {
            var internalRenderer = GlobalRenderer.GetComponentInChildren<Renderer>();
            return internalRenderer.material.color;
        }
    }
    #endregion

    #region IEquatable<City>
    public bool Equals(City other)
    {
        var e = _Cell.Equals(other?._Cell);
        //Debug.Log($"Testing if {this} equals {other} : {e}");
        return e;
    }
    #endregion

    static City()
    {
        var allCityNames = Resources.Load("Text/CityNames/françaises") as TextAsset;
        cityNames = allCityNames.text.Split('\n').ToList();
    }

    #region Initializer
    private void InitCity(Cell position, string name)
    {
        _Cell = position;
        Name = name;
        UpdateLabel();

        IncomingFlux = new Dictionary<ICargoProvider, Flux>();
    }

    private void SetupCity(Cell position, string name)
    {
        cargoGenerator = new HCargoGenerator(UpdateInformations, this, HCargoGenerator.CargoLevel.LowCargo);
        linkHandler = new HLinkHandler(this._Cell, UpdateInformations);

        InitCity(position, name);
    }

    private void SetupCity(Cell position, string name, float cargoChance, float cargoProduction, float exactCargo)
    {
        cargoGenerator = new HCargoGenerator(UpdateInformations, this, cargoChance, cargoProduction, exactCargo);
        linkHandler = new HLinkHandler(this._Cell, UpdateInformations);

        InitCity(position, name);
    }
    #endregion

    #region Constructor
    public City(City dummyCity)
        : base(dummyCity._Cell, World.Instance?.CityPrefab, World.Instance?.CityContainer)
    {
        SetupCity(dummyCity._Cell, dummyCity.Name, dummyCity.CargoChance, dummyCity.CargoProduction, dummyCity.ExactCargo);
    }

    public City(Cell cell)
        : base(cell, World.Instance.CityPrefab, World.Instance.CityContainer)
    {
        SetupCity(cell, RandomName());
    }

    [JsonConstructor]
    public City(Cell _cell, string name, float cargoChance, float cargoProduction, float exactCargo)
        : base(_cell, World.Instance?.CityPrefab, World.Instance?.CityContainer)
    {
        SetupCity(_cell, name, cargoChance, cargoProduction, exactCargo);
    }
    #endregion

    #region Construction override
    public override void ClickHandler(PointerEventData eventData)
    {
        ShowInfo();
    }
    #endregion

    #region Name and label
    public void UpdateLabel()
    {
        var label = $"{Name} [{Cargo}]";
        var cityRender = GlobalRenderer?.GetComponentInChildren<IUnityLabelable>();
        cityRender?.Label(label);
    }

    public static string RandomName()
    {
        var r = UnityEngine.Random.Range(0, cityNames.Count - 1);
        var name = cityNames[r];
        cityNames.RemoveAt(r);
        return name;
    }
    #endregion

    #region IHasRelativeDistance
    public int ManhattanDistance(IHasCell target)
    {
        return _Cell.ManhattanDistance(target._Cell);
    }

    public double FlyDistance(IHasCell target)
    {
        return _Cell.FlyDistance(target._Cell);
    }

    public double FlyDistance(Cell point)
    {
        return _Cell.FlyDistance(point);
    }
    #endregion

    #region IFluxReferencer
    public void ReferenceFlux(Flux flux)
    {
        //Debug.Log($"Add {this} : {flux}");
        if (flux.Target == this)
            IncomingFlux.Add(flux.Source, flux);
        else
            cargoGenerator.ReferenceFlux(flux);

    }

    public void RemoveFlux(Flux flux)
    {
        cargoGenerator.RemoveFlux(flux);

        if (IncomingFlux.ContainsKey(flux.Source))
            IncomingFlux.Remove(flux.Source);
    }
    #endregion

    #region ILinkable
    public void ClearLinks()
    {
        linkHandler.ClearLinks();
    }

    public void AddUnreachable(ILinkable c)
    {
        linkHandler.AddUnreachable(c);
    }

    public void AddUnreachable(List<ILinkable> list)
    {
        linkHandler.AddUnreachable(list);
    }

    public void AddLinkTo(ILinkable c)
    {
        linkHandler.AddLinkTo(c);
    }

    public void AddLinkTo(List<ILinkable> list)
    {
        linkHandler.AddLinkTo(list);
    }

    public bool IsUnreachable(ILinkable c)
    {
        return linkHandler.IsUnreachable(c);
    }

    public bool IsLinkedTo(ILinkable c)
    {
        return linkHandler.IsLinkedTo(c);
    }

    public int RoadInDirection(Cell target)
    {
        return linkHandler.RoadInDirection(target);
    }
    #endregion

    public static int Quantity(int w, int h)
    {
        var averageSquareSize = Mathf.Sqrt(w * h);
        return Mathf.RoundToInt(Mathf.Sqrt(averageSquareSize * 2) * (2f/5f)) + 1;
    }

    #region Cargo

    #region ICargoGenerator
    public bool GenerateCargo()
    {
        return cargoGenerator.GenerateCargo();
    }
    #endregion

    #region ICargoProvider
    public bool ProvideCargo(int quantity)
    {
        return cargoGenerator.ProvideCargo(quantity);
    }

    public void UpdateAllOutgoingFlux()
    {
        cargoGenerator.UpdateAllOutgoingFlux();
    }
    #endregion

    #region ICargoAccepter
    public bool DistributeCargo(int quantity)
    {
        return true;
    }
    #endregion

    #endregion

    public override int GetHashCode()
    {
        return _Cell.GetHashCode() ^ Name.GetHashCode();
    }

    public void ShowInfo()
    {
        if (InfoWindow == null)
        {
            var screenPosition = Camera.main.WorldToScreenPoint(GlobalRenderer.transform.position);
            InfoWindow = WindowFactory.BuildTextInfo(Name, screenPosition, this);
        }
    }



    

    public string InfoText()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append($"<b>Stock</b>: {Cargo} caisse{((Cargo > 1) ? "s" : "")} de cargo ({Mathf.Round(100 * ExactCargo) / 100})\n");
        sb.Append($"<b>Génération de cargo</b>:\n");
        sb.Append($"\tProbabilité de {(int)(CargoChance * 100f)}%\n\tProduction à {Mathf.Round(100 * CargoProduction * (1f / Simulation.TickFrequency)) / 100}/s\n");
        sb.Append($"<b>Position</b>: {_Cell}\n");
        sb.Append("<b>Production:</b>\n");

        if (OutgoingFlux.Count == 0)
            sb.Append("\tExport: aucun\n");
        else
        {
            sb.Append("\tExport:\n");
            foreach (var k in OutgoingFlux)
            {
                sb.Append($"\t\t{k.Value.TotalCargoMoved} vers {k.Key} \n");
                if (k.Value.IsWaitingForDelivery)
                    sb.Append($"\t\t\t<color=\"red\">Attente d'espace pour livrer</color>\n");
                if (k.Value.IsWaitingForInput)
                    sb.Append($"\t\t\t<color=\"red\">Attente de marchandise à livrer</color>\n");
                if (k.Value.IsWaitingForPath)
                    sb.Append($"\t\t\t<color=\"red\">Pas de chemin !</color>\n");
            }
        }

        if (IncomingFlux.Count == 0)
            sb.Append("\tImport: aucun\n");
        else
        {
            sb.Append("\tImport:\n");
            foreach (var k in IncomingFlux)
            {
                sb.Append($"\t\t{k.Value.TotalCargoMoved} depuis {k.Key}\n");
                if (k.Value.IsWaitingForDelivery)
                    sb.Append($"\t\t\t<color=\"red\">Attente d'espace pour livrer</color>\n");
                if (k.Value.IsWaitingForInput)
                    sb.Append($"\t\t\t<color=\"red\">Attente de marchandise à livrer</color>\n");
                if (k.Value.IsWaitingForPath)
                    sb.Append($"\t\t\t<color=\"red\">Pas de chemin !</color>\n");
            }
        }
        sb.Append("<b>Lié aux villes</b>:\n");
        var linked = Linked.OrderBy(c => ManhattanDistance(c));
        foreach (ILinkable c in linked)
        {
            if(c is IHasName)
                sb.Append($"\t{(c as IHasName).Name} \r({ManhattanDistance(c)} cases)\n");
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


