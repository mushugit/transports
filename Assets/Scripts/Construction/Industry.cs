using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System;
using UnityEngine.EventSystems;

[JsonObject(MemberSerialization.OptIn)]
public class Industry : Construction, IEquatable<Industry>, IFluxSource, ICargoStocker, ICargoGenerator, ILinkable, IHasName
{
    HCargoGenerator cargoGenerator;
    HLinkHandler linkHandler;

    private static Dictionary<City, int> numberOfIndustryPerCity;

    #region IHasName
    [JsonProperty]
    public string Name { get; private set; }
    #endregion

    #region ILinkable Properties
    public List<ILinkable> Linked { get { return linkHandler.Linked; } }
    public List<ILinkable> Unreachable { get { return linkHandler.Unreachable; } }
    #endregion  

    #region ICargoProvider properties
    [JsonProperty]
    public float CargoChance { get { return cargoGenerator.CargoChance; } }
    [JsonProperty]
    public float CargoProduction { get { return cargoGenerator.CargoProduction; } }
    [JsonProperty]
    public float ExactCargo { get { return cargoGenerator.ExactCargo; } }

    public int Cargo { get { return cargoGenerator.Cargo; } }

    public Dictionary<ICargoAccepter, Flux> OutgoingFlux { get { return cargoGenerator.OutgoingFlux; } }
    #endregion

    #region Constructor
    public Industry(Cell cell)
        : base(cell, World.Instance?.IndustryPrefab, World.Instance?.IndustryContainer)
    {
        cargoGenerator = new HCargoGenerator(UpdateLabel, this);
        linkHandler = new HLinkHandler(cell, null);
        var city = World.Instance?.ClosestCity(cell);
        if (city != null)
        {
            InitCityReference();
            var n = ++numberOfIndustryPerCity[city];
            Name = $"Industrie #{n} de {city.Name}";
        }
        else
            Name = "Industrie";

        UpdateLabel();
    }

    public Industry(Industry dummyIndustry)
        : base(dummyIndustry._Cell, World.Instance?.IndustryPrefab, World.Instance?.IndustryContainer)
    {
        cargoGenerator = new HCargoGenerator(UpdateLabel, this, dummyIndustry.CargoChance, dummyIndustry.CargoProduction, dummyIndustry.ExactCargo);
        linkHandler = new HLinkHandler(dummyIndustry._Cell, null);
        Name = dummyIndustry.Name;
    }

    [JsonConstructor]
    public Industry(Cell _cell, string name, float cargoChance, float cargoProduction, float exactCargo,
        float colorR, float colorG, float colorB, float colorA)
        : base(_cell, World.Instance?.IndustryPrefab, World.Instance?.IndustryContainer)
    {
        cargoGenerator = new HCargoGenerator(UpdateLabel, this, cargoChance, cargoProduction, exactCargo);
        linkHandler = new HLinkHandler(_cell, null);
        Name = name;
        SetColor(new Color(colorR, colorG, colorB, colorA));
    }
    #endregion

    #region ICargoProvider
    public bool ProvideCargo(int quantity)
    {
        return cargoGenerator.ProvideCargo(quantity);
    }

    public void ReferenceFlux(Flux flux)
    {
        cargoGenerator.ReferenceFlux(flux);
    }

    public void RemoveFlux(Flux flux)
    {
        cargoGenerator.RemoveFlux(flux);
    }

    public void UpdateAllOutgoingFlux()
    {
        cargoGenerator.UpdateAllOutgoingFlux();
    }
    #endregion

    #region ICargoGenerator
    public bool GenerateCargo()
    {
        return cargoGenerator.GenerateCargo();
    }
    #endregion

    #region IHasColor
    [JsonProperty]
    public float ColorR { get { return Color.r; } }
    [JsonProperty]
    public float ColorG { get { return Color.g; } }
    [JsonProperty]
    public float ColorB { get { return Color.b; } }
    [JsonProperty]
    public float ColorA { get { return Color.a; } }
    private Color color = Color.black;
    public Color Color
    {
        get
        {
            if (color != Color.black)
                return color;
            else
            {
                var internalRenderer = GlobalRenderer?.GetComponentInChildren<Renderer>();
                if (internalRenderer != null)
                    return internalRenderer.material.color;
                else return Color.black;
            }
        }
    }
    public void SetColor(Color color)
    {
        this.color = color;
        var renderers = GlobalRenderer?.GetComponentsInChildren<Renderer>();
        if (renderers != null)
        {
            foreach (Renderer r in renderers)
            {
                r.material.color = color;
            }
        }
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

    #region IEquatable<Industry>
    public bool Equals(Industry other)
    {
        if (other == null) return false;
        return _Cell.Equals(other._Cell);
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

    #region Name and label
    public void UpdateLabel()
    {
        var label = $"{Name} [{Cargo}]";
        var renderer = GlobalRenderer?.GetComponentInChildren<IUnityLabelable>();
        renderer?.Label(label);
    }
    #endregion

    public void UpdateInformations()
    {

    }

    public override string ToString()
    {
        return Name;
    }


    public static int Quantity(int w, int h)
    {
        var averageSquareSize = Mathf.Sqrt(w * h);
        return Mathf.FloorToInt(Mathf.Sqrt(averageSquareSize * 2f) * (3f / 5f)) + 1;
    }

    public override void ClickHandler(PointerEventData eventData)
    {

    }

    private static void InitCityReference()
    {
        if (numberOfIndustryPerCity == null)
        {
            var cities = World.Instance.Cities;
            numberOfIndustryPerCity = new Dictionary<City, int>(cities.Count);
            foreach (City c in cities)
            {
                numberOfIndustryPerCity.Add(c, 0);
            }
        }
    }
}

