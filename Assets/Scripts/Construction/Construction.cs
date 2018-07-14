using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.EventSystems;

[JsonObject(MemberSerialization.OptIn)]
public abstract class Construction
{
    public delegate void ClickCallbackDelegate(PointerEventData eventData);

    [JsonProperty]
    public Cell _Cell { get; protected set; }

    public Component GlobalRenderer;

    public ClickCallbackDelegate ClickCallback;

    protected Construction(Cell cell, Component prefab, Transform container)
    {
        _Cell = cell;

        ClickCallback = ClickHandler;

        if (prefab != null)
        {
            GlobalRenderer = HUnityRenderer.UnityRenderer.Build(new Vector3(_Cell.X, 0f, _Cell.Y), prefab, container);

            var objectRenderer = GlobalRenderer.GetComponentInChildren<IUnityObjectRenderer>();
            if(objectRenderer!=null)
                objectRenderer.NestedConstruction = this;
        }
    }

    public abstract void ClickHandler(PointerEventData eventData);

    public virtual void Destroy()
    {
        GameObject.Destroy(GlobalRenderer.gameObject);
    }

}
