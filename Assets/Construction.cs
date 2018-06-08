using UnityEngine;

public abstract class Construction
{
    public int X { get; }
    public int Y { get; }

    protected Construction() : this(0, 0) { }

    protected Construction(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }
}
