using System.Collections.Generic;
using UnityEngine;

public class HCargoGenerator : ICargoProvider, ICargoGenerator
{
    public delegate void CargoUpdatedDelegate();

    public Dictionary<ICargoAccepter, Flux> OutgoingFlux { get; private set; }

    public float CargoChance { get; private set; }
    public float CargoProduction { get; private set; }

    public float ExactCargo { get; private set; }
    public int Cargo { get; private set; } = 0;

    private CargoUpdatedDelegate cargoUpdatedDelegate;


    public HCargoGenerator(CargoUpdatedDelegate cargoUpdatedDelegate)
        : this(cargoUpdatedDelegate, new Vector2(.1f, 1f), new Vector2(0.002f, 0.02f))
    { }

    public HCargoGenerator(CargoUpdatedDelegate cargoUpdatedDelegate, Vector2 cargoChanceRange, Vector2 cargoProductionRange)
        : this(cargoUpdatedDelegate, Random.Range(cargoChanceRange.x, cargoChanceRange.y), Random.Range(cargoProductionRange.x, cargoProductionRange.y), 0)
    { }

    public HCargoGenerator(CargoUpdatedDelegate cargoUpdatedDelegate, float cargoChance, float cargoProduction, float exactCargo)
    {
        OutgoingFlux = new Dictionary<ICargoAccepter, Flux>();

        CargoChance = cargoChance;
        CargoProduction = cargoProduction;

        ExactCargo = exactCargo;
        UpdateCargo();

        this.cargoUpdatedDelegate = cargoUpdatedDelegate;
    }

    public bool GenerateCargo()
    {
        if (Random.value <= CargoChance)
        {
            ExactCargo += CargoProduction;
            UpdateCargo();
            return true;
        }
        else
            return false;
    }

    private void UpdateCargoInformation()
    {
        cargoUpdatedDelegate?.Invoke();
    }

    private void UpdateCargo()
    {
        Cargo = Mathf.FloorToInt(ExactCargo);
        UpdateCargoInformation();
    }

    public bool ProvideCargo(int quantity)
    {
        if (ExactCargo >= quantity)
        {
            ExactCargo -= quantity;
            UpdateCargo();
            return true;
        }
        else
            return false;
    }


    public void ReferenceFlux(Flux flux)
    {
        var t = flux?.Target;
        if (!OutgoingFlux.ContainsKey(t))
            OutgoingFlux.Add(t, flux);
    }

    public void RemoveFlux(Flux flux)
    {
        var t = flux?.Target;
        if (OutgoingFlux.ContainsKey(t))
            OutgoingFlux.Remove(t);
    }
}

