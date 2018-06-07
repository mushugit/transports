using UnityEngine;

public abstract class Construction {
	public int x {get;}
	public int y {get;}

	protected Construction() : this(0,0) {}

	protected Construction(int x, int y){
		this.x=x;
		this.y=y;
	}
}
