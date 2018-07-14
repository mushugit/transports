using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System;
using UnityEngine.EventSystems;

[JsonObject(MemberSerialization.OptIn)]
public class Industry : Construction, IEquatable<Industry>, ICargoProvider, ICargoStocker
{
    HCargoGenerator cargoGenerator;

    [JsonProperty]
    public string Name { get; private set; } //TODO : variable globale commence par maj

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
        :base(cell, World.Instance.IndustryPrefab, World.Instance.IndustryContainer)
    {
        cargoGenerator = new HCargoGenerator(UpdateLabel);
        var city = World.Instance.ClosestCity(cell);
        Name = $"Industrie de {city.Name}";
    }
    #endregion

    #region ICargoProvider
    public bool ProvideCargo(int quantity)
    {
        throw new NotImplementedException();
    }

    public void ReferenceFlux(Flux flux)
    {
        throw new NotImplementedException();
    }

    public void RemoveFlux(Flux flux)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region IEquatable<Industry>
    public bool Equals(Industry other)
    {
        if (other == null) return false;
        return _Cell.Equals(other._Cell);
    }
    #endregion

    #region Name and label
    public void UpdateLabel()
    {
        var label = $"{Name} [{Cargo}]";
        var cityRender = GlobalRenderer as IUnityLabelable;
        cityRender.Label(label);
    }
    #endregion

    public override void ClickHandler(PointerEventData eventData)
    {

    }
}

