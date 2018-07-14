using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.EventSystems;

[JsonObject(MemberSerialization.OptIn)]
public class Depot : Construction
{
	[JsonProperty]
	public int Direction { get; }

	public Depot(Cell cell, int direction)
        : base(cell, World.Instance?.DepotPrefab, World.Instance?.DepotContainer)
	{				
		Direction = direction;
	}

    public override void ClickHandler(PointerEventData eventData)
    {
        
    }
}

