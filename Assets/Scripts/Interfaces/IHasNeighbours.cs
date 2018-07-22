using System;
using System.Collections.Generic;

public interface IHasNeighbours<N>
{
	IEnumerable<N> Neighbours(List<Type> passable);
}
