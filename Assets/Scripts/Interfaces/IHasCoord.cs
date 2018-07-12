public interface IHasCoord
{
	int X { get; }
	int Y { get; }

    int ManathanDistance(IHasCoord other);
    float FlyDistance(IHasCoord other);
}

