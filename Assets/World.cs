using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class World : MonoBehaviour {
	public Component cellPrefab;
	public Component cityPrefab;
	public Component[] roadPrefabs;

	public float width = 10f;
	public float height = 10f;

	public Construction[,] constructions {get; private set;}
	public Component[,] terrain {get; private set;}
	private List<City> cities;

	void Update() {
		if (Input.GetButton ("Reload"))
			SceneManager.LoadScene( SceneManager.GetActiveScene ().name);
	}

	void Start () {
		Generate ();
		StartCoroutine ("Simulation");
	}

	void Generate() {
		var w = (int)width;
		var h = (int)height;
		constructions = new Construction [w,h]; 
		terrain = new Component [w,h]; 

		Cities (w, h);
		//Roads(w,h);

		for (float x = 0f; x < width; x++) {
			for (float y = 0f; y < height; y++) {
				terrain[(int)x,(int)y] = Terrain (x, y);
			}
		}	
		StartCoroutine("SearchPath");
	}
	

	IEnumerator Simulation() {
		while (true) {
			foreach (City c in cities) {
				c.GenerateCargo ();
			}
			yield return new WaitForSeconds (0.02f);
		}
	}

	Component Terrain(float x, float y) {
		return Instantiate(cellPrefab,new Vector3 (x,-0.05f,y),Quaternion.identity);
	}

	void Cities(int w, int h){
		var quantity = City.Quantity(w,h);
		cities = new List<City> (quantity);
		var n = 0;
		while(n<quantity){
			int x = Random.Range (0, w);
			int y = Random.Range (0, h);
			if (constructions [x, y] == null) {
				var c = new City (new Point(x,y),cityPrefab);
				constructions [x, y] = c;
				cities.Add (c);
				n++;
			}
		}	
	}
	
	void Roads(int w, int h){
		var quantity = 10;
		var n = 0;
		while(n<quantity){
			int x = Random.Range (0, w);
			int y = Random.Range (0, h);
			if (constructions [x, y] == null) {
				Road r = new Road (x,y, roadPrefabs);
				constructions [x, y] = r;
				n++;
			}
		}	
	}
	
	List<Point> TraceBackPath(Point[,] cameFrom, Node current){
		var path = new List<Point>();
		var p = current.p;
		path.Add(p);
		while(cameFrom[p.x,p.y]!=null){
			p = cameFrom[p.x,p.y];
			path.Add(p);
		}
		return path;
	}
	
	IEnumerator SearchPath(){ //Point start, Point target
		Debug.Log("Path between " + cities.First().name + " and " + cities.Last().name);
		var startNode = new Node(this,cities.First().p,0f,0);
		var targetNode = new Node(this,cities.Last().p,0f, cities.Last().p.ManhattanDistance(cities.First().p));
		var closed = new List<Node>();
		var opened = new List<Node>();
		opened.Add(startNode);
		var cameFrom = new Point[(int)width,(int)height];
		
		while(opened.Count > 0){
			/*Debug.Log("Opened=");
			opened.ForEach(o=>Debug.Log(o.p.ToString()));
			Debug.Log("Closed=");
			closed.ForEach(o=>Debug.Log(o.p.ToString()));*/
			
			opened.ForEach(o=>terrain[o.p.x,o.p.y].SendMessage("makeBlue"));
			closed.ForEach(o=>terrain[o.p.x,o.p.y].SendMessage("makeRed"));;
			
			
			//opened.OrderBy(x => x.h).ToList().ForEach(o=>Debug.Log(o.p.ToString()));
			opened.Sort();
			Node n = opened.First();
			if(n.p.ManhattanDistance(cities.Last().p)<=0){
				cameFrom[cities.Last().p.x,cities.Last().p.y] = n.p;
				terrain[n.p.x,n.p.y].SendMessage("makeRed");
				var path = TraceBackPath(cameFrom, n);
				Debug.Log("Found !");
				foreach(Point p in path){
					Debug.Log(p.ToString());
					
				}
				yield break;
			}
			
			opened.Remove(n);
			closed.Add(n);
			
			var neighbors =  n.Neighbors();
			foreach(Node neighbor in neighbors){
				if(closed.Contains(neighbor))
					continue;
				opened.Add(neighbor);
				float tentative = n.c + n.p.ManhattanDistance(neighbor.p);
				if(tentative >= neighbor.c)
					continue;
				cameFrom[neighbor.p.x,neighbor.p.y] = n.p;
				neighbor.score(tentative);
				neighbor.distance( (int)Mathf.Round(neighbor.c)+neighbor.p.ManhattanDistance(cities.Last().p));
			}
			yield return new WaitForSeconds(1f);
		}
		
		Debug.Log("No path found !");
		yield return null;		
	}
	
	
}
