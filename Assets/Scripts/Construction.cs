using Newtonsoft.Json;

public abstract class Construction
{
	[JsonProperty]
	public Cell Point { get; protected set; }

    protected Construction() : this(0, 0) { }

    protected Construction(int x, int y)
    {
        Point = new Cell(x, y,this);
    }

    protected Construction(Cell point)
    {
        Point = Point;
    }

	public abstract void Destroy();
}
