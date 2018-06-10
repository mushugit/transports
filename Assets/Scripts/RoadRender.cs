using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadRender : MonoBehaviour {

    public GameObject road_N;
    public GameObject road_E;
    public GameObject road_S;
    public GameObject road_W;

    public GameObject turn_NE;
    public GameObject turn_NW;
    public GameObject turn_SE;
    public GameObject turn_SW;

    public GameObject roundabout;

    bool NorthNeightbor { get; set; } = false;
    bool EastNeightbor { get; set; } = false;
    bool SouthNeightbor { get; set; } = false;
    bool WestNeightbor { get; set; } = false;

    public static Component Build(Vector3 position, Component roadPrefab)
    {
        return Instantiate(roadPrefab, position, Quaternion.identity);
    }

    void Start () {   
    }
	
	void SetRoadNorth(bool isNorth)
    {
        NorthNeightbor = isNorth;
    }

    void SetRoadEast(bool isEast)
    {
        EastNeightbor = isEast;
    }

    void SetRoadSouth(bool isSouth)
    {
        SouthNeightbor = isSouth;
    }

    void SetRoadWest(bool isWest)
    {
        WestNeightbor = isWest;
    }

    void Update () {
        road_N.SetActive(NorthNeightbor);
        road_E.SetActive(EastNeightbor);
        road_S.SetActive(SouthNeightbor);
        road_W.SetActive(WestNeightbor);

        int countNeightbors = 0;

        if (NorthNeightbor) countNeightbors++;
        if (EastNeightbor) countNeightbors++;
        if (SouthNeightbor) countNeightbors++;
        if (WestNeightbor) countNeightbors++;

        turn_NE.SetActive(false);
        turn_NW.SetActive(false);
        turn_SE.SetActive(false);
        turn_SW.SetActive(false);

        if (countNeightbors == 2)
        {
            if(NorthNeightbor && !SouthNeightbor)
            {
                turn_NE.SetActive(EastNeightbor);
                turn_NW.SetActive(WestNeightbor);
            }
            if(SouthNeightbor && !NorthNeightbor)
            {
                turn_SE.SetActive(EastNeightbor);
                turn_SW.SetActive(WestNeightbor);
            }
        }

        roundabout.SetActive(countNeightbors >= 3);
    }
}
