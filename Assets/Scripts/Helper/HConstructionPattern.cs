using UnityEngine;

public class HConstructionPattern
{
    private int _blocSizeWidth;
    private int _blocSizeHeight;

    private int _width;
    private int _height;

    private bool[,] _roadPattern;
    private bool[,] _buildingPattern;
    private bool[,] _connectablePattern;

    public bool[,] RoadPattern
    {
        get
        {
            return (_roadPattern ?? (_roadPattern = GenerateRoadPattern()));
        }
    }

    public bool[,] BuildingPattern
    {
        get
        {
            return (_buildingPattern ?? (_buildingPattern = GenerateBuildingPattern()));
        }
    }

    public bool[,] ConnectablePattern
    {
        get
        {
            return (_connectablePattern ?? (_connectablePattern = GenerateConnectablePattern()));
        }
    }


    public HConstructionPattern(int width, int height)
    {
        _width = width;
        _height = height;
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

    public bool[,] GenerateRoadPattern()
    {
        var _map = new bool[_width, _height];

        for (int x = 0; x < _width; x += (_blocSizeWidth + 1))
        {
            for (int y = 0; y < _height; y++)
            {
                _map[x, y] = true;
            }
        }
        for (int y = 0; y < _height; y += (_blocSizeHeight + 1))
        {
            for (int x = 0; x < _width; x++)
            {
                _map[x, y] = true;
            }

        }
        return _map;
    }

    public bool[,] GenerateBuildingPattern()
    {
        var map = (bool[,])RoadPattern.Clone();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                map[x, y] = !map[x, y];
            }
        }
        return map;
    }

    public bool[,] GenerateConnectablePattern()
    {
        var _map = new bool[_width, _height];

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _map[x, y] = IsConnectable(x, y);
            }
        }
        return _map;
    }
}
