using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[JsonObject(MemberSerialization.OptIn)]
public class Road : Construction
{
    public override string BuildOperation { get { return "build_road"; } }
    public override string DestroyOperation { get { return "destroy_road"; } }
    public override string BuildLabel { get { return "construire une route"; } }
    public override string DestroyLabel { get { return "détruire une route"; } }

    #region Constructor
    [JsonConstructor]
    public Road(Cell cell)
        : base(cell, World.Instance?.RoadPrefab, World.Instance?.RoadContainer)
    {

    }
    #endregion

    public override void ClickHandler(PointerEventData eventData)
    {
        
    }

    private void UpdateConnexions(bool north, bool east, bool south, bool west)
    {
        //Debug.Log("N=" + north + " E=" + east + " S=" + south + " E=" + east + " W=" + west);
        var roadRenderer = GlobalRenderer.GetComponentInChildren<RoadRenderer>();
        roadRenderer.SetRoadNorth(north);
		roadRenderer.SetRoadEast(east);
		roadRenderer.SetRoadSouth(south);
		roadRenderer.SetRoadWest(west);

		roadRenderer.UpdateRender();
    }

    public void UpdateConnections()
    {
        var north = World.Instance?.North(this);
        var east = World.Instance?.East(this);
        var south = World.Instance?.South(this);
        var west = World.Instance?.West(this);

        var isLinkNorth = HLinkHandler.IsConnectable(2, north);
        var isLinkEast = HLinkHandler.IsConnectable(3, east);
        var isLinkSouth = HLinkHandler.IsConnectable(0, south);
        var isLinkWest = HLinkHandler.IsConnectable(1, west);

        //UnityEngine.Debug.Log($"Update de {r.Point} : n={isLinkNorth} e={isLinkEast} s={isLinkSouth} w={isLinkWest}");
        UpdateConnexions(isLinkNorth, isLinkEast, isLinkSouth, isLinkWest);
    }

    public static void UpdateAllRoad(IEnumerable<Road> list)
    {
        var neighborsRoads = new List<Road>();
        foreach (Road neighborsRoad in list)
        {
            if (!neighborsRoads.Contains(neighborsRoad))
                neighborsRoads.Add(neighborsRoad);
        }

        foreach (Road neighborsRoad in neighborsRoads)
        {
            neighborsRoad.UpdateConnections();
        }
    }

    public static void UpdateRoadArround(Cell c)
    {

    }
}
