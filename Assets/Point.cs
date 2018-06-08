using System;
using UnityEngine;


public class Point
{
    public int X { get; }
    public int Y { get; }

    public Point(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public int ManhattanDistance(Point c)
    {
        int d = Mathf.Abs(c.X - X) + Mathf.Abs(c.Y - Y) - 1;
        //Debug.Log("ManhattanDistance entre " + ToString() + " et " + c.ToString() + " = " + d);
        return d;
    }

    public float Distance(Point c)
    {
        return Mathf.Sqrt((this.X - X) * (this.X - X) + (this.Y - Y) * (this.Y - Y));
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;
        var n = (Point)obj;
        return X == n.X && Y == n.Y;
    }

    public override int GetHashCode()
    {
        return X.GetHashCode() ^ Y.GetHashCode();
    }

    /*public static bool operator ==(Point a, Point b) {
		if(a==null || b==null)
			return false;
		return a.x==b.x && a.y==b.y;
	}
	
	public static bool operator !=(Point a, Point b) {
		return !(a == b);
   	}
	*/
    public override string ToString()
    {
        return string.Format("[{0},{1}]", X, Y);
    }

}

