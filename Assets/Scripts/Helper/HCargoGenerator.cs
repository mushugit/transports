using System.Collections.Generic;
using UnityEngine;

public class HCargoGenerator : ICargoProvider, ICargoGenerator
{
    public delegate void CargoUpdatedDelegate();

    public enum CargoLevel
    {
        LowCargo,
        MediumCargo,
        HighCargo
    }

    #region ICargoProvider properties
    public Dictionary<ICargoAccepter, Flux> OutgoingFlux { get; private set; }
    #endregion

    #region ILinkable properties
    public List<ILinkable> Linked { get { return parent.Linked; } }
    public List<ILinkable> Unreachable { get { return parent.Unreachable; } }
    #endregion

    #region IHasCell
    public Cell _Cell { get { return parent._Cell; } }
    #endregion

    public float CargoChance { get; private set; }
    public float CargoProduction { get; private set; }

    public float ExactCargo { get; private set; }
    public int Cargo { get; private set; } = 0;


    private readonly CargoUpdatedDelegate cargoUpdatedDelegate;
    private ICargoProvider parent;



    #region Constructors
    public HCargoGenerator(CargoUpdatedDelegate cargoUpdatedDelegate, ICargoProvider parent, CargoLevel cargoLevel = CargoLevel.MediumCargo)
        : this(cargoUpdatedDelegate, parent, CargoChanceRange(cargoLevel), CargoProductionRange(cargoLevel))
    { }

    public HCargoGenerator(CargoUpdatedDelegate cargoUpdatedDelegate, ICargoProvider parent, 
        Vector2 cargoChanceRange, Vector2 cargoProductionRange)
        : this(cargoUpdatedDelegate, parent, Random.Range(cargoChanceRange.x, cargoChanceRange.y), 
              Random.Range(cargoProductionRange.x, cargoProductionRange.y))
    { }

    public HCargoGenerator(CargoUpdatedDelegate cargoUpdatedDelegate, ICargoProvider parent, 
        float cargoChance, float cargoProduction, float exactCargo = 0)
    {
        OutgoingFlux = new Dictionary<ICargoAccepter, Flux>();

        CargoChance = cargoChance;
        CargoProduction = cargoProduction;

        ExactCargo = exactCargo;
        UpdateCargo();

        this.cargoUpdatedDelegate = cargoUpdatedDelegate;
        this.parent = parent;
    }

    #endregion

    #region ICargoGenerator
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
    #endregion

    #region ICargoProvider
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

    public int PeekCargo()
    {
        return Cargo;
    }

    public void UpdateAllOutgoingFlux()
    {
        foreach (var flux in OutgoingFlux)
        {
            //Debug.Log($"Update truck path for {flux}");
            flux.Value.UpdateTruckPath();
        }
    }
    #endregion

    #region IFluxReferencer
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
    #endregion

    #region ILinkable
    public void ClearLinks()
    {
        parent.ClearLinks();
    }
    public void AddUnreachable(ILinkable item)
    {
        parent.AddUnreachable(item);
    }
    public void AddUnreachable(List<ILinkable> list)
    {
        parent.AddUnreachable(list);
    }
    public void AddLinkTo(ILinkable item)
    {
        parent.AddLinkTo(item);
    }
    public void AddLinkTo(List<ILinkable> list)
    {
        parent.AddLinkTo(list);
    }
    public bool IsUnreachable(ILinkable item)
    {
        return parent.IsUnreachable(item);
    }
    public bool IsLinkedTo(ILinkable item)
    {
        return parent.IsLinkedTo(item);
    }
    public int RoadInDirection(Cell c)
    {
        return parent.RoadInDirection(c);
    }
    #endregion

    #region IHasRelativeDistance
    public int ManhattanDistance(IHasCell c)
    {
        return parent.ManhattanDistance(c);
    }
    public double FlyDistance(IHasCell c)
    {
        return parent.FlyDistance(c);
    }
    #endregion

    private static Vector2 CargoChanceRange(CargoLevel level)
    {
        var v = new Vector2(0.1f, 0.3f);
        float multiplier = 1;
        switch (level)
        {
            case CargoLevel.LowCargo:
                multiplier = 0.6f;
                break;
            case CargoLevel.MediumCargo:
                multiplier = 1;
                break;
            case CargoLevel.HighCargo:
                multiplier = 2;
                break;
            default:
                multiplier = 1;
                break;
        }
        return v * multiplier;
    }

    private static Vector2 CargoProductionRange(CargoLevel level)
    {
        var v = new Vector2(0.01f, 0.06f);
        float multiplier = 1;
        switch (level)
        {
            case CargoLevel.LowCargo:
                multiplier = 0.75f;
                break;
            case CargoLevel.MediumCargo:
                multiplier = 1;
                break;
            case CargoLevel.HighCargo:
                multiplier = 2;
                break;
            default:
                multiplier = 1;
                break;
        }
        return v * multiplier;
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


}

