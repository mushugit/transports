using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class World : MonoBehaviour
{
    public static WorldSave LoadData { get; set; }

    public static bool GameLoading = true;
    public static float ProgressLoading;
    public static string ItemLoading = "Niveau en préparation";
    public static int TotalLoading = 1;

    private readonly double _coroutineYieldRate = 2000d;

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
    #endregion Unity editor properties

    public static Economy LocalEconomy { get; private set; }

    public static readonly int WorldLoadSceneIndex = 1;
    public static readonly int LoseSceneIndex = 3;

    public Construction[,] Constructions { get; private set; }

    public List<City> Cities;
    public List<Industry> Industries;

    public HConstructionPattern ConstructionPattern { get; private set; }

    public static Vector3 Center { get; private set; } = new Vector3(width / 2f, 0f, height / 2f);

    //TODO: pass reference to class using it
    public static World Instance { get; private set; }

    public static bool DisplayLabel = true;

    public static void ReloadLevel(int sceneIndex)
    {
        ClearInstance();
        SceneManager.LoadScene(sceneIndex);
    }

    public static void ClearInstance()
    {
        Instance?.Cities.Clear();
        Instance?.Industries.Clear();

        if (Instance != null)
            ServiceLocator.GetInstance<Simulation>()?.Clear();

        Flux.AllFlux.Clear();
        Instance = null;
    }

    public static void Lose()
    {
        SceneManager.LoadScene(LoseSceneIndex);
    }

    public static void CleanLoader()
    {
        var loader = Instance.GetComponentInParent<LevelLoader>();
        if (loader != null)
        {
            DestroyImmediate(loader.gameObject);
        }
    }

    private void SetLoading(string text)
    {
        ItemLoading = text;
    }

    public void Update()
    {
        if (GameLoading)
        {
            return;
        }

        if (Input.GetButtonDown("Label"))
        {
            DisplayLabel = !DisplayLabel;
        }

        if (LocalEconomy.Balance < LocalEconomy.GetGain("loose"))
        {
            Lose();
        }
    }

    private void InitLoader()
    {
        var nbLinks = City.Quantity((int)width, (int)height) + Industry.Quantity((int)width, (int)height);
        TotalLoading = 1 + nbLinks;
    }

    private void InitLoader(int forcedLoadCount)
    {
        TotalLoading = 1 + forcedLoadCount;
    }

    public void Awake()
    {
        Debug.Log("World Awake");
        Instance = this;
        ServiceLocator.Register<World, World>(this);
        ServiceLocator.Init();

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

        foreach (var item in linkableItems)
        {
            item.ClearLinks();
        }

        foreach (var item in linkableItems)
        {
            foreach (var otherItem in linkableItems)
            {
                if (item != otherItem && !item.IsLinkedTo(otherItem) && !item.IsUnreachable(otherItem))
                {
                    var pf = new Pathfinder<Cell>(new List<Type>(2) { typeof(Road), typeof(City), typeof(Industry) });
                    StartCoroutine(pf.RoutineFindPath(item._Cell, otherItem._Cell));
                    var path = pf.Path;
                    if (path?.TotalCost > 0)
                    {
                        //Debug.Log($"Found path of {path.TotalCost} steps from {c} to {otherCity}");
                        StartCoroutine(HLinkHandler.UpdateLink(item, otherItem));
                    }
                    else
                    {
                        //Debug.Log($"No path from {c} to {otherCity}");
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

    public void Start()
    {
        Instance = this;
        ServiceLocator.Init();
        Application.targetFrameRate = 60;

        if (LoadData == null)
        {
            UpdateWorldSize();
            InitLoader();
            StartCoroutine(GenerateMap());
        }
        else
        {
            width = LoadData.Width;
            height = LoadData.Height;
            UpdateWorldSize();
            InitLoader(LoadData.Constructions.Count + LoadData.AllFlux.Count);
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

    private IEnumerator LoadMap()
    {
        Debug.Log("Loading");
        if (LoadData.Balance.HasValue)
        {
            LocalEconomy = new Economy(EconomyTemplate.Difficulty.Free, LoadData.Balance.Value);
        }

        var w = (int)width;
        var h = (int)height;

        ConstructionPattern = new HConstructionPattern(w, h);

        Constructions = new Construction[w, h];
        Cities = new List<City>(City.Quantity(w, h));
        Industries = new List<Industry>(Industry.Quantity(w, h));

        Cell.ResetCellSystem();
        ServiceLocator.GetInstance<Simulation>().Clear();

        ItemLoading = "Chargement du terrain";
        Terrain(width, height);
        ProgressLoading++;

        ItemLoading = "Chargement des constructions";
        var roads = new List<Cell>();
        var industryToLoad = new List<Industry>();
        foreach (var c in LoadData.Constructions)
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
                BuildDepot(c as Depot);
            }

            if (c is Road)
            {
                roads.Add(c._Cell);
            }

            ProgressLoading++;
        }

        Industry.InitCityReference();
        foreach (var i in industryToLoad)
        {
            BuildIndustry(i);
        }

        yield return StartCoroutine(BuildRoads(roads));

        ItemLoading = "Chargement des flux";
        Debug.Log($"Flux à charger : {LoadData.AllFlux.Count.ToString()}");
        foreach (var f in LoadData.AllFlux)
        {
            Debug.Log($"\tFlux : {f}");
            ServiceLocator.GetInstance<Simulation>().AddFlux(f);
        }

        ProgressLoading++;

        ItemLoading = "Chargement terminé";
        GameLoading = false;

        if (LoadData.DifficultyLevel.HasValue)
        {
            LocalEconomy.ChangeDifficulty(LoadData.DifficultyLevel.Value);
        }
        else
        {
            LocalEconomy.ChangeDifficulty(EconomyTemplate.Difficulty.Normal);
        }
        CompleteLoading();
        yield return StartCoroutine(ServiceLocator.GetInstance<Simulation>().Run());
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
        GameLoading = false;

        foreach (var b in uiCanvas.GetComponentsInChildren<Button>())
        {
            b.interactable = true;
        }
    }

    private IEnumerator GenerateMap()
    {
        var w = (int)width;
        var h = (int)height;

        ConstructionPattern = new HConstructionPattern(w, h);

        Constructions = new Construction[w, h];
        Cities = new List<City>(City.Quantity(w, h));
        Industries = new List<Industry>(Industry.Quantity(w, h));

        ItemLoading = "Chargement du terrain";
        Terrain(width, height);
        ProgressLoading++;

        ItemLoading = "Chargement des villes";
        GenerateCity(w, h);
        foreach (var c in Cities)
        {
            var closestCity = ClosestCityUnlinked(c);
            if (closestCity != null)
            {
                yield return StartCoroutine(HLinkHandler.Link(c, closestCity, SetLoading, BuildRoads, HLinkHandler.UpdateLink));
            }

            ProgressLoading++;
        }

        ItemLoading = "Chargement des industries";
        GenerateIndustry(w, h);
        foreach (var i in Industries)
        {
            var closestCity = ClosestCityUnlinked(i);
            if (closestCity != null)
            {
                yield return StartCoroutine(HLinkHandler.Link(i, closestCity, SetLoading, BuildRoads, HLinkHandler.UpdateLink));
            }

            ProgressLoading++;
        }

        ItemLoading = "Chargement terminé";
        GameLoading = false;

        LocalEconomy.ChangeDifficulty(EconomyTemplate.Difficulty.Normal);
        CompleteLoading();
        yield return StartCoroutine(ServiceLocator.GetInstance<Simulation>().Run());
    }

    private void Terrain(float width, float height)
    {
        var t = Instantiate(TerrainPrefab, Vector3.zero, Quaternion.identity);
        t.transform.localScale = new Vector3(width, 1f, height);
        var c = t.GetComponentInChildren<CellRender>();
        c?.SetScale(width, height);
    }

    #region Construction Operation
    #region Construction Generator
    private void GenerateIndustry(int w, int h)
    {
        var quantity = Industry.Quantity(w, h);
        var n = 0;
        while (n < quantity)
        {
            var x = UnityEngine.Random.Range(0, w);
            var y = UnityEngine.Random.Range(0, h);
            var center = new Cell(x, y);
            if (Constructions[x, y] == null)
            {
                var canBuild = true;
                foreach (var otherCity in Cities)
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

    private void GenerateCity(int w, int h)
    {
        var quantity = City.Quantity(w, h);
        var n = 0;

        while (n < quantity)
        {
            var x = UnityEngine.Random.Range(0, w);
            var y = UnityEngine.Random.Range(0, h);
            var center = new Cell(x, y);
            if (Constructions[x, y] == null)
            {
                var canBuild = true;
                foreach (var otherCity in Cities)
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
    #endregion Construction Generator

    #region Construction Build
    public void BuildRoad(Cell pos)
    {
        int cost;
        if (!Economy.CheckCost(LocalEconomy, "build_road", "construire une route", out cost))
        {
            return;
        }

        AudioManager.Player.Play("buildRoad");
        var road = new Road(pos);
        Constructions[pos.X, pos.Y] = road;

        road.UpdateConnections();
        foreach (var roadAround in Neighbors(pos))
        {
            roadAround.UpdateConnections();
        }

        RecalculateLinks();
    }

    public IEnumerator BuildRoads(IEnumerable<Cell> path)
    {
        //Debug.Log("Road from " + path.First() + " to " + path.Last());
        var countLoop = 0d;
        var constructedRoads = new List<Road>();
        foreach (var p in path)
        {
            countLoop++;
            if (Constructions[p.X, p.Y] == null)
            {
                var r = new Road(p);
                constructedRoads.Add(r);
                Constructions[p.X, p.Y] = r;
            }

            if (countLoop % _coroutineYieldRate == 0)
            {
                yield return null;
            }
        }

        var neighborsRoads = new List<Road>();
        foreach (var r in constructedRoads)
        {
            countLoop++;

            foreach (var neighborsRoad in Neighbors(r._Cell))
            {
                if (!neighborsRoads.Contains(neighborsRoad))
                {
                    neighborsRoads.Add(neighborsRoad);
                }
            }

            r.UpdateConnections();
            foreach (var neighborsRoad in neighborsRoads)
            {
                neighborsRoad.UpdateConnections();
            }

            if (countLoop % _coroutineYieldRate == 0)
            {
                yield return null;
            }
        }

        yield return null;
    }

    private void _BuildDepot(Depot dummy)
    {
        int cost;
        if (!Economy.CheckCost(LocalEconomy, "build_depot", "construire un dépôt", out cost))
        {
            return;
        }

        AudioManager.Player.Play("buildCity");
        var c = new Depot(dummy);

        Constructions[c._Cell.X, c._Cell.Y] = c;

        Road.UpdateAllRoad(Neighbors(c._Cell));
    }

    private void _BuildDepot(Cell pos, int direction)
    {
        int cost;
        if (!Economy.CheckCost(LocalEconomy, "build_depot", "construire un dépôt", out cost))
        {
            return;
        }

        AudioManager.Player.Play("buildCity");
        Constructions[pos.X, pos.Y] = new Depot(pos, direction);

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
        {
            return;
        }

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
        {
            return;
        }

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
        {
            return;
        }

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
        {
            return;
        }

        AudioManager.Player.Play("buildCity");
        var c = new City(center);

        Constructions[center.X, center.Y] = c;
        Cities.Add(c);

        Road.UpdateAllRoad(Neighbors(center));

        RecalculateLinks();
    }
    #endregion Construction Build

    #region Construction Destroy
    public bool DestroyConstruction(Cell p)
    {
        var c = Constructions[p.X, p.Y];
        var isDestroyed = false;
        if (c != null)
        {
            if (c is Road)
            {
                isDestroyed = Demolish(c);
            }

            if (c is Industry)
            {
                isDestroyed = Demolish(c, Industries);
            }

            if (c is City)
            {
                isDestroyed = Demolish(c, Cities);
            }

            if (c is Depot)
            {
                isDestroyed = Demolish(c);
            }
        }

        Road.UpdateAllRoad(Neighbors(p));
        return isDestroyed;
    }

    private bool Demolish(Construction c, IList list = null)
    {
        int cost;
        if (!Economy.CheckCost(LocalEconomy, c.DestroyOperation, c.DestroyLabel, out cost))
        {
            return false;
        }

        Constructions[c._Cell.X, c._Cell.Y] = null;
        list?.Remove(c);

        if (c is ILinkable)
        {
            RecalculateLinks();
        }

        c.Demolish();
        return true;
    }
    #endregion Construction Destroy
    #endregion Construction Operation

    #region Neighbor
    public int CountNeighbors(Construction c)
    {
        var count = 0;

        //left
        if (c._Cell.X > 0 && Constructions[c._Cell.X - 1, c._Cell.Y] != null)
        {
            count++;
        }

        //right
        if (c._Cell.X < width - 1 && Constructions[c._Cell.X + 1, c._Cell.Y] != null)
        {
            count++;
        }

        //up
        if (c._Cell.Y < height - 1 && Constructions[c._Cell.X, c._Cell.Y + 1] != null)
        {
            count++;
        }

        //down
        if (c._Cell.Y > 0 && Constructions[c._Cell.X, c._Cell.Y - 1] != null)
        {
            count++;
        }

        return count;
    }

    public Construction West(Construction c)
    {
        if (c._Cell.X > 0)
        {
            return Constructions[c._Cell.X - 1, c._Cell.Y];
        }

        return null;
    }

    public Construction East(Construction c)
    {
        if (c._Cell.X < width - 1)
        {
            return Constructions[c._Cell.X + 1, c._Cell.Y];
        }

        return null;
    }

    public Construction North(Construction c)
    {
        if (c._Cell.Y < height - 1)
        {
            return Constructions[c._Cell.X, c._Cell.Y + 1];
        }

        return null;
    }

    public Construction South(Construction c)
    {
        if (c._Cell.Y > 0)
        {
            return Constructions[c._Cell.X, c._Cell.Y - 1];
        }

        return null;
    }

    public List<Road> Neighbors(Cell cell)
    {
        var neighbors = new List<Road>();
        var directions = cell.Directions();

        foreach (var point in directions.Where(d => d != null))
        {
            var construction = Constructions[point.X, point.Y];
            if (construction is Road) //c != null && 
            {
                neighbors.Add(construction as Road);
            }
        }

        return neighbors;
    }

    public List<Road> ExtendedNeighbors(Road road)
    {
        var neighbors = new List<Road>();
        var directions = road._Cell.ExtendedDirections();

        foreach (var p in directions.Where(d => d != null))
        {
            var construction = Constructions[p.X, p.Y];
            if (construction != null && construction is Road)
            {
                neighbors.Add(construction as Road);
            }
        }

        return neighbors;
    }
    #endregion Neighbor

    #region City distances
    private City ClosestCityUnlinked(ILinkable item)
    {
        var minDistance = int.MaxValue;
        City closestCity = null;
        foreach (var otherCity in Cities)
        {
            if (otherCity != item && !item.Linked.Contains(otherCity) && item is IHasRelativeDistance)
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

        /*//Debug
		if (closestCity != null)
			Debug.Log("[WORLD] " + c.Name + " est proche et non lié de " + closestCity.Name);
		else
			Debug.Log("[WORLD] " + c.Name + " est lié à tout");z*/
        return closestCity;
    }

    public City ClosestCity(Cell cell)
    {
        var minDistance = int.MaxValue;
        City closestCity = null;
        foreach (var city in Cities)
        {
            if (city._Cell != cell)
            {
                var d = cell.ManhattanDistance(city);
                if (d < minDistance)
                {
                    minDistance = d;
                    closestCity = city;
                }
            }
        }

        return closestCity;
    }

    public City ClosestCity(City city)
    {
        var minDistance = int.MaxValue;
        City closestCity = null;
        foreach (var otherCity in Cities)
        {
            if (otherCity != city)
            {
                var d = city.ManhattanDistance(otherCity);
                if (d < minDistance)
                {
                    minDistance = d;
                    closestCity = otherCity;
                }
            }
        }

        return closestCity;
    }
    #endregion City distances
}
