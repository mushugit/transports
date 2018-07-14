using System.Collections.Generic;

public interface ICargoProvider : IFluxReferencer
{
    bool ProvideCargo(int quantity);

    Dictionary<ICargoAccepter, Flux> OutgoingFlux { get; }
}

