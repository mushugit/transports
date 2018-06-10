using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class World : MonoBehaviour
{
    public Component cellPrefab;
    public Component cityPrefab;
    public Component roadPrefab;

    public float width = 20f;
    public float height = 20f;


    public Construction[,] Constructions { get; private set; }
    public Component[,] Terrains { get; private set; }
    private List<City> cities;

    bool visualSearchInProgress; //while not finished

    Vector2 Center()
    {
        return new Vector2(width / 2f, height / 2f);
    }

    void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void Update()
    {
        if (Input.GetButton("Reload"))
            ReloadLevel();
    }

    void Start()
    {
        CenterCam();
        Generate();
        StartCoroutine("Simulation");
    }

    void CenterCam()
    {
        Camera.main.SendMessage("Center", Center());
    }

    void Generate()
    {
        var w = (int)width;
        var h = (int)height;

        Constructions = new Construction[w, h];
        Terrains = new Component[w, h];

        Cities(w, h);

        for (float x = 0f; x < width; x++)
        {
            for (float y = 0f; y < height; y++)
            {
                Terrains[(int)x, (int)y] = Terrain(x, y);
            }
        }


        foreach (City c in cities)
        {
            var closedCity = ClosestCity(c);
            Link(c, closedCity);
        }

    }

    void Link(City a, City b)
    {
        if (!a.IsLinkedTo(b))
        {
            var parameters = new SearchParameter(a.Point, b.Point, 0, 0, false, null);
            var path = SearchPath(parameters);
            if (path != null)
            {
                //Debug.Log("Path from " + a.Name + " to " + b.Name);
                Roads(path);
                UpdateLink();
            }
            else
            {
                //Debug.Log("No path from " + a.Name + " to " + b.Name);
            }
        }
    }

    void UpdateLink()
    {
        foreach (City a in cities)
        {
            foreach (City b in cities)
            {
                if (a != b && !a.IsLinkedTo(b))
                {
                    var parameters = new SearchParameter(a.Point, b.Point, 0, 0, false, null, true);
                    var path = SearchPath(parameters);
                    if (path != null)
                    {
                        //Debug.Log(a.Name + " is now linked to " + b.Name);
                        a.AddLinkTo(b);
                        b.AddLinkTo(a);
                    }
                }
            }
        }
    }

    bool AllCitiesLinkedToEachOther()
    {
        foreach (City a in cities)
        {
            foreach (City b in cities)
            {
                if (b != a)
                {
                    if (!a.IsLinkedTo(b))
                        return false;
                }
            }
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

    Component Terrain(float x, float y)
    {
        return Instantiate(cellPrefab, new Vector3(x, -0.05f, y), Quaternion.identity);
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
            var cityCenter = new Point(x, y);
            if (Constructions[x, y] == null)
            {
                var canBuild = true;
                foreach (City otherCity in cities)
                {
                    if (otherCity.ManhattanDistance(cityCenter) < 3)
                    {
                        canBuild = false;
                    }
                }
                if (canBuild)
                {
                    var c = new City(cityCenter, cityPrefab);
                    Constructions[x, y] = c;
                    cities.Add(c);
                    n++;
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

    void Roads(List<Point> path)
    {
        //Debug.Log("Road from " + path.First() + " to " + path.Last());

        foreach (Point p in path)
        {
            if (Constructions[p.X, p.Y] == null)
            {
                Road r = new Road(p, roadPrefab);
                Constructions[p.X, p.Y] = r;
            }
        }
        var neighborsRoads = new List<Road>();
        foreach (Point p in path)
        {
            Construction c = Constructions[p.X, p.Y];
            if (c is Road)
            {
                Road r = (Road)c;
                neighborsRoads.Remove(r);
                foreach (Construction n in Neighbors(c))
                {
                    if (n is Road && !neighborsRoads.Contains(n))
                        neighborsRoads.Add((Road)n);
                }

                UpdateRoad(r);
                foreach (Road neighborsRoad in neighborsRoads)
                {
                    UpdateRoad(neighborsRoad);
                }
            }
        }
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


    public List<Construction> Neighbors(Construction c)
    {
        var neighbors = new List<Construction>();
        //left
        if (c.Point.X > 0)
            if (Constructions[c.Point.X - 1, c.Point.Y] != null)
                neighbors.Add(Constructions[c.Point.X - 1, c.Point.Y]);
        //right
        if (c.Point.X < width - 1)
            if (Constructions[c.Point.X + 1, c.Point.Y] != null)
                neighbors.Add(Constructions[c.Point.X + 1, c.Point.Y]);
        //up
        if (c.Point.Y < height - 1)
            if (Constructions[c.Point.X, c.Point.Y + 1] != null)
                neighbors.Add(Constructions[c.Point.X, c.Point.Y + 1]);
        //down
        if (c.Point.Y > 0)
            if (Constructions[c.Point.X, c.Point.Y - 1] != null)
                neighbors.Add(Constructions[c.Point.X, c.Point.Y - 1]);

        return neighbors;
    }

    List<Point> TraceBackPath(Point[,] cameFrom, Node current)
    {
        var path = new List<Point>();
        var p = current.Point;
        path.Add(p);
        while (cameFrom[p.X, p.Y] != null)
        {
            p = cameFrom[p.X, p.Y];
            path.Add(p);
        }
        return path;
    }

    internal class SearchParameter
    {
        public Point Start { get; }
        public Point Target { get; }
        public List<Point> Path { get; set; }
        public float Speed { get; }
        public float WaitAtTheEnd { get; }
        public bool AvoidCities { get; }
        public bool OnlyRoads { get; }

        public SearchParameter(Point start, Point target, float speed, float waitAtTheEnd, bool avoidCities, List<Point> path, bool onlyRoads = false)
        {
            Start = start;
            Target = target;
            Speed = speed;
            Path = path;
            WaitAtTheEnd = waitAtTheEnd;
            AvoidCities = avoidCities;
            OnlyRoads = onlyRoads;
        }
    }

    IEnumerator SearchPathYield(SearchParameter parameters)
    {
        visualSearchInProgress = true;

        var start = parameters.Start;
        var target = parameters.Target;

        var startNode = new Node(this, start, 0f, 0);
        var targetNode = new Node(this, target, 0f, target.ManhattanDistance(start));


        var closed = new List<Node>();
        var opened = new List<Node>
        {
            startNode
        };
        var cameFrom = new Point[(int)width, (int)height];

        while (opened.Count > 0)
        {

            opened.ForEach(o => Terrains[o.Point.X, o.Point.Y].SendMessage("MakeBlue"));
            closed.ForEach(o => Terrains[o.Point.X, o.Point.Y].SendMessage("MakeRed")); ;

            opened.Sort();
            Node n = opened.First();
            if (n.Point.ManhattanDistance(target) <= 0)
            {
                cameFrom[target.X, target.Y] = n.Point;
                Terrains[n.Point.X, n.Point.Y].SendMessage("MakeRed");
                parameters.Path = TraceBackPath(cameFrom, n);
                if (parameters.WaitAtTheEnd > 0f)
                {
                    yield return new WaitForSeconds(parameters.WaitAtTheEnd);
                    visualSearchInProgress = false;
                }
                else
                {
                    visualSearchInProgress = false;
                    yield break;
                }

            }

            opened.Remove(n);
            closed.Add(n);

            var neighbors = n.Neighbors(parameters.AvoidCities, parameters.OnlyRoads);
            foreach (Node neighbor in neighbors)
            {
                if (closed.Contains(neighbor))
                    continue;
                opened.Add(neighbor);
                float tentative = n.Cost + n.Point.ManhattanDistance(neighbor.Point);
                if (tentative >= neighbor.Cost)
                    continue;
                cameFrom[neighbor.Point.X, neighbor.Point.Y] = n.Point;
                neighbor.Score(tentative);
                neighbor.Distance((int)Mathf.Round(neighbor.Cost) + neighbor.Point.ManhattanDistance(target));
            }
            if (parameters.Speed > 0f)
                yield return new WaitForSeconds(parameters.Speed);
            else
                yield return null;
        }
        visualSearchInProgress = false;
        yield return null;
    }

    List<Point> SearchPath(SearchParameter parameters)
    {
        var start = parameters.Start;
        var target = parameters.Target;

        var startNode = new Node(this, start, 0f, 0);
        var targetNode = new Node(this, target, 0f, target.ManhattanDistance(start));


        var closed = new List<Node>();
        var opened = new List<Node>
        {
            startNode
        };
        var cameFrom = new Point[(int)width, (int)height];

        while (opened.Count > 0)
        {
            opened.Sort();
            Node n = opened.First();
            if (n.Point.ManhattanDistance(target) <= 0)
            {
                cameFrom[target.X, target.Y] = n.Point;
                parameters.Path = TraceBackPath(cameFrom, n);
                return parameters.Path;
            }

            opened.Remove(n);
            closed.Add(n);

            var neighbors = n.Neighbors(parameters.AvoidCities, parameters.OnlyRoads);
            foreach (Node neighbor in neighbors)
            {
                if (closed.Contains(neighbor))
                    continue;
                opened.Add(neighbor);
                float tentative = n.Cost + n.Point.ManhattanDistance(neighbor.Point);
                if (tentative >= neighbor.Cost)
                    continue;
                cameFrom[neighbor.Point.X, neighbor.Point.Y] = n.Point;
                neighbor.Score(tentative);
                neighbor.Distance((int)Mathf.Round(neighbor.Cost) + neighbor.Point.ManhattanDistance(target));
            }
        }
        parameters.Path = null;
        return null;
    }

    void VisualSearchPath()
    {
        var firstCity = cities.First();
        var targetCity = FarestCity(firstCity);
        //Debug.Log("Path between " + firstCity.Name + " and " + targetCity.Name);
        var path = new List<Point>();
        var searchParameter = new SearchParameter(firstCity.Point, targetCity.Point, 0.2f, 5f, false, path);

        StartCoroutine("SearchPathYield", searchParameter);
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
