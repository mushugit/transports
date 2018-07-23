using System.Collections.Generic;
using UnityEngine;

public class HConstructionPattern
{
    public const int MinCityDistance = 4;

    private int _blocSizeWidth;
    private int _blocSizeHeight;

    private int _width;
    private int _height;

    private World _world;

    public HConstructionPattern(World world)
    {
        _world = world;
        _width = (int)World.width;
        _height = (int)World.height;
        _blocSizeWidth = FindBlockSize(_width);
        _blocSizeHeight = FindBlockSize(_height);
    }

    private int FindBlockSize(int length)
    {
        var blocSize = 3;
        for (int m = 4; m >= 2; m--)
        {
            if ((length - 1) % (m + 1) == 0)
            {
                blocSize = m;
            }
        }
        return blocSize;
    }

    private int CountBloc(int length, int blocSize)
    {
        return Mathf.FloorToInt(length - 1) / (blocSize + 1);
    }

    public int CountHorizontalBloc()
    {
        return CountBloc(_width, _blocSizeWidth);
    }

    public int CountVerticalBloc()
    {
        return CountBloc(_height, _blocSizeHeight);
    }

    public bool IsRoad(int x, int y)
    {
        return (x % (_blocSizeWidth + 1) == 0 || y % (_blocSizeHeight + 1) == 0);
    }

    public bool IsBuilding(int x, int y)
    {
        return !IsRoad(x, y);
    }

    public bool IsConnectable(int x, int y)
    {
        return (IsRoad(x, y) || IsRoad(x - 1, y) || IsRoad(x + 1, y) || IsRoad(x, y - 1) || IsRoad(x, y + 1));
    }

    public List<Cell> InitialCitySpots()
    {
        var list = new List<Cell>();

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (IsConnectable(x, y))
                {
                    list.Add(new Cell(x, y));
                }
            }
        }

        return list;
    }

    public List<Cell> RemoveAround(List<Cell> list, Cell cell, int removeDistance)
    {
        list.RemoveAll(cellItem => cell.ManhattanDistance(cellItem) <= removeDistance);
        return list;
    }
}
