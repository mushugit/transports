using System;
using System.Collections.Generic;

interface IHasNeighbours<N>
{
	IEnumerable<N> Neighbours(List<Type> passable);
}
