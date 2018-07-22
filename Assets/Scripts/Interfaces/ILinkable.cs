using System.Collections;
using System.Collections.Generic;

public interface ILinkable : IHasCell, IHasRelativeDistance
{
    List<ILinkable> Linked { get; }
    List<ILinkable> Unreachable { get; }

    void ClearLinks();
    void AddUnreachable(ILinkable c);
    void AddUnreachable(List<ILinkable> list);
    void AddLinkTo(ILinkable c);
    void AddLinkTo(List<ILinkable> list);

    bool IsUnreachable(ILinkable c);
    bool IsLinkedTo(ILinkable c);

    int RoadInDirection(Cell c);
}

