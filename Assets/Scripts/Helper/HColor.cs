using Newtonsoft.Json;
using UnityEngine;

public class HColor : IHasColor
{
    private Construction parent;

    #region IHasColor
    [JsonProperty]
    public float ColorR { get { return Color.r; } }
    [JsonProperty]
    public float ColorG { get { return Color.g; } }
    [JsonProperty]
    public float ColorB { get { return Color.b; } }
    [JsonProperty]
    public float ColorA { get { return Color.a; } }

    public Color InternalColor { get; private set; } = Color.black;
    public Color Color
    {
        get
        {
            if (InternalColor != Color.black)
                return InternalColor;
            else
            {
                var internalRenderer = parent.GlobalRenderer?.GetComponentInChildren<Renderer>();
                if (internalRenderer != null)
                    return internalRenderer.material.color;
                else return Color.black;
            }
        }
    }
    public void SetColor(Color color)
    {
        this.InternalColor = color;
        var renderers = new Renderer[0];
        var objectRenderer = parent.GlobalRenderer?.GetComponentInChildren<IUnityObjectRenderer>();
        if (objectRenderer != null)
        {
            if (objectRenderer is Component)
            {
                var component = objectRenderer as Component;
                renderers = component.GetComponentsInChildren<Renderer>();
            }
        }
        else
            renderers = parent.GlobalRenderer?.GetComponentsInChildren<Renderer>();
        if (renderers != null)
        {
            foreach (Renderer r in renderers)
            {
                r.material.color = color;
            }
        }
    }
    #endregion

    public HColor(Construction parent)
    {
        this.parent = parent;
    }

    public static readonly string NoColorChangeTag = "NoColorChange";

    public static void GetInitialColor(Construction construction, Component p)
    {
        var c = Random.ColorHSV(0, 1, 1, 1, 0.49f, 0.51f);
        if (construction != null && !construction.IsOriginal)
        {
            if (construction is IHasColor)
                c = (construction as IHasColor).Color;
        }
        else
        {
            while (c == Color.black)
            {
                c = Random.ColorHSV();
            }
        }

        var renderers = p.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.material.color = c;
        }
    }
}

