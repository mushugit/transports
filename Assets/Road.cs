using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road : Construction {

	Component roadRender;
	new int x;
	new int y;
	
	public Road(int x,int y, Component[] roadPrefabs){
		this.x = x;
		this.y = y;
		
		roadRender = RoadRender.Build (new Vector3(x+0.5f,0f,y+0.5f), roadPrefabs);	
	}
}
