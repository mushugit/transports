using Newtonsoft.Json;

public abstract class Construction
{
	[JsonProperty]
	public Coord Point { get; protected set; }

    protected Construction() : this(0, 0) { }

    protected Construction(int x, int y)
    {
        Point = new Coord(x, y);
    }

    protected Construction(Coord point)
    {
        Point = Point;
    }

	public abstract void Destroy();
}
