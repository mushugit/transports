using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public interface ICargoGenerator
{
    Vector2 CargoChanceRange { get; }
    Vector2 CargoProductionRange { get; }

    [JsonProperty]
    float CargoChance { get; }
    [JsonProperty]
    float CargoProduction { get; }
    [JsonProperty]
    float ExactCargo { get; }

    int Cargo { get; }

    Dictionary<Construction, Flux> OutgoingFlux { get; }

    void ReferenceFlux(Flux flux);

    void UpdateCargo();

    bool DistributeCargo(int quantity);

    void RemoveFlux(Flux f);

    void GenerateCargo();
}

