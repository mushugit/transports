using System;
using System.Collections;
using System.Collections.Generic;
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

	public Transform cityContainer;
	public Transform roadContainer;
	public Transform depotContainer;

	public Component truckPrefab;

	public Component uiCanvas;

	public static float width = 25;
	public static float height = width;

	public int minCityDistance = 4;
	public static Economy LocalEconomy { get; private set; }

	public static readonly int worldLoadSceneIndex = 1;
	public static readonly int looseSceneIndex = 3;


	public Construction[,] Constructions { get; private set; }
	public List<City> Cities;

	public static Vector3 Center { get; private set; } = new Vector3(width / 2f, 0f, height / 2f);

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

	private void TestDelegate(int value)
	{
		UnityEngine.Debug.Log($"Delegate received {value}");
	}

	private void Awake()
	{
		Instance = this;

		LocalEconomy = new Economy(EconomyTemplate.Difficulty.Free);
		Constructions = new Construction[(int)width, (int)height];
	}

	private void RecalculateLinks()
	{
		/*var sw = new Stopwatch();
		sw.Start();*/


		foreach (City c in Cities)
		{
			c.ClearLinks();
		}

		foreach (City c in Cities)
		{
			foreach (City otherCity in Cities)
			{
				if (c != otherCity && !c.IsLinkedTo(otherCity) && !c.IsUnreachable(otherCity))
				{
					var pf = new Pathfinder<Cell>(0, 0, new List<Type>(2) { typeof(Road), typeof(City) });
					StartCoroutine(pf.RoutineFindPath(c.Point, otherCity.Point));
					var path = pf.Path;
					if (path?.TotalCost > 0)
					{
						//UnityEngine.Debug.Log($"Found path of {path.TotalCost} steps from {c} to {otherCity}");
						StartCoroutine(UpdateLink(c, otherCity));
						
					}
					else
					{
						//UnityEngine.Debug.Log($"No path from {c} to {otherCity}");
						UpdateUnreachable(c, otherCity);
					}
				}
			}

			c.UpdateAllOutgoingFlux();
		}

		/*sw.Stop();
		DisplayTimeSpan("RecalculateLinks", sw.Elapsed, 1);*/
	}

	IEnumerator UpdateUnreachable(City a, City b)
	{
		var allAUnreachable = new List<City>(a.UnreachableCities) { a };
		var allBUnreachable = new List<City>(b.UnreachableCities) { b };

		foreach (City c in a.UnreachableCities)
			c.AddUnreachable(allBUnreachable);
		a.AddUnreachable(allBUnreachable);
		foreach (City c in b.UnreachableCities)
			c.AddUnreachable(allAUnreachable);
		b.AddUnreachable(allAUnreachable);
		yield return null;
	}

	void Start()
	{
		Application.targetFrameRate = 60;

		if (loadData == null)
		{
			UpdateWorldSize();
			InitLoader();
			StartCoroutine(Generate());
		}
		else
		{
			width = loadData.Width;
			height = loadData.Height;
			UpdateWorldSize();
			InitLoader(loadData.Constructions.Count + loadData.AllFlux.Count);
			StartCoroutine(Load());
		}
	}

	public void UpdateWorldSize()
	{
		Center = new Vector3(width / 2f, 0f, height / 2f);
		Constructions = new Construction[(int)width, (int)height];

		Cell.ResetCellSystem();
		MiniMapCamera.UpdateRender();

		var cam = Camera.main.GetComponent<Cam>();
		cam?.Center();
	}

	IEnumerator Load()
	{
		var w = (int)width;
		var h = (int)height;
		Constructions = new Construction[w, h];

		Cell.ResetCellSystem();

		itemLoading = "Chargement du terrain";
		Terrain(width, height);
		progressLoading++;

		itemLoading = "Chargement des constructions";
		Cities = new List<City>();
		var roads = new List<Cell>();
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
			if (c is Road)
			{
				roads.Add(c.Point);
			}
			progressLoading++;
		}
		yield return StartCoroutine(BuildRoads(roads));

		itemLoading = "Chargement des flux";
		foreach (Flux f in loadData.AllFlux)
		{
			Simulation.AddFlux(f);
		}
		progressLoading++;

		itemLoading = "Chargement terminé";
		gameLoading = false;

		LocalEconomy = new Economy(EconomyTemplate.Difficulty.Normal);
		CompleteLoading();
		yield return StartCoroutine(Simulation.Run());
	}

	private void CompleteLoading()
	{
		ActivateUI();
		CleanLoader();

		Cell.ResetCellSystem();
		MiniMapCamera.UpdateRender();

		var cam = Camera.main.GetComponent<Cam>();
		cam?.Center();
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
		CompleteLoading();
		yield return StartCoroutine(Simulation.Run());
	}

	IEnumerator Link(City a, City b)
	{
		if (!a.IsLinkedTo(b))
		{
			if ((a.LinkedCities == null && b.LinkedCities == null) || (a.LinkedCities != null && b.LinkedCities != null))
			{
				if (a.RoadInDirection(b.Point) < b.RoadInDirection(a.Point))
				{
					var c = a;
					a = b;
					b = c;
				}
			}
			else
			{
				if (a.LinkedCities == null)
				{
					var c = a;
					a = b;
					b = c;
				}
			}

			itemLoading = "Relie " + a.Name + " vers " + b.Name;
			var pf = new Pathfinder<Cell>(0, 0, null);
			yield return StartCoroutine(pf.RoutineFindPath(a.Point, b.Point));
			if (pf.Path != null && pf.Path.TotalCost > 0)
			{
				//UnityEngine.Debug.Log($"Path from {a.Name} to {b.Name}");
				yield return StartCoroutine(BuildRoads(pf.Path));
				yield return StartCoroutine(UpdateLink(a, b));
			}
			else
			{
				//UnityEngine.Debug.Log($"NO PATH from {a.Name} to {b.Name} ({pf.Path})");
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

	public void DestroyConstruction(Cell p)
	{
		var c = Constructions[p.X, p.Y];
		if (c != null)
		{
			if (c is Road)
			{
				DestroyRoad(p);
				RecalculateLinks();
			}
			if (c is City)
			{
				DestroyCity(p);
				RecalculateLinks();
			}
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
			var cityCenter = new Cell(x, y, null);
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

	public static bool CheckCost(string operationName, string messageOperation, out int cost)
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

	private void _BuildDepot(Cell pos, int direction)
	{
		int cost;
		if (!CheckCost("build_depot", "construire un dépôt", out cost))
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

	public void BuildDepot(Cell pos, int direction)
	{
		_BuildDepot(pos, direction);
	}

	public void BuildDepot(Cell pos)
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

	public void BuildCity(Cell cityCenter)
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

		RecalculateLinks();
	}

	public void DestroyCity(Cell cityCenter)
	{
		int cost;
		if (!CheckCost("destroy_city", "raser une ville", out cost))
			return;

		var c = Constructions[cityCenter.X, cityCenter.Y] as City;
		Cities.Remove(c);
		c.Destroy();
		Constructions[cityCenter.X, cityCenter.Y] = null;
	}

	public void DestroyDepot(Cell pos)
	{
		int cost;
		if (!CheckCost("destroy_depot", "détruire un dépôt", out cost))
			return;

		var d = Constructions[pos.X, pos.Y] as Depot;
		d.Destroy();
		Constructions[pos.X, pos.Y] = null;
	}

	public void UpdateRoad(Road r)
	{
		var north = North(r);
		var east = East(r);
		var south = South(r);
		var west = West(r);
		var isLinkNorth = IsLinkable(2, north);
		var isLinkEast = IsLinkable(3, east);
		var isLinkSouth = IsLinkable(0, south);
		var isLinkWest = IsLinkable(1, west);
		//UnityEngine.Debug.Log($"Update de {r.Point} : n={isLinkNorth} e={isLinkEast} s={isLinkSouth} w={isLinkWest}");
		r.UpdateConnexions(isLinkNorth, isLinkEast, isLinkSouth, isLinkWest);
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

	public void BuildRoad(Cell pos)
	{
		int cost;
		if (!CheckCost("build_road", "construire une route", out cost))
			return;

		AudioManager.Player.Play("buildRoad");
		Road road = new Road(pos, roadPrefab);
		Constructions[pos.X, pos.Y] = road;

		UpdateRoad(road);
		var neighbors = Neighbors(pos);
		foreach (Road r in neighbors)
			UpdateRoad(r);

		RecalculateLinks();
	}

	public IEnumerator BuildRoads(Path<Cell> path)
	{
		//UnityEngine.Debug.Log("Road from " + path.First() + " to " + path.Last());
		var countLoop = 0d;
		var constructedRoads = new List<Road>();
		foreach (Cell p in path)
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

	public IEnumerator BuildRoads(List<Cell> path)
	{
		//UnityEngine.Debug.Log("Road from " + path.First() + " to " + path.Last());
		var countLoop = 0d;
		var constructedRoads = new List<Road>();
		foreach (Cell p in path)
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

	public void DestroyRoad(Cell pos)
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


	public List<Road> Neighbors(Cell point)
	{
		var neighbors = new List<Road>();
		var directions = point.Directions();

		foreach (Cell p in directions)
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

		foreach (Cell p in directions)
		{
			var c = Constructions[p.X, p.Y];
			if (c != null && c is Road)
				neighbors.Add(c as Road);
		}

		return neighbors;
	}

	public static void DisplayTimeSpan(string label, TimeSpan ts, int divisor)
	{
		var t = (long)ts.TotalMilliseconds * 10 * 1000 / divisor;
		//UnityEngine.Debug.Log($"{label}:{new TimeSpan(t)} ({ts}/{divisor})");
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
			UnityEngine.Debug.Log("[WORLD] " + c.Name + " est proche et non lié de " + closestCity.Name);
		else
			UnityEngine.Debug.Log("[WORLD] " + c.Name + " est lié à tout");
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
