using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class World : MonoBehaviour
{
	public static WorldSave loadData = null;

	public static bool gameLoading = true;
	public static float progressLoading = 0f;
	public static string itemLoading = "Niveau en préparation";
	public static float totalLoading = 1f;

	public double searchSpeed = 200d;

	public Component terrainPrefab;
	public Component cityPrefab;
	public Component roadPrefab;
	public Component depotPrefab;

	public Component uiCanvas;

	public static float width = 50;
	public static float height = width;

	public int minCityDistance = 4;
	public static Economy LocalEconomy { get; private set; }

	public static readonly int worldLoadSceneIndex = 1;
	public static readonly int looseSceneIndex = 3;


	public Construction[,] Constructions { get; private set; }
	public List<City> Cities;

	public readonly static Vector3 Center = new Vector3(width / 2f, 0f, height / 2f);

	public static World Instance { get; private set; }

	public static void ReloadLevel()
	{
		PauseMenu.ForceResume();
		SceneManager.LoadScene(worldLoadSceneIndex);
	}

	public static void Loose()
	{
		SceneManager.LoadScene(looseSceneIndex);
	}

	public static void CleanLoader()
	{
		var loader = Instance.GetComponentInParent<LevelLoader>();
		if (loader != null)
			DestroyImmediate(loader.gameObject);
	}

	void Update()
	{
		if (gameLoading)
			return;

		if (LocalEconomy.Balance < LocalEconomy.GetGain("loose"))
			Loose();

		if (Input.GetButton("Reload"))
			ReloadLevel();
	}

	void InitLoader()
	{
		float nbLinks = City.Quantity((int)width, (int)height);
		totalLoading = 1 + nbLinks;
	}

	void InitLoader(int forcedLoadCount)
	{
		totalLoading = 1 + forcedLoadCount;
	}

	private void Awake()
	{
		LocalEconomy = new Economy(EconomyTemplate.Difficulty.Free);
		Constructions = new Construction[(int)width, (int)height];
		Instance = this;

	}

	void Start()
	{
		Application.targetFrameRate = 60;

		if (loadData == null)
		{
			InitLoader();
			StartCoroutine(Generate());
		}
		else
		{
			width = loadData.Width;
			height = loadData.Height;
			InitLoader(loadData.Constructions.Count + loadData.AllFlux.Count);
			StartCoroutine(Load());
		}
	}

	IEnumerator Load()
	{
		var w = (int)width;
		var h = (int)height;

		Constructions = new Construction[w, h];

		itemLoading = "Chargement du terrain";
		Terrain(width, height);
		progressLoading++;

		itemLoading = "Chargement des constructions";
		Cities = new List<City>();
		var roads = new List<Coord>();
		foreach (Construction c in loadData.Constructions)
		{
			if (c is City)
			{
				BuildCity(c as City);
			}
			if (c is Depot)
			{
				var d = c as Depot;
				BuildDepot(d.Point, d.Direction);
			}
			if(c is Road)
			{
				roads.Add(c.Point);
			}
			progressLoading++;
		}
		yield return StartCoroutine(BuildRoads(roads));

		itemLoading = "Chargement des flux";
		foreach(Flux f in loadData.AllFlux)
		{
			Simulation.AddFlux(f);
		}
		progressLoading++;

		itemLoading = "Chargement terminé";
		gameLoading = false;

		LocalEconomy = new Economy(EconomyTemplate.Difficulty.Normal);
		ActivateUI();
		CleanLoader();
		yield return StartCoroutine(Simulation.Run());
	}

	private void ActivateUI()
	{
		PauseMenu.ForceResume();
		gameLoading = false;

		var buttons = uiCanvas.GetComponentsInChildren<Button>();
		foreach (Button b in buttons)
		{
			b.interactable = true;
		}
	}

	IEnumerator Generate()
	{
		var w = (int)width;
		var h = (int)height;

		Constructions = new Construction[w, h];

		itemLoading = "Chargement du terrain";
		Terrain(width, height);
		progressLoading++;

		itemLoading = "Chargement des villes";
		GenerateCity(w, h);
		foreach (City c in Cities)
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

		LocalEconomy = new Economy(EconomyTemplate.Difficulty.Normal);
		ActivateUI();
		CleanLoader();
		yield return StartCoroutine(Simulation.Run());
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
				yield return StartCoroutine(BuildRoads(parameters.Path));
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
		foreach (City a in Cities)
		{
			if (a.LinkedCities.Count != Cities.Count)
				return false;
		}
		return true;
	}

	void Terrain(float width, float height)
	{
		var t = Instantiate(terrainPrefab, Vector3.zero, Quaternion.identity);
		t.transform.localScale = new Vector3(width, 1f, height);
		var c = t.GetComponentInChildren<CellRender>();
		c?.SetScale(width, height);
	}

	public void DestroyConstruction(Coord p)
	{
		var c = Constructions[p.X, p.Y];
		if (c != null)
		{
			if (c is Road)
				DestroyRoad(p);
			if (c is City)
				DestroyCity(p);
			if (c is Depot)
				DestroyDepot(p);
		}

		var neighborsRoads = new List<Road>();
		foreach (Road neighborsRoad in Neighbors(p))
		{
			if (!neighborsRoads.Contains(neighborsRoad))
				neighborsRoads.Add(neighborsRoad);
		}

		foreach (Road neighborsRoad in neighborsRoads)
		{
			UpdateRoad(neighborsRoad);
		}
	}

	void GenerateCity(int w, int h)
	{
		var quantity = City.Quantity(w, h);
		Cities = new List<City>(quantity);
		var n = 0;
		while (n < quantity)
		{
			int x = UnityEngine.Random.Range(0, w);
			int y = UnityEngine.Random.Range(0, h);
			var cityCenter = new Coord(x, y);
			if (Constructions[x, y] == null)
			{
				var canBuild = true;
				foreach (City otherCity in Cities)
				{
					if (otherCity.ManhattanDistance(cityCenter) < minCityDistance)
					{
						canBuild = false;
						break;
					}
				}
				if (canBuild)
				{
					BuildCity(cityCenter);
					n++;
				}
			}
		}
	}

	public static bool CheckCost(string operationName,string messageOperation, out int cost)
	{
		if (!LocalEconomy.DoCost(operationName, out cost))
		{
			Message.ShowError("Pas assez d'argent",
				$"Vous ne pouvez pas {messageOperation} celà coûte {cost} et vous avez {LocalEconomy.Balance}");
			return false;
		}
		else
			return true;
	}

	private void _BuildDepot(Coord pos, int direction)
	{
		int cost;
		if (!CheckCost("build_depot","construire un dépôt",out cost))
			return;

		AudioManager.Player.Play("buildCity");
		var c = new Depot(pos, depotPrefab, direction);

		Constructions[pos.X, pos.Y] = c;

		var neighborsRoads = new List<Road>();
		foreach (Road neighborsRoad in Neighbors(pos))
		{
			if (!neighborsRoads.Contains(neighborsRoad))
				neighborsRoads.Add(neighborsRoad);
		}

		foreach (Road neighborsRoad in neighborsRoads)
		{
			UpdateRoad(neighborsRoad);
		}

	}

	public void BuildDepot(Coord pos, int direction)
	{
		_BuildDepot(pos, direction);
	}

	public void BuildDepot(Coord pos)
	{
		_BuildDepot(pos, Builder.RotationDirection);
	}

	public void BuildCity(City dummyCity)
	{
		int cost;
		if (!CheckCost("build_city", "bâtir une nouvelle ville", out cost))
			return;

		var c = new City(dummyCity, cityPrefab);
		var cityCenter = dummyCity.Point;

		Constructions[cityCenter.X, cityCenter.Y] = c;
		Cities.Add(c);

		var neighborsRoads = new List<Road>();
		foreach (Road neighborsRoad in Neighbors(cityCenter))
		{
			if (!neighborsRoads.Contains(neighborsRoad))
				neighborsRoads.Add(neighborsRoad);
		}

		foreach (Road neighborsRoad in neighborsRoads)
		{
			UpdateRoad(neighborsRoad);
		}
	}

	public void BuildCity(Coord cityCenter)
	{
		int cost;
		if (!CheckCost("build_city", "bâtir une nouvelle ville", out cost))
			return;

		AudioManager.Player.Play("buildCity");
		var c = new City(cityCenter, cityPrefab);

		Constructions[cityCenter.X, cityCenter.Y] = c;
		Cities.Add(c);

		var neighborsRoads = new List<Road>();
		foreach (Road neighborsRoad in Neighbors(cityCenter))
		{
			if (!neighborsRoads.Contains(neighborsRoad))
				neighborsRoads.Add(neighborsRoad);
		}

		foreach (Road neighborsRoad in neighborsRoads)
		{
			UpdateRoad(neighborsRoad);
		}
	}

	public void DestroyCity(Coord cityCenter)
	{
		int cost;
		if (!CheckCost("destroy_city", "raser une ville", out cost))
			return;

		var c = Constructions[cityCenter.X, cityCenter.Y] as City;
		Cities.Remove(c);
		c.Destroy();
		Constructions[cityCenter.X, cityCenter.Y] = null;
	}

	public void DestroyDepot(Coord pos)
	{
		int cost;
		if (!CheckCost("destroy_depot", "détruire un dépôt", out cost))
			return;

		var c = Constructions[pos.X, pos.Y] as City;
		c.Destroy();
		Constructions[pos.X, pos.Y] = null;
	}

	public void UpdateRoad(Road r)
	{
		r.UpdateConnexions(IsLinkable(2, North(r)), IsLinkable(3, East(r)), IsLinkable(0, South(r)), IsLinkable(1, West(r)));
	}

	public bool IsLinkable(int direction, Construction c)
	{
		if (c == null)
			return false;

		if (c is Construction)
		{
			if (c is Depot)
			{
				var d = c as Depot;
				return direction == d.Direction;
			}
			else
				return true;
		}

		return false;
	}

	public void BuildRoad(Coord pos)
	{
		int cost;
		if (!CheckCost("build_road", "construire une route", out cost))
			return;

		AudioManager.Player.Play("buildRoad");
		Road road = new Road(pos, roadPrefab);
		UpdateRoad(road);
		Constructions[pos.X, pos.Y] = road;
		var neighbors = Neighbors(pos);
		foreach (Road r in neighbors)
			UpdateRoad(r);
	}

	public IEnumerator BuildRoads(List<Coord> path)
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
			}
			if (countLoop % searchSpeed == 0)
				yield return null;
		}

		var neighborsRoads = new List<Road>();
		foreach (Road r in constructedRoads)
		{
			countLoop++;

			neighborsRoads.Remove(r);
			foreach (Road neighborsRoad in Neighbors(r.Point))
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

	public void DestroyRoad(Coord pos)
	{
		int cost;
		if (!CheckCost("destroy_road", "détruire une route", out cost))
			return;

		var r = Constructions[pos.X, pos.Y] as Road;

		var neighborsRoads = new List<Road>();
		foreach (Road neighborsRoad in Neighbors(r.Point))
		{
			if (!neighborsRoads.Contains(neighborsRoad))
				neighborsRoads.Add(neighborsRoad);
		}

		foreach (Road neighborsRoad in neighborsRoads)
		{
			UpdateRoad(neighborsRoad);
		}

		Constructions[pos.X, pos.Y] = null;

		r.Destroy();
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


	public List<Road> Neighbors(Coord point)
	{
		var neighbors = new List<Road>();
		var directions = point.Directions();

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
				//TODO : visual A*
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
				{
					//TODO : visual A*
				}

				// Rebuild path
				parameters.Path = TraceBackPath(cameFrom, target);

				if (parameters.WaitAtTheEnd > 0f)
					yield return new WaitForSeconds(parameters.WaitAtTheEnd);
				if (parameters.VisualSearch)
				{
					//TODO : visual A*
				}

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
		{
			//TODO : visual A*
		}
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
		foreach (City otherCity in Cities)
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
		foreach (City otherCity in Cities)
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
		foreach (City otherCity in Cities)
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
