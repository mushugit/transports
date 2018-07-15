using System.Collections.Generic;

public interface ICargoProvider : IFluxReferencer, ILinkable
{
    bool ProvideCargo(int quantity);

    Dictionary<ICargoAccepter, Flux> OutgoingFlux { get; }

    void UpdateAllOutgoingFlux();
}

