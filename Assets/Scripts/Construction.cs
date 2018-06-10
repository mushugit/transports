using UnityEngine;

public abstract class Construction
{
    public Point Point { get; protected set; }

    protected Construction() : this(0, 0) { }

    protected Construction(int x, int y)
    {
        Point = new Point(x, y);
    }

    protected Construction(Point point)
    {
        Point = Point;
    }
}
