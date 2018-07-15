using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.EventSystems;

[JsonObject(MemberSerialization.OptIn)]
public class Depot : Construction, IHasColor
{
    private HColor colorHandler;

    [JsonProperty]
	public int Direction { get; }

    #region IHasColor
    [JsonProperty]
    public float ColorR { get { return colorHandler.ColorR; } }
    [JsonProperty]
    public float ColorG { get { return colorHandler.ColorG; } }
    [JsonProperty]
    public float ColorB { get { return colorHandler.ColorB; } }
    [JsonProperty]
    public float ColorA { get { return colorHandler.ColorA; } }

    public Color Color { get { return colorHandler.Color; } }

    public void SetColor(Color color)
    {
        colorHandler.SetColor(color);
    }
    #endregion

    #region Constructor

    public Depot(Cell cell, int direction)
        : base(cell, World.Instance?.DepotPrefab, World.Instance?.DepotContainer, direction)
	{
        colorHandler = new HColor(this);
        Direction = direction;
	}

    [JsonConstructor]
    public Depot(Cell cell, int direction, float colorR, float colorG, float colorB, float colorA)
         : base(cell, World.Instance?.DepotPrefab, World.Instance?.DepotContainer, direction)
    {
        colorHandler = new HColor(this);
        Direction = direction;

        IsOriginal = false;

        SetColor(new Color(colorR, colorG, colorB, colorA));
    }

    public Depot(Depot dummy)
        : base(dummy._Cell, World.Instance?.DepotPrefab, World.Instance?.DepotContainer, dummy.Direction)
    {
        colorHandler = new HColor(this);
        Direction = dummy.Direction;

        IsOriginal = false;

        SetColor(dummy.Color);
    }
    #endregion

    public override void ClickHandler(PointerEventData eventData)
    {
        
    }
}

