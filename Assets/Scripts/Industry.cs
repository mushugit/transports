using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System;

[JsonObject(MemberSerialization.OptIn)]
public class Industry : Construction, IEquatable<Industry>, ICargoGenerator
{
    public Component IndustryRenderComponent { get; private set; }

    [JsonProperty]
    public string Name { get; private set; }

    public List<City> LinkedCities { get; private set; }
    public List<City> UnreachableCities { get; private set; }

    public Vector2 CargoChanceRange { get; private set; } = new Vector2(.1f, 1f);
    public Vector2 CargoProductionRange { get; private set; } = new Vector2(0.002f, 0.02f);

    [JsonProperty]
    public float CargoChance { get; private set; }
    [JsonProperty]
    public float CargoProduction { get; private set; }
    [JsonProperty]
    public float ExactCargo { get; private set; }

    public int Cargo { get; private set; } = 0;

    public Dictionary<Construction, Flux> OutgoingFlux { get; private set; }

    private void SetupIndustry(Cell position, Component prefab, string name)
    {
        Point = position;
        Name = name;
        if (prefab != null)
        {
            IndustryRenderComponent = IndustryRender.Build(new Vector3(Point.X, 0f, Point.Y), prefab);
            var objectRenderer = IndustryRenderComponent.GetComponentInChildren<IndustryObjectRender>();
            objectRenderer.Industry(this);
        }

        UpdateLabel();

        LinkedCities = new List<City>();
        UnreachableCities = new List<City>();

        OutgoingFlux = new Dictionary<Construction, Flux>();
    }

    protected Industry()
    {
    }

    protected Industry(Cell point) : base(point)
    {
    }

    protected Industry(int x, int y) : base(x, y)
    {
    }

    public bool Equals(Industry other)
    {
        var e = Point.Equals(other?.Point);
        //Debug.Log($"Testing if {this} equals {other} : {e}");
        return e;
    }

    public void UpdateLabel()
    {
        var label = $"{Name} [{Cargo}]";
        IndustryRenderComponent?.SendMessage(nameof(CityRender.Label), label);
    }

    public override void Destroy()
    {

    }

    public void ReferenceFlux(Flux flux)
    {
        throw new NotImplementedException();
    }

    public void UpdateCargo()
    {
        throw new NotImplementedException();
    }

    public bool DistributeCargo(int quantity)
    {
        throw new NotImplementedException();
    }

    public void RemoveFlux(Flux f)
    {
        throw new NotImplementedException();
    }

    public void GenerateCargo()
    {
        throw new NotImplementedException();
    }
}

