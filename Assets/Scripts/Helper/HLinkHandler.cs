using System.Collections.Generic;

public class HLinkHandler : ILinkable
{
    public delegate void LinkUpdatedDelegate();

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
}

