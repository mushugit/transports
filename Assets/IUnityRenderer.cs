using UnityEngine;

public interface IUnityRenderer
{
    Component Build(Vector3 position, Component prefab, Transform container);
}

