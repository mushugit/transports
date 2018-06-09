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

    public float width = 10f;
    public float height = 10f;

    public Construction[,] Constructions { get; private set; }
    public Component[,] Terrains { get; private set; }
    private List<City> cities;

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
        Generate();
        StartCoroutine("Simulation");
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
        StartCoroutine("SearchPath");
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

    IEnumerator SearchPath()
    { //Point start, Point target
        Debug.Log("Path between " + cities.First().Name + " and " + cities.Last().Name);
        var startNode = new Node(this, cities.First().Point, 0f, 0);
        var targetNode = new Node(this, cities.Last().Point, 0f, cities.Last().Point.ManhattanDistance(cities.First().Point));
        var closed = new List<Node>();
        var opened = new List<Node>
        {
            startNode
        };
        var cameFrom = new Point[(int)width, (int)height];

        while (opened.Count > 0)
        {
            /*Debug.Log("Opened=");
            opened.ForEach(o=>Debug.Log(o.p.ToString()));
            Debug.Log("Closed=");
            closed.ForEach(o=>Debug.Log(o.p.ToString()));*/

            opened.ForEach(o => Terrains[o.Point.X, o.Point.Y].SendMessage("MakeBlue"));
            closed.ForEach(o => Terrains[o.Point.X, o.Point.Y].SendMessage("MakeRed")); ;


            //opened.OrderBy(x => x.h).ToList().ForEach(o=>Debug.Log(o.p.ToString()));
            opened.Sort();
            Node n = opened.First();
            if (n.Point.ManhattanDistance(cities.Last().Point) <= 0)
            {
                cameFrom[cities.Last().Point.X, cities.Last().Point.Y] = n.Point;
                Terrains[n.Point.X, n.Point.Y].SendMessage("MakeRed");
                var path = TraceBackPath(cameFrom, n);
                /*Debug.Log("Found !");
                foreach (Point p in path)
                {
                    Debug.Log(p.ToString());

                }*/
                yield return new WaitForSeconds(1f);
                ReloadLevel();
                yield return null;
            }

            opened.Remove(n);
            closed.Add(n);

            var neighbors = n.Neighbors();
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
                neighbor.Distance((int)Mathf.Round(neighbor.Cost) + neighbor.Point.ManhattanDistance(cities.Last().Point));
            }
            yield return new WaitForSeconds(0.3f);
        }

        Debug.Log("No path found !");
        yield return null;
    }


}
