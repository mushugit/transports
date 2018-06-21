using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class World : MonoBehaviour
{
	public static bool gameLoading = true;
	public static float progressLoading = 0f;
	public static string itemLoading = "Niveau en préparation";
	public static float totalLoading = 1f;

	public double searchSpeed = 200d;

	public Component cellPrefab;
	public Component cityPrefab;
	public Component roadPrefab;

	public static float width = 25f;
	public static float height = width;

	public int minCityDistance = 4;


	public Construction[,] Constructions { get; private set; }
	public CellRender[,] Terrains { get; private set; }
	private List<City> cities;

	public readonly static Vector3 Center = new Vector3(width / 2f, 0f, height / 2f);

	void ReloadLevel()
	{
		//Screenshot.Take(400, 400);
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	void Update()
	{
		if (gameLoading)
			return;

		if (Input.GetButton("Reload"))
			ReloadLevel();
	}

	void InitLoader()
	{
		float nbCells = width * height;
		float nbLinks = City.Quantity((int)width, (int)height);
		totalLoading = nbCells + nbLinks;
	}

	void Start()
	{
		Application.targetFrameRate = 60;

		InitLoader();
		StartCoroutine(Generate());
	}

	IEnumerator Generate()
	{
		var w = (int)width;
		var h = (int)height;

		Constructions = new Construction[w, h];
		Terrains = new CellRender[w, h];

		int countCells = 0;
		itemLoading = "Chargement du terrain";
		for (float x = 0f; x < width; x++)
		{
			for (float y = 0f; y < height; y++)
			{
				Terrains[(int)x, (int)y] = Terrain(x, y);
				countCells++;
				progressLoading++;
				if (countCells % 10000 == 0)
					yield return null;
			}
		}

		itemLoading = "Chargement des villes";
		Cities(w, h);
		foreach (City c in cities)
		{
			var closestCity = ClosestCityUnlinked(c);
			if (closestCity != null)
			{
				yield return StartCoroutine(Link(c, closestCity));
			}
			progressLoading++;
		}

		itemLoading = "Chargement terminé";
		gameLoading = false;

		//yield return StartCoroutine(Simulation());
		//yield return new WaitForSeconds(1f);
		//ReloadLevel();
	}

	IEnumerator Link(City a, City b)
	{
		if (!a.IsLinkedTo(b))
		{
			if (a.LinkedCities == null)
			{
				var c = a;
				a = b;
				b = c;
			}

			itemLoading = "Relie " + a.Name + " à " + b.Name;
			//Debug
			//var parameters = new SearchParameter(a.Point, b.Point, 1f, 3f, false, null, false, true);
			var parameters = new SearchParameter(a.Point, b.Point, 0, 0, false, null);
			yield return StartCoroutine(SearchPath(parameters));
			//Debug.Log($"Path={parameters.Path}");
			if (parameters.Path != null && parameters.Path.Count > 1)
			{
				//Debug.Log("Path from " + a.Name + " to " + b.Name);
				yield return StartCoroutine(Roads(parameters.Path));
				yield return StartCoroutine(UpdateLink(a, b));
			}
			else
			{
				//Debug.Log("No path from " + a.Name + " to " + b.Name);
			}
		}
	}

	IEnumerator UpdateLink(City a, City b)
	{
		var allALink = new List<City>(a.LinkedCities) { a };
		var allBLink = new List<City>(b.LinkedCities) { b };

		foreach (City c in a.LinkedCities)
			c.AddLinkTo(allBLink);
		a.AddLinkTo(allBLink);
		foreach (City c in b.LinkedCities)
			c.AddLinkTo(allALink);
		b.AddLinkTo(allALink);
		yield return null;
	}

	bool AllCitiesLinkedToEachOther()
	{
		foreach (City a in cities)
		{
			if (a.LinkedCities.Count != cities.Count)
				return false;
		}
		return true;
	}

	IEnumerator Simulation()
	{
		while (true)
		{
			foreach (City c in cities)
			{
				c.GenerateCargo();
			}
			yield return new WaitForSeconds(0.02f);
		}
	}

	CellRender Terrain(float x, float y)
	{
		var t= Instantiate(cellPrefab, new Vector3(x, 0f, y), Quaternion.identity);

		var render = t.GetComponentInChildren<CellRender>();
		render.Initialize(new Coord((int)x, (int)y), null);

		return render;
	}

	void Cities(int w, int h)
	{
		var quantity = City.Quantity(w, h);
		cities = new List<City>(quantity);
		var n = 0;
		while (n < quantity)
		{
			int x = Random.Range(0, w);
			int y = Random.Range(0, h);
			var cityCenter = new Coord(x, y);
			if (Constructions[x, y] == null)
			{
				var canBuild = true;
				foreach (City otherCity in cities)
				{
					if (otherCity.ManhattanDistance(cityCenter) < minCityDistance)
					{
						canBuild = false;
						break;
					}
				}
				if (canBuild)
				{
					var c = new City(cityCenter, cityPrefab);
					Constructions[x, y] = c;
					Terrains[x, y].Build(c);
					cities.Add(c);
					n++;
					//Debug.Log($"City {n} at {c.Point}");
				}
			}
		}
	}

	void UpdateRoad(Road r)
	{
		Construction north = North(r);
		bool northLink = north != null && (north is Road || north is City);
		Construction east = East(r);
		bool eastLink = east != null && (east is Road || east is City);
		Construction south = South(r);
		bool southLink = south != null && (south is Road || south is City);
		Construction west = West(r);
		bool westLink = west != null && (west is Road || west is City);

		r.UpdateConnexions(northLink, eastLink, southLink, westLink);
	}

	IEnumerator Roads(List<Coord> path)
	{
		//Debug.Log("Road from " + path.First() + " to " + path.Last());
		var countLoop = 0d;
		var constructedRoads = new List<Road>();
		foreach (Coord p in path)
		{
			countLoop++;
			if (Constructions[p.X, p.Y] == null)
			{
				Road r = new Road(p, roadPrefab);
				constructedRoads.Add(r);
				Constructions[p.X, p.Y] = r;
				Terrains[p.X, p.Y].Build(r);
			}
			if (countLoop % searchSpeed == 0)
				yield return null;
		}

		var neighborsRoads = new List<Road>();
		foreach (Road r in constructedRoads)
		{
			countLoop++;

			neighborsRoads.Remove(r);
			foreach (Road neighborsRoad in Neighbors(r))
			{
				if (!neighborsRoads.Contains(neighborsRoad))
					neighborsRoads.Add(neighborsRoad);
			}

			UpdateRoad(r);
			foreach (Road neighborsRoad in neighborsRoads)
			{
				UpdateRoad(neighborsRoad);
			}

			if (countLoop % searchSpeed == 0)
				yield return null;
		}
		yield return null;
	}

	public int CountNeighbors(Construction c)
	{
		int count = 0;

		//left
		if (c.Point.X > 0)
			if (Constructions[c.Point.X - 1, c.Point.Y] != null)
				count++;
		//right
		if (c.Point.X < width - 1)
			if (Constructions[c.Point.X + 1, c.Point.Y] != null)
				count++;
		//up
		if (c.Point.Y < height - 1)
			if (Constructions[c.Point.X, c.Point.Y + 1] != null)
				count++;
		//down
		if (c.Point.Y > 0)
			if (Constructions[c.Point.X, c.Point.Y - 1] != null)
				count++;

		return count;
	}

	public Construction West(Construction c)
	{
		if (c.Point.X > 0)
			return Constructions[c.Point.X - 1, c.Point.Y];
		return null;
	}

	public Construction East(Construction c)
	{
		if (c.Point.X < width - 1)
			return Constructions[c.Point.X + 1, c.Point.Y];
		return null;
	}

	public Construction North(Construction c)
	{
		if (c.Point.Y < height - 1)
			return Constructions[c.Point.X, c.Point.Y + 1];
		return null;
	}

	public Construction South(Construction c)
	{
		if (c.Point.Y > 0)
			return Constructions[c.Point.X, c.Point.Y - 1];
		return null;
	}


	public List<Road> Neighbors(Road r)
	{
		var neighbors = new List<Road>();
		var directions = r.Point.Directions();

		foreach (Coord p in directions)
		{
			if (p != null)
			{
				var c = Constructions[p.X, p.Y];
				if (c != null && c is Road)
					neighbors.Add(c as Road);
			}
		}

		return neighbors;
	}

	public List<Road> ExtendedNeighbors(Road r)
	{
		var neighbors = new List<Road>();
		var directions = r.Point.ExtendedDirections();

		foreach (Coord p in directions)
		{
			var c = Constructions[p.X, p.Y];
			if (c != null && c is Road)
				neighbors.Add(c as Road);
		}

		return neighbors;
	}

	List<Coord> TraceBackPath(Coord[,] cameFrom, Coord current)
	{
		var path = new List<Coord>();
		var p = current;
		//Debug.Log($"Je suis à {p} et je venais de {cameFrom[p.X, p.Y]}");
		path.Add(p);
		while (cameFrom[p.X, p.Y] != null)
		{
			p = cameFrom[p.X, p.Y];
			//Debug.Log($"Je suis à {p} et je venais de {cameFrom[p.X, p.Y]}");
			path.Add(p);
		}
		return path;
	}

	internal class SearchParameter
	{
		public Coord Start { get; }
		public Coord Target { get; }
		public List<Coord> Path { get; set; }
		public float Speed { get; }
		public float WaitAtTheEnd { get; }
		public bool AvoidCities { get; }
		public bool OnlyRoads { get; }
		public bool VisualSearch { get; }

		public SearchParameter(Coord start, Coord target, float speed, float waitAtTheEnd, bool avoidCities, List<Coord> path, bool onlyRoads = false, bool visualSearch = false)
		{
			Start = start;
			Target = target;
			Speed = speed;
			Path = path;
			WaitAtTheEnd = waitAtTheEnd;
			AvoidCities = avoidCities;
			OnlyRoads = onlyRoads;
			VisualSearch = visualSearch;
		}
	}

	void CleanGrassColor(List<Node> opened, List<Node> closed)
	{
		foreach (Node n in opened)
		{
			Terrains[n.Point.X, n.Point.Y].SendMessage("Revert");
		}
		foreach (Node n in closed)
		{
			Terrains[n.Point.X, n.Point.Y].SendMessage("Revert");
		}

	}

	IEnumerator SearchPath(SearchParameter parameters)
	{
		//Debug.Log("Search path from " + parameters.Start + " to " + parameters.Target);

		var start = parameters.Start;
		var target = parameters.Target;

		var startNode = new Node(this, start, 0f, start.ManhattanDistance(target));


		var closed = new List<Node>();
		var opened = new List<Node>
		{
			startNode
		};
		var cameFrom = new Coord[(int)width, (int)height];

		double countLoop = 0;
		//Debug.Log("===========START SEARCH=================");
		while (opened.Count > 0)
		{
			countLoop++;
			if (parameters.VisualSearch)
			{
				opened.ForEach(o => Terrains[o.Point.X, o.Point.Y].SendMessage("MakeBlue"));
				closed.ForEach(o => Terrains[o.Point.X, o.Point.Y].SendMessage("MakeRed")); ;
			}

			opened.Sort();
			Node n = opened.First();
			//Debug.Log($"Chosen = {n.Point} H={n.Heuristic} c={n.Cost}");
			if (n.Point.ManhattanDistance(target) <= 1)
			{
				// Target found
				//Debug.Log("Found path !");
				// Add last point
				cameFrom[target.X, target.Y] = n.Point;

				if (parameters.VisualSearch)
					Terrains[n.Point.X, n.Point.Y].SendMessage("MakeRed");

				// Rebuild path
				parameters.Path = TraceBackPath(cameFrom, target);

				if (parameters.WaitAtTheEnd > 0f)
					yield return new WaitForSeconds(parameters.WaitAtTheEnd);
				if (parameters.VisualSearch)
					CleanGrassColor(opened, closed);

				yield break;
			}

			opened.Remove(n);
			closed.Add(n);

			var neighbors = n.Neighbors(parameters.AvoidCities, parameters.OnlyRoads, target);
			foreach (Node neighbor in neighbors)
			{
				//Debug.Log($"Neighbor of ({n}) is ({neighbor})");
				if (closed.Contains(neighbor))
				{
					//Debug.Log("Déjà testé");
					continue;
				}

				if (!opened.Contains(neighbor))
					opened.Add(neighbor);
				float tentativeCost = n.Cost + TrueDistance(n.Point, neighbor.Point);
				//Debug.Log($"Tentative={tentativeCost}");
				if (tentativeCost >= neighbor.Cost)
					continue;

				//Debug.Log($"J'ajoute de {n.Point} à {neighbor.Point}");
				cameFrom[neighbor.Point.X, neighbor.Point.Y] = n.Point;

				neighbor.Score(tentativeCost);
				neighbor.Distance(tentativeCost + Heurisic(neighbor.Point, target));
			}
			if (parameters.Speed > 0f)
				yield return new WaitForSeconds(parameters.Speed);
			else
			{
				if (countLoop % searchSpeed == 0)
					yield return null;
			}
		}

		if (parameters.VisualSearch)
			CleanGrassColor(opened, closed);
		yield return null;
	}

	float TrueDistance(Coord origin, Coord target)
	{
		var constructionOrigin = Constructions[origin.X, origin.Y];
		var constructionTarget = Constructions[target.X, target.Y];

		var dividerCost = 1f;

		if (constructionOrigin is Road || constructionOrigin is City)
			dividerCost *= 2f;
		if (constructionTarget is Road || constructionTarget is City)
			dividerCost *= 2f;

		return ((float)origin.ManhattanDistance(target)) / dividerCost;
	}

	float Heurisic(Coord origin, Coord target)
	{
		return origin.ManhattanDistance(target);
		//return origin.Distance(target);
	}

	City ClosestCityUnlinked(City c)
	{
		var minDistance = int.MaxValue;
		City closestCity = null;
		foreach (City otherCity in cities)
		{
			if (otherCity != c && !c.LinkedCities.Contains(otherCity))
			{
				var d = c.ManhattanDistance(otherCity);
				if (d < minDistance)
				{
					minDistance = d;
					closestCity = otherCity;
				}

			}
		}

		/*//Debug
		if (closestCity != null)
			Debug.Log("[WORLD] " + c.Name + " est proche et non lié de " + closestCity.Name);
		else
			Debug.Log("[WORLD] " + c.Name + " est lié à tout");
		*/

		return closestCity;
	}

	City ClosestCity(City c)
	{
		var minDistance = int.MaxValue;
		City closestCity = null;
		foreach (City otherCity in cities)
		{
			if (otherCity != c)
			{
				var d = c.ManhattanDistance(otherCity);
				if (d < minDistance)
				{
					minDistance = d;
					closestCity = otherCity;
				}
			}
		}
		return closestCity;
	}

	City FarestCity(City c)
	{
		var maxDistance = 0;
		City farestCity = null;
		foreach (City otherCity in cities)
		{
			if (otherCity != c)
			{
				var d = c.ManhattanDistance(otherCity);
				if (d >= maxDistance)
				{
					maxDistance = d;
					farestCity = otherCity;
				}
			}
		}
		return farestCity;
	}


}
