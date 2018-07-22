using System.Collections.Generic;

public interface ICargoProvider : IFluxReferencer, ILinkable
{
    int PeekCargo();

    bool ProvideCargo(int quantity);

    Dictionary<ICargoAccepter, Flux> OutgoingFlux { get; }

    void UpdateAllOutgoingFlux();
}

