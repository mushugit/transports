using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class World : MonoBehaviour
{
    public Component cellPrefab;
    public Component cityPrefab;
    public Component[] roadPrefabs;

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
        if (Input.GetButton("Reload") || !visualSearchInProgress)
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
        //Roads(w,h);

        for (float x = 0f; x < width; x++)
        {
            for (float y = 0f; y < height; y++)
            {
                Terrains[(int)x, (int)y] = Terrain(x, y);
            }
        }
        VisualSearchPath();
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

    void Roads(int w, int h)
    {
        var quantity = 10;
        var n = 0;
        while (n < quantity)
        {
            int x = Random.Range(0, w);
            int y = Random.Range(0, h);
            if (Constructions[x, y] == null)
            {
                Road r = new Road(x, y, roadPrefabs);
                Constructions[x, y] = r;
                n++;
            }
        }
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
        public List<Point> Path { get; set;  }
        public float Speed { get; }
        public float WaitAtTheEnd { get; }
        public bool AvoidCities { get; }

        public SearchParameter(Point start, Point target, float speed, float waitAtTheEnd, bool avoidCities, List<Point> path)
        {
            Start = start;
            Target = target;
            Speed = speed;
            Path = path;
            WaitAtTheEnd = waitAtTheEnd;
            AvoidCities = avoidCities;
        }
    }

    IEnumerator SearchPathYield(SearchParameter searchParameter)
    {
        visualSearchInProgress = true;

        var start = searchParameter.Start;
        var target = searchParameter.Target;

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
                searchParameter.Path = TraceBackPath(cameFrom, n);
                if (searchParameter.WaitAtTheEnd > 0f)
                {
                    yield return new WaitForSeconds(searchParameter.WaitAtTheEnd);
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

            var neighbors = n.Neighbors(searchParameter.AvoidCities);
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
            if (searchParameter.Speed > 0f)
                yield return new WaitForSeconds(searchParameter.Speed);
            else
                yield return null;
        }
        visualSearchInProgress = false;
        yield return null;
    }

    List<Point> SearchPath(SearchParameter parameter)
    {
        var start = parameter.Start;
        var target = parameter.Target;

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
            if (n.Point.ManhattanDistance(cities.Last().Point) <= 0)
            {
                cameFrom[target.X, target.Y] = n.Point;
                return TraceBackPath(cameFrom, n);
            }

            opened.Remove(n);
            closed.Add(n);

            var neighbors = n.Neighbors(parameter.AvoidCities);
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

        return null;
    }

    void VisualSearchPath()
    {
        var firstCity = cities.First();
        var targetCity = FarestCity(firstCity);
        Debug.Log("Path between " + firstCity.Name + " and " + targetCity.Name);
        var path = new List<Point>();
        var searchParameter = new SearchParameter(firstCity.Point, targetCity.Point, 0.2f, 1f,false, path);

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
