using UnityEngine;

class HUnityRenderer : Object, IUnityRenderer
{
    private static HUnityRenderer instance;

    public static HUnityRenderer UnityRenderer
    {
        get { return instance ?? (instance = new HUnityRenderer()); }
    }

    private HUnityRenderer() { }

    #region IUnityRenderer
    public Component Build(Vector3 position, Component prefab, Transform container)
    {
        return Instantiate(prefab, position, Quaternion.identity, container);
    }
    #endregion
}

