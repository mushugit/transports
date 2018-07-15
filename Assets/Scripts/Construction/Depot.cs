using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.EventSystems;

[JsonObject(MemberSerialization.OptIn)]
public class Depot : Construction, IHasColor
{
	[JsonProperty]
	public int Direction { get; }

    #region IHasColor
    [JsonProperty]
    public float ColorR { get { return color.r; } }
    [JsonProperty]
    public float ColorG { get { return color.g; } }
    [JsonProperty]
    public float ColorB { get { return color.b; } }
    [JsonProperty]
    public float ColorA { get { return color.a; } }
    private Color color = Color.black;
    public Color Color
    {
        get
        {
            if (color != Color.black)
                return color;
            else
            {
                var internalRenderer = GlobalRenderer?.GetComponentInChildren<Renderer>();
                if (internalRenderer != null)
                    return internalRenderer.material.color;
                else return Color.black;
            }
        }
    }
    public void SetColor(Color color)
    {
        var renderers = GlobalRenderer?.GetComponentsInChildren<Renderer>();
        if (renderers != null)
        {
            foreach (Renderer r in renderers)
            {
                r.material.color = color;
            }
        }
    }
    #endregion

    public Depot(Cell cell, int direction)
        : base(cell, World.Instance?.DepotPrefab, World.Instance?.DepotContainer)
	{				
		Direction = direction;
	}

    public override void ClickHandler(PointerEventData eventData)
    {
        
    }
}

