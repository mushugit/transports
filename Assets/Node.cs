using System;
using System.Collections;
using System.Collections.Generic;

public class Node : IComparable<Node>
{
	public Point p {get;}
	public float c {get; private set;}
	public int h {get; private set;}
	
	public World w {get;}
	
	public Node(World world, Point position, float cost, int heuristic)
	{
		p = position;
		c=cost;
		h=heuristic;
		w = world;
	}
	
	public void score(float s){
		c=s;
	}
	
	public void distance(int d){
		h=d;
	}
	
	public int compare(Node n){
		if(h<n.h) return 1;
		if(h==n.h) return 0;
		return -1;
	}
	
	public List<Node> Neighbors(){
		var neighbors = new List<Node>();
		//left
		if(p.x > 0)
			if(w.constructions[p.x-1,p.y] == null)
				neighbors.Add(new Node(w,new Point(p.x-1,p.y),float.MaxValue,0));
		//right
		if(p.x < w.width-1)
			if(w.constructions[p.x+1,p.y] == null)
				neighbors.Add(new Node(w,new Point(p.x+1,p.y),float.MaxValue,0));
		//up
		if(p.y < w.height-1)
			if(w.constructions[p.x,p.y+1] == null)
				neighbors.Add(new Node(w,new Point(p.x,p.y+1),float.MaxValue,0));
		//down
		if(p.y > 0)
			if(w.constructions[p.x,p.y-1] == null)
				neighbors.Add(new Node(w,new Point(p.x,p.y-1),float.MaxValue,0));
		
		return neighbors;
	}
	
	public override bool Equals(object obj){
		if (obj == null || GetType() != obj.GetType()) 
			return false;
		var n = (Node) obj;
		if(n.p == null)
			return false;
		return p.x==n.p.x && p.y==n.p.y;
	}
	
	public override int GetHashCode() {
		return p.GetHashCode() ^ c.GetHashCode() ^ h.GetHashCode();
	}
	
    public int CompareTo(Node n)
    {
        if (n == null)
            return 1;
        
        return this.h.CompareTo(n.h);
    }
	
	/*
	public static bool operator ==(Node a, Node b) {
		if(a.p==null||b.p==null)
			return false;
		
		return a.p.x==b.p.x && a.p.y==b.p.y;
	}
	
	public static bool operator !=(Node a, Node b) {
		return !(a == b);
   	}*/
}

