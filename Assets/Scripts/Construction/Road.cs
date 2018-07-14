using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Road : Construction
{
    #region Constructor
    [JsonConstructor]
    public Road(Cell cell)
        : base(cell, World.Instance.RoadPrefab, World.Instance.RoadContainer)
    {

    }
    #endregion

    public override void ClickHandler(PointerEventData eventData)
    {
        
    }

    public void UpdateConnexions(bool north, bool east, bool south, bool west)
    {
        //Debug.Log("N=" + north + " E=" + east + " S=" + south + " E=" + east + " W=" + west);
        var roadRenderer = GlobalRenderer.GetComponentInChildren<RoadRenderer>();
        roadRenderer.SetRoadNorth(north);
		roadRenderer.SetRoadEast(east);
		roadRenderer.SetRoadSouth(south);
		roadRenderer.SetRoadWest(west);

		roadRenderer.UpdateRender();
    }
}
