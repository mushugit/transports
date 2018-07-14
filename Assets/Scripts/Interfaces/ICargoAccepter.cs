using System.Collections.Generic;

public interface ICargoAccepter : IFluxReferencer
{
    bool DistributeCargo(int quantity);

    Dictionary<ICargoProvider, Flux> IncomingFlux { get; }
}

