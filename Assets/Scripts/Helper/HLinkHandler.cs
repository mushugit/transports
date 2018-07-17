using System.Collections;
using System.Collections.Generic;

public class HLinkHandler : ILinkable
{
    public delegate void LinkUpdatedDelegate();
    public delegate void LinkUpdateTextDelegate(string message);
    public delegate IEnumerator DoLinkCoroutine(IEnumerable<Cell> path);
    public delegate IEnumerator UpdateLinkCoroutine(ILinkable a, ILinkable b);

    public List<ILinkable> Linked { get; private set; }
    public List<ILinkable> Unreachable { get; private set; }

    public Cell _Cell { get; private set; }

    private readonly LinkUpdatedDelegate linkUpdatedDelegate;

    public HLinkHandler(Cell parent, LinkUpdatedDelegate linkUpdated)
    {
        linkUpdatedDelegate = linkUpdated;
        _Cell = parent;

        Linked = new List<ILinkable>();
        Unreachable = new List<ILinkable>();
    }

    public void ClearLinks()
    {
        Linked.Clear();
        Unreachable.Clear();
    }

    public void AddUnreachable(ILinkable c)
    {
        if (!Unreachable.Contains(c))
        {
            Unreachable.Add(c);
            linkUpdatedDelegate?.Invoke();
        }
    }

    public void AddUnreachable(List<ILinkable> list)
    {
        var addedACity = false;
        foreach (ILinkable c in list)
        {
            if (!Unreachable.Contains(c))
            {
                addedACity = true;
                Unreachable.Add(c);
            }
        }
        if (addedACity)
            linkUpdatedDelegate?.Invoke();
    }

    public static IEnumerator UpdateUnreachable(ILinkable a, ILinkable b)
    {
        var allAUnreachable = new List<ILinkable>(a.Unreachable) { a };
        var allBUnreachable = new List<ILinkable>(b.Unreachable) { b };

        foreach (ILinkable c in a.Unreachable)
            c.AddUnreachable(allBUnreachable);
        a.AddUnreachable(allBUnreachable);
        foreach (ILinkable c in b.Unreachable)
            c.AddUnreachable(allAUnreachable);
        b.AddUnreachable(allAUnreachable);
        yield return null;
    }

    public void AddLinkTo(ILinkable c)
    {
        if (!Linked.Contains(c))
        {
            Linked.Add(c);
            linkUpdatedDelegate?.Invoke();
        }
    }

    public void AddLinkTo(List<ILinkable> list)
    {
        var addedACity = false;
        foreach (ILinkable c in list)
        {
            if (!Linked.Contains(c))
            {
                addedACity = true;
                Linked.Add(c);
            }
        }
        if (addedACity)
            linkUpdatedDelegate?.Invoke();
    }

    public bool IsUnreachable(ILinkable c)
    {
        return Unreachable.Contains(c);
    }

    public bool IsLinkedTo(ILinkable c)
    {
        return Linked.Contains(c);
    }

    public int RoadInDirection(Cell target)
    {

        var values = new int[9, 9];
        var multiplier = new int[9, 9];


        if (target.X > _Cell.X)
        {
            for (int y = 0; y < 3; y++)
            {
                values[1, y] += 1;
                values[2, y] += 2;
            }
        }
        if (_Cell.X < target.X)
        {
            for (int y = 0; y < 3; y++)
            {
                values[1, y] += 1;
                values[0, y] += 2;
            }
        }
        if (target.X == _Cell.X)
        {
            for (int y = 0; y < 3; y++)
            {
                multiplier[1, y] = 2;
            }
        }

        if (target.Y > _Cell.Y)
        {
            for (int x = 0; x < 3; x++)
            {
                values[x, 1] += 1;
                values[x, 2] += 2;
            }
        }
        if (_Cell.Y < target.Y)
        {
            for (int x = 0; x < 3; x++)
            {
                values[x, 1] += 1;
                values[x, 0] += 2;
            }
        }
        if (target.Y == _Cell.Y)
        {
            for (int x = 0; x < 3; x++)
            {
                multiplier[x, 1] = 2;
            }
        }

        var g = 0;
        values[1, 1] = 0;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                g += values[i, j] * ((multiplier[i, j] != 0) ? multiplier[i, j] : 1);
            }
        }
        return g;
    }

    public int ManhattanDistance(IHasCell cell)
    {
        return _Cell.ManhattanDistance(cell);
    }

    public double FlyDistance(IHasCell cell)
    {
        return _Cell.FlyDistance(cell);
    }

    public static IEnumerator Link(IPathable<Cell> a, IPathable<Cell> b, LinkUpdateTextDelegate callback,
        DoLinkCoroutine linkCoroutine, UpdateLinkCoroutine updateCoroutine)
    {
        if (!a.IsLinkedTo(b))
        {
            if ((a.Linked == null && b.Linked == null) || (a.Linked != null && b.Linked != null))
            {
                if (a.RoadInDirection(b._Cell) < b.RoadInDirection(a._Cell))
                {
                    var c = a;
                    a = b;
                    b = c;
                }
            }
            else
            {
                if (a.Linked == null)
                {
                    var c = a;
                    a = b;
                    b = c;
                }
            }

            if (a is IHasName && b is IHasName)
            {
                var aNamed = a as IHasName;
                var bNamed = b as IHasName;
                callback("Relie " + aNamed.Name + " vers " + bNamed.Name);
            }
            else
                callback("Relie");

            var pf = new Pathfinder<Cell>(0, 0, null);
            yield return World.Instance?.StartCoroutine(pf.RoutineFindPath(a._Cell, b._Cell));
            if (pf.Path != null && pf.Path.TotalCost > 0)
            {
                //UnityEngine.Debug.Log($"Path from {a.Name} to {b.Name}");
                yield return World.Instance?.StartCoroutine(linkCoroutine(pf.Path)); // BuildRoads(pf.Path));
                yield return World.Instance?.StartCoroutine(updateCoroutine(a, b)); // UpdateLink(a, b));
            }
            else
            {
                //UnityEngine.Debug.Log($"NO PATH from {a.Name} to {b.Name} ({pf.Path})");
            }
        }
    }

    public static IEnumerator UpdateLink(ILinkable a, ILinkable b)
    {
        var allALink = new List<ILinkable>(a.Linked) { a };
        var allBLink = new List<ILinkable>(b.Linked) { b };

        foreach (ILinkable c in a.Linked)
            c.AddLinkTo(allBLink);
        a.AddLinkTo(allBLink);
        foreach (ILinkable c in b.Linked)
            c.AddLinkTo(allALink);
        b.AddLinkTo(allALink);
        yield return null;
    }

    public static bool IsConnectable(int direction, Construction c)
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
}

