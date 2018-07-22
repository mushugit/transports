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

    private readonly double coroutineYieldRate = 200d;

    public static float width = 25;
    public static float height = width;

    #region Unity editor properties
    [Header("World Gen settings")]

    public int minCityDistance = 4;
    public int minIndustryDistanceToCity = 2;

    [Header("General prefabs")]
    public Component TruckPrefab;
    public Component TerrainPrefab;

    [Header("Construction prefabs")]
    public Component CityPrefab;
    public Component RoadPrefab;
    public Component DepotPrefab;
    public Component IndustryPrefab;

    [Header("Containers")]
    public Transform CityContainer;
    public Transform RoadContainer;
    public Transform DepotContainer;
    public Transform IndustryContainer;

    [Header("UI")]
    public Component uiCanvas;
    #endregion

    public static Economy LocalEconomy { get; private set; }

    public static readonly int worldLoadSceneIndex = 1;
    public static readonly int looseSceneIndex = 3;

    public Construction[,] Constructions { get; private set; }

    public List<City> Cities;
    public List<Industry> Industries;

    public static Vector3 Center { get; private set; } = new Vector3(width / 2f, 0f, height / 2f);

    public static World Instance { get; private set; }

    public static bool DisplayLabel = true;

    public static void ReloadLevel(int sceneIndex)
    {
        Instance = null;
        Simulation.Clear();
        SceneManager.LoadScene(sceneIndex);
    }

    public static void ClearInstance()
    {
        Instance?.Cities.Clear();
        Instance?.Industries.Clear();

        Flux.AllFlux.Clear();
        Instance = null;
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

    private void SetLoading(string text)
    {
        itemLoading = text;
    }

    void Update()
    {
        if (gameLoading)
            return;

        if (Input.GetButtonDown("Label"))
            DisplayLabel = !DisplayLabel;

        if (LocalEconomy.Balance < LocalEconomy.GetGain("loose"))
            Loose();
    }

    void InitLoader()
    {
        float nbLinks = City.Quantity((int)width, (int)height) + Industry.Quantity((int)width, (int)height);
        totalLoading = 1 + nbLinks;
    }

    void InitLoader(int forcedLoadCount)
    {
        totalLoading = 1 + forcedLoadCount;
    }

    private void Awake()
    {
        UnityEngine.Debug.Log("World Awake");
        Instance = this;

        LocalEconomy = new Economy(EconomyTemplate.Difficulty.Free);
        Constructions = new Construction[(int)width, (int)height];
        Cities = new List<City>(City.Quantity((int)width, (int)height));
        Industries = new List<Industry>(Industry.Quantity((int)width, (int)height));
    }

    private void RecalculateLinks()
    {
        var linkableItems = new List<ILinkable>();
        linkableItems.AddRange(Cities);
        linkableItems.AddRange(Industries);

        foreach (ILinkable item in linkableItems)
            item.ClearLinks();

        foreach (ILinkable item in linkableItems)
        {
            foreach (ILinkable otherItem in linkableItems)
            {
                if (item != otherItem && !item.IsLinkedTo(otherItem) && !item.IsUnreachable(otherItem))
                {
                    var pf = new Pathfinder<Cell>(0, 0, new List<Type>(2) { typeof(Road), typeof(City), typeof(Industry) });
                    StartCoroutine(pf.RoutineFindPath(item._Cell, otherItem._Cell));
                    var path = pf.Path;
                    if (path?.TotalCost > 0)
                    {
                        //UnityEngine.Debug.Log($"Found path of {path.TotalCost} steps from {c} to {otherCity}");
                        StartCoroutine(HLinkHandler.UpdateLink(item, otherItem));
                    }
                    else
                    {
                        //UnityEngine.Debug.Log($"No path from {c} to {otherCity}");
                        HLinkHandler.UpdateUnreachable(item, otherItem);
                    }
                }
            }

            if (item is ICargoProvider)
            {
                var cp = item as ICargoProvider;
                cp.UpdateAllOutgoingFlux();
            }
        }
    }

    void Start()
    {
        Application.targetFrameRate = 60;

        if (loadData == null)
        {
            UpdateWorldSize();
            InitLoader();
            StartCoroutine(GenerateMap());
        }
        else
        {
            width = loadData.Width;
            height = loadData.Height;
            UpdateWorldSize();
            InitLoader(loadData.Constructions.Count + loadData.AllFlux.Count);
            StartCoroutine(LoadMap());
        }
    }

    public void UpdateWorldSize()
    {
        Center = new Vector3(width / 2f, 0f, height / 2f);
        Constructions = new Construction[(int)width, (int)height];
        Cities = new List<City>(City.Quantity((int)width, (int)height));
        Industries = new List<Industry>(Industry.Quantity((int)width, (int)height));

        Cell.ResetCellSystem();
        MiniMapCamera.UpdateRender();

        var cam = Camera.main.GetComponent<Cam>();
        cam?.Center();
    }

    IEnumerator LoadMap()
    {
        Debug.Log("Loading");
        if (loadData.Balance.HasValue)
        {
            LocalEconomy = new Economy(EconomyTemplate.Difficulty.Free, loadData.Balance.Value);
        }

        var w = (int)width;
        var h = (int)height;
        Constructions = new Construction[w, h];
        Cities = new List<City>(City.Quantity(w, h));
        Industries = new List<Industry>(Industry.Quantity(w, h));

        Cell.ResetCellSystem();
        Simulation.Clear();

        itemLoading = "Chargement du terrain";
        Terrain(width, height);
        progressLoading++;

        itemLoading = "Chargement des constructions";
        var roads = new List<Cell>();
        var industryToLoad = new List<Industry>();
        foreach (Construction c in loadData.Constructions)
        {
            if (c is City)
            {
                BuildCity(c as City);
            }
            if (c is Industry)
            {
                //Load industry later as they need the list of cities for names
                industryToLoad.Add(c as Industry);
            }
            if (c is Depot)
            {
                var d = c as Depot;
                BuildDepot(c as Depot);
            }
            if (c is Road)
            {
                roads.Add(c._Cell);
            }
            progressLoading++;
        }
        Industry.InitCityReference();
        foreach (Industry i in industryToLoad)
        {
            BuildIndustry(i);
        }
        yield return StartCoroutine(BuildRoads(roads));

        itemLoading = "Chargement des flux";
        UnityEngine.Debug.Log($"Flux à charger : {loadData.AllFlux.Count}");
        foreach (Flux f in loadData.AllFlux)
        {
            UnityEngine.Debug.Log($"\tFlux : {f}");
            Simulation.AddFlux(f);
        }
        progressLoading++;

        itemLoading = "Chargement terminé";
        gameLoading = false;

        if (loadData.DifficultyLevel.HasValue)
        {
            LocalEconomy.ChangeDifficulty(loadData.DifficultyLevel.Value);
        }
        else
        {
            LocalEconomy.ChangeDifficulty(EconomyTemplate.Difficulty.Normal);
        }
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

    IEnumerator GenerateMap()
    {
        var w = (int)width;
        var h = (int)height;

        Constructions = new Construction[w, h];
        Cities = new List<City>(City.Quantity(w, h));
        Industries = new List<Industry>(Industry.Quantity(w, h));

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
                yield return StartCoroutine(HLinkHandler.Link(c, closestCity, SetLoading, BuildRoads, HLinkHandler.UpdateLink));
            }
            progressLoading++;
        }

        itemLoading = "Chargement des industries";
        GenerateIndustry(w, h);
        foreach (Industry i in Industries)
        {
            var closestCity = ClosestCityUnlinked(i);
            if (closestCity != null)
            {
                yield return StartCoroutine(HLinkHandler.Link(i, closestCity, SetLoading, BuildRoads, HLinkHandler.UpdateLink));
            }
            progressLoading++;
        }

        itemLoading = "Chargement terminé";
        gameLoading = false;

        LocalEconomy.ChangeDifficulty(EconomyTemplate.Difficulty.Normal);
        CompleteLoading();
        yield return StartCoroutine(Simulation.Run());
    }

    void Terrain(float width, float height)
    {
        var t = Instantiate(TerrainPrefab, Vector3.zero, Quaternion.identity);
        t.transform.localScale = new Vector3(width, 1f, height);
        var c = t.GetComponentInChildren<CellRender>();
        c?.SetScale(width, height);
    }

    #region Construction Operation
    #region Construction Generator
    void GenerateIndustry(int w, int h)
    {
        var quantity = Industry.Quantity(w, h);
        var n = 0;
        while (n < quantity)
        {
            int x = UnityEngine.Random.Range(0, w);
            int y = UnityEngine.Random.Range(0, h);
            var center = new Cell(x, y);
            if (Constructions[x, y] == null)
            {
                var canBuild = true;
                foreach (City otherCity in Cities)
                {
                    if (otherCity.ManhattanDistance(center) < minIndustryDistanceToCity)
                    {
                        canBuild = false;
                        break;
                    }
                }
                if (canBuild)
                {
                    BuildIndustry(center);
                    n++;
                }
            }
        }
    }

    void GenerateCity(int w, int h)
    {
        var quantity = City.Quantity(w, h);
        var n = 0;
        while (n < quantity)
        {
            int x = UnityEngine.Random.Range(0, w);
            int y = UnityEngine.Random.Range(0, h);
            var center = new Cell(x, y);
            if (Constructions[x, y] == null)
            {
                var canBuild = true;
                foreach (City otherCity in Cities)
                {
                    if (otherCity.ManhattanDistance(center) < minCityDistance)
                    {
                        canBuild = false;
                        break;
                    }
                }
                if (canBuild)
                {
                    BuildCity(center);
                    n++;
                }
            }
        }
    }
    #endregion

    #region Construction Build
    public void BuildRoad(Cell pos)
    {
        int cost;
        if (!Economy.CheckCost(LocalEconomy, "build_road", "construire une route", out cost))
            return;

        AudioManager.Player.Play("buildRoad");
        Road road = new Road(pos);
        Constructions[pos.X, pos.Y] = road;

        road.UpdateConnections();
        var neighbors = Neighbors(pos);
        foreach (Road r in neighbors)
            r.UpdateConnections();

        RecalculateLinks();
    }

    public IEnumerator BuildRoads(IEnumerable<Cell> path)
    {
        //UnityEngine.Debug.Log("Road from " + path.First() + " to " + path.Last());
        var countLoop = 0d;
        var constructedRoads = new List<Road>();
        foreach (Cell p in path)
        {
            countLoop++;
            if (Constructions[p.X, p.Y] == null)
            {
                Road r = new Road(p);
                constructedRoads.Add(r);
                Constructions[p.X, p.Y] = r;
            }
            if (countLoop % coroutineYieldRate == 0)
                yield return null;
        }

        var neighborsRoads = new List<Road>();
        foreach (Road r in constructedRoads)
        {
            countLoop++;

            foreach (Road neighborsRoad in Neighbors(r._Cell))
            {
                if (!neighborsRoads.Contains(neighborsRoad))
                    neighborsRoads.Add(neighborsRoad);
            }

            r.UpdateConnections();
            foreach (Road neighborsRoad in neighborsRoads)
            {
                neighborsRoad.UpdateConnections();
            }

            if (countLoop % coroutineYieldRate == 0)
                yield return null;
        }
        yield return null;
    }

    private void _BuildDepot(Depot dummy)
    {
        int cost;
        if (!Economy.CheckCost(LocalEconomy, "build_depot", "construire un dépôt", out cost))
            return;

        AudioManager.Player.Play("buildCity");
        var c = new Depot(dummy);

        Constructions[c._Cell.X, c._Cell.Y] = c;

        Road.UpdateAllRoad(Neighbors(c._Cell));
    }

    private void _BuildDepot(Cell pos, int direction)
    {
        int cost;
        if (!Economy.CheckCost(LocalEconomy, "build_depot", "construire un dépôt", out cost))
            return;

        AudioManager.Player.Play("buildCity");
        var c = new Depot(pos, direction);

        Constructions[pos.X, pos.Y] = c;

        Road.UpdateAllRoad(Neighbors(pos));
    }

    public void BuildDepot(Cell pos, int direction)
    {
        _BuildDepot(pos, direction);
    }

    public void BuildDepot(Cell pos)
    {
        _BuildDepot(pos, Builder.RotationDirection);
    }

    public void BuildDepot(Depot dummy)
    {
        _BuildDepot(dummy);
    }

    public void BuildIndustry(Industry dummy)
    {
        int cost;
        if (!Economy.CheckCost(LocalEconomy, "build_industry", "fonder une nouvelle industrie", out cost))
            return;

        AudioManager.Player.Play("buildCity");
        var i = new Industry(dummy);
        var center = dummy._Cell;

        Constructions[center.X, center.Y] = i;
        Industries.Add(i);

        Road.UpdateAllRoad(Neighbors(center));

        RecalculateLinks();
    }

    public void BuildIndustry(Cell center)
    {
        int cost;
        if (!Economy.CheckCost(LocalEconomy, "build_industry", "fonder une nouvelle industrie", out cost))
            return;

        AudioManager.Player.Play("buildCity");
        var i = new Industry(center);

        Constructions[center.X, center.Y] = i;
        Industries.Add(i);

        Road.UpdateAllRoad(Neighbors(center));

        RecalculateLinks();
    }

    public void BuildCity(City dummy)
    {
        int cost;
        if (!Economy.CheckCost(LocalEconomy, "build_city", "bâtir une nouvelle ville", out cost))
            return;

        var c = new City(dummy);
        var center = dummy._Cell;

        Constructions[center.X, center.Y] = c;
        Cities.Add(c);

        Road.UpdateAllRoad(Neighbors(center));
    }

    public void BuildCity(Cell center)
    {
        int cost;
        if (!Economy.CheckCost(LocalEconomy, "build_city", "bâtir une nouvelle ville", out cost))
            return;

        AudioManager.Player.Play("buildCity");
        var c = new City(center);

        Constructions[center.X, center.Y] = c;
        Cities.Add(c);

        Road.UpdateAllRoad(Neighbors(center));

        RecalculateLinks();
    }
    #endregion

    #region Construction Destroy
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
            if (c is Industry)
            {
                DestroyIndustry(p);
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

        Road.UpdateAllRoad(Neighbors(p));
    }

    public void DestroyRoad(Cell pos)
    {
        int cost;
        if (!Economy.CheckCost(LocalEconomy, "destroy_road", "détruire une route", out cost))
            return;

        var r = Constructions[pos.X, pos.Y] as Road;

        Road.UpdateAllRoad(Neighbors(r._Cell));

        Constructions[pos.X, pos.Y] = null;

        r.Destroy();
    }

    public void DestroyIndustry(Cell center)
    {
        int cost;
        if (!Economy.CheckCost(LocalEconomy, "destroy_industry", "raser une industrie", out cost))
            return;

        var i = Constructions[center.X, center.Y] as Industry;
        Industries.Remove(i);
        i.Destroy();
        Constructions[center.X, center.Y] = null;
    }

    public void DestroyCity(Cell center)
    {
        int cost;
        if (!Economy.CheckCost(LocalEconomy, "destroy_city", "raser une ville", out cost))
            return;

        var c = Constructions[center.X, center.Y] as City;
        Cities.Remove(c);
        c.Destroy();
        Constructions[center.X, center.Y] = null;
    }

    public void DestroyDepot(Cell center)
    {
        int cost;
        if (!Economy.CheckCost(LocalEconomy, "destroy_depot", "détruire un dépôt", out cost))
            return;

        var d = Constructions[center.X, center.Y] as Depot;
        d.Destroy();
        Constructions[center.X, center.Y] = null;
    }
    #endregion
    #endregion

    #region Neighbor
    public int CountNeighbors(Construction c)
    {
        int count = 0;

        //left
        if (c._Cell.X > 0)
            if (Constructions[c._Cell.X - 1, c._Cell.Y] != null)
                count++;
        //right
        if (c._Cell.X < width - 1)
            if (Constructions[c._Cell.X + 1, c._Cell.Y] != null)
                count++;
        //up
        if (c._Cell.Y < height - 1)
            if (Constructions[c._Cell.X, c._Cell.Y + 1] != null)
                count++;
        //down
        if (c._Cell.Y > 0)
            if (Constructions[c._Cell.X, c._Cell.Y - 1] != null)
                count++;

        return count;
    }

    public Construction West(Construction c)
    {
        if (c._Cell.X > 0)
            return Constructions[c._Cell.X - 1, c._Cell.Y];
        return null;
    }

    public Construction East(Construction c)
    {
        if (c._Cell.X < width - 1)
            return Constructions[c._Cell.X + 1, c._Cell.Y];
        return null;
    }

    public Construction North(Construction c)
    {
        if (c._Cell.Y < height - 1)
            return Constructions[c._Cell.X, c._Cell.Y + 1];
        return null;
    }

    public Construction South(Construction c)
    {
        if (c._Cell.Y > 0)
            return Constructions[c._Cell.X, c._Cell.Y - 1];
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
        var directions = r._Cell.ExtendedDirections();

        foreach (Cell p in directions)
        {
            var c = Constructions[p.X, p.Y];
            if (c != null && c is Road)
                neighbors.Add(c as Road);
        }

        return neighbors;
    }
    #endregion

    #region City distances
    City ClosestCityUnlinked(ILinkable item)
    {
        var minDistance = int.MaxValue;
        City closestCity = null;
        foreach (City otherCity in Cities)
        {
            if (otherCity != item && !item.Linked.Contains(otherCity))
            {
                if (item is IHasRelativeDistance)
                {
                    var itemWithDistance = item as IHasRelativeDistance;
                    var d = itemWithDistance.ManhattanDistance(otherCity);
                    if (d < minDistance)
                    {
                        minDistance = d;
                        closestCity = otherCity;
                    }
                }
            }
        }

        /*//Debug
		if (closestCity != null)
			UnityEngine.Debug.Log("[WORLD] " + c.Name + " est proche et non lié de " + closestCity.Name);
		else
			UnityEngine.Debug.Log("[WORLD] " + c.Name + " est lié à tout");z*/
        return closestCity;
    }

    public City ClosestCity(Cell c)
    {
        var minDistance = int.MaxValue;
        City closestCity = null;
        foreach (City city in Cities)
        {
            if (city._Cell != c)
            {
                var d = c.ManhattanDistance(city);
                if (d < minDistance)
                {
                    minDistance = d;
                    closestCity = city;
                }
            }
        }
        return closestCity;
    }

    public City ClosestCity(City c)
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
    #endregion
}
