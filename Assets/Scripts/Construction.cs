using Newtonsoft.Json;

public abstract class Construction : IHasCell
{
	[JsonProperty]
	public Cell Coord { get; protected set; }

    protected Construction() : this(0, 0) { }

    protected Construction(int x, int y)
    {
        Coord = new Cell(x, y,this);
    }

    protected Construction(Cell cell)
    {
        Coord = cell;
    }

	public abstract void Destroy();
}
