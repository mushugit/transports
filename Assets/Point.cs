using System;
using UnityEngine;


public class Point
{
	public int x {get;}
	public int y {get;}

	public Point(int x, int y)
	{
		this.x = x;
		this.y = y;
	}
	
	public int ManhattanDistance(Point c){
		int d = Mathf.Abs (c.x - x) + Mathf.Abs (c.y - y) - 1;
		//Debug.Log("ManhattanDistance entre " + ToString() + " et " + c.ToString() + " = " + d);
		return d;
	}

	public float Distance(Point c){
		return Mathf.Sqrt ((this.x - x) * (this.x - x) + (this.y - y) * (this.y - y));
	}
	
	public override bool Equals(object obj){
		if (obj == null || GetType() != obj.GetType()) 
			return false;
		var n = (Point) obj;
		return x==n.x && y==n.y;
	}
	
	public override int GetHashCode() {
		return x.GetHashCode() ^ y.GetHashCode();
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
		return string.Format("[{0},{1}]", x, y);
	}
 
}

