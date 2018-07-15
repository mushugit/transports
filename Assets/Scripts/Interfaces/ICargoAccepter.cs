using System.Collections.Generic;

public interface ICargoAccepter : IFluxReferencer, ILinkable
{
    bool DistributeCargo(int quantity);

    Dictionary<ICargoProvider, Flux> IncomingFlux { get; }
}

